using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Codamint.Plugins;
using Codamint.Services;
using Codamint.Settings;

namespace Codamint.Agents
{
    /// <summary>
    /// メインCoding Agentクラス
    /// </summary>
    public class CodingAgent
    {
        private readonly Kernel _kernel;
        private readonly AgentSettings _settings;
        private readonly ILogger<CodingAgent> _logger;
        private readonly KernelService _kernelService;

        public CodingAgent(Kernel kernel, AgentSettings settings, ILogger<CodingAgent> logger, KernelService kernelService)
        {
            _kernel = kernel;
            _settings = settings;
            _logger = logger;
            _kernelService = kernelService;
            InitializePlugins();
            InitializeFilters();
        }

        /// <summary>
        /// カーネルフィルターを初期化してツール呼び出しをインターセプト
        /// </summary>
        private void InitializeFilters()
        {
            // ツール実行前フィルター
            _kernel.FunctionInvocationFilters.Add(new ToolCallInterceptionFilter(_logger));
        }

        /// <summary>
        /// プロバイダーに応じた実行設定を作成（自動ツール呼び出し有効）
        /// </summary>
        private PromptExecutionSettings CreateExecutionSettingsWithFunctionChoice()
        {
            var executionSettings = new OpenAIPromptExecutionSettings
            {
                FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
            };

            return executionSettings;
        }

        /// <summary>
        /// プラグインを初期化して登録
        /// </summary>
        private void InitializePlugins()
        {
            _logger.LogInformation("Initializing plugins...");

            try
            {
                // 各プラグインをカーネルに追加
                // CodeExecutionPlugin は無効化（C#実行機能を削除）
                // CodeGenerationPlugin は削除（ツール結果のエスケープ問題が発生するため）
                // CodeAnalysisPlugin は削除（不要な呼び出しが多いため）
                _kernel.Plugins.AddFromType<PowerShellPlugin>("PowerShell");
                _kernel.Plugins.AddFromType<FileOperationPlugin>("FileOperation");

                _logger.LogInformation("All plugins initialized successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing plugins");
                throw;
            }
        }

        /// <summary>
        /// ユーザープロンプトを処理して実行
        /// </summary>
        public async Task<string> ProcessPromptAsync(string userPrompt)
        {
            try
            {
                _logger.LogInformation("Processing user prompt: {Prompt}", userPrompt);

                var systemPrompt = "You are a helpful coding assistant. Use available functions to complete tasks. " +
                    "FileOperation functions: ReadFile, WriteFile, ListFiles, DeleteFile, CreateDirectory, GetFileInfo, AppendFile, GetCurrentDirectory. " +
                    "PowerShell functions: ExecutePowerShellCommand, ExecutePowerShellScript, GetPowerShellCmdlets. " +
                    "Call functions ONCE to complete requests. Do not make multiple sequential function calls. Respond in the user's language.";

                var fullPrompt = systemPrompt + " User Request: " + userPrompt;

                // プロバイダーに応じた実行設定を作成
                var executionSettings = CreateExecutionSettingsWithFunctionChoice();

                var arguments = new KernelArguments();
                arguments.ExecutionSettings = new Dictionary<string, PromptExecutionSettings>
                {
                    { "default", executionSettings }
                };

                var result = await _kernel.InvokePromptAsync(fullPrompt, arguments);
                var response = result.ToString();

                _logger.LogInformation("Prompt processed successfully");
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing prompt");
                return $"Error processing your request: {ex.Message}";
            }
        }

        /// <summary>
        /// コード生成タスクを実行
        /// </summary>
        public async Task<string> GenerateCodeAsync(string requirements, string language = "C#")
        {
            try
            {
                _logger.LogInformation("Generating {Language} code for: {Requirements}", language, requirements);

                var prompt = $"Generate {language} code based on: {requirements}";
                var result = await _kernel.InvokePromptAsync(prompt);
                return result.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating code");
                return $"Error generating code: {ex.Message}";
            }
        }

        /// <summary>
        /// コード分析タスクを実行
        /// </summary>
        public async Task<string> AnalyzeCodeAsync(string code)
        {
            try
            {
                _logger.LogInformation("Analyzing code");

                var prompt = $@"Analyze the following code for bugs, issues, and improvements:

Code:
{code}

Provide detailed analysis with suggestions for improvement.";

                var result = await _kernel.InvokePromptAsync(prompt);
                return result.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing code");
                return $"Error analyzing code: {ex.Message}";
            }
        }

        /// <summary>
        /// ファイル読み込みタスクを実行
        /// </summary>
        public async Task<string> ReadFileAsync(string filePath)
        {
            try
            {
                _logger.LogInformation("Reading file: {FilePath}", filePath);

                if (!File.Exists(filePath))
                {
                    return $"Error: File not found - {filePath}";
                }

                var content = await File.ReadAllTextAsync(filePath);
                return content;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading file");
                return $"Error reading file: {ex.Message}";
            }
        }

        /// <summary>
        /// ファイル書き込みタスクを実行
        /// </summary>
        public async Task<string> WriteFileAsync(string filePath, string content)
        {
            try
            {
                _logger.LogInformation("Writing file: {FilePath}", filePath);

                var directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                await File.WriteAllTextAsync(filePath, content);
                return $"File written successfully: {filePath}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error writing file");
                return $"Error writing file: {ex.Message}";
            }
        }

        /// <summary>
        /// エージェント情報を取得
        /// </summary>
        public string GetAgentInfo()
        {
            return $@"
=== {_settings.Name} ===
Description: {_settings.Description}
Model: {_kernel}
File Operations: {(_settings.EnableFileOperations ? "Enabled" : "Disabled")}
Code Execution: {(_settings.EnableCodeExecution ? "Enabled" : "Disabled")}
Max Retries: {_settings.MaxRetries}
Timeout: {_settings.TimeoutSeconds}s
";
        }
    }
}
