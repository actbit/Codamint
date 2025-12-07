using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Binder;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Codamint.Settings;

namespace Codamint.Services
{
    /// <summary>
    /// Semantic Kernel初期化サービス
    /// </summary>
    public class KernelService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<KernelService> _logger;

        public KernelService(IConfiguration configuration, ILogger<KernelService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Semantic Kernelを初期化して返す
        /// </summary>
        public Kernel InitializeKernel()
        {
            // OpenAI設定を取得
            var openAiSettings = _configuration.GetSection("OpenAI").Get<OpenAISettings>();

            if (openAiSettings == null || string.IsNullOrEmpty(openAiSettings.ApiKey))
            {
                throw new InvalidOperationException(
                    "OpenAI settings not configured. Please set the OpenAI:ApiKey in appsettings.json or environment variables.");
            }

            _logger.LogInformation(
                "Initializing Semantic Kernel with model: {Model}, Provider: {Provider}, Endpoint: {Endpoint}",
                openAiSettings.Model,
                openAiSettings.ProviderType,
                openAiSettings.Endpoint);

            // Kernel Builderを使用してカーネルを作成
            var kernelBuilder = Kernel.CreateBuilder();

            // プロバイダータイプに応じて設定
            switch (openAiSettings.ProviderType.ToLower())
            {
                case "azureopenai":
                    InitializeAzureOpenAI(kernelBuilder, openAiSettings);
                    break;

                case "custom":
                    InitializeCustomOpenAI(kernelBuilder, openAiSettings);
                    break;

                case "openai":
                default:
                    InitializeOpenAI(kernelBuilder, openAiSettings);
                    break;
            }

            // Kernel をビルド
            var kernel = kernelBuilder.Build();

            _logger.LogInformation("Semantic Kernel initialized successfully");

            return kernel;
        }

        /// <summary>
        /// 標準 OpenAI API の初期化
        /// </summary>
        private void InitializeOpenAI(IKernelBuilder kernelBuilder, OpenAISettings settings)
        {
            _logger.LogInformation("Configuring standard OpenAI API");

            kernelBuilder.AddOpenAIChatCompletion(
                modelId: settings.Model,
                apiKey: settings.ApiKey,
                endpoint: new Uri(settings.Endpoint)
            );
        }

        /// <summary>
        /// Azure OpenAI の初期化
        /// </summary>
        private void InitializeAzureOpenAI(IKernelBuilder kernelBuilder, OpenAISettings settings)
        {
            _logger.LogInformation("Configuring Azure OpenAI API");

            if (string.IsNullOrEmpty(settings.AzureDeploymentName))
            {
                throw new InvalidOperationException(
                    "AzureDeploymentName must be configured for Azure OpenAI provider");
            }

            kernelBuilder.AddAzureOpenAIChatCompletion(
                deploymentName: settings.AzureDeploymentName,
                endpoint: settings.Endpoint,
                apiKey: settings.ApiKey,
                modelId: settings.Model,
                apiVersion: settings.ApiVersion
            );
        }

        /// <summary>
        /// カスタム OpenAI 互換 API の初期化
        /// </summary>
        private void InitializeCustomOpenAI(IKernelBuilder kernelBuilder, OpenAISettings settings)
        {
            _logger.LogInformation("Configuring custom OpenAI-compatible API at {Endpoint}", settings.Endpoint);

            // カスタムエンドポイント用の設定
            kernelBuilder.AddOpenAIChatCompletion(
                modelId: settings.Model,
                apiKey: settings.ApiKey,
                endpoint: new Uri(settings.Endpoint),
                httpClient: CreateHttpClientWithProxy(settings)
            );
        }

        /// <summary>
        /// プロキシ設定付き HttpClient を作成
        /// </summary>
        private HttpClient CreateHttpClientWithProxy(OpenAISettings settings)
        {
            var handler = new HttpClientHandler();

            if (!string.IsNullOrEmpty(settings.ProxyUrl))
            {
                handler.Proxy = new System.Net.WebProxy(settings.ProxyUrl);
                _logger.LogInformation("Configured HTTP proxy: {ProxyUrl}", settings.ProxyUrl);
            }

            var httpClient = new HttpClient(handler)
            {
                Timeout = TimeSpan.FromSeconds(settings.TimeoutSeconds)
            };

            return httpClient;
        }

        /// <summary>
        /// OpenAI設定を取得
        /// </summary>
        public OpenAISettings GetOpenAISettings()
        {
            var settings = _configuration.GetSection("OpenAI").Get<OpenAISettings>();
            if (settings == null)
            {
                throw new InvalidOperationException("OpenAI settings not found in configuration");
            }
            return settings;
        }

        /// <summary>
        /// エージェント設定を取得
        /// </summary>
        public AgentSettings GetAgentSettings()
        {
            var settings = _configuration.GetSection("Agent").Get<AgentSettings>();
            if (settings == null)
            {
                throw new InvalidOperationException("Agent settings not found in configuration");
            }
            return settings;
        }

        /// <summary>
        /// OpenAI実行設定を作成（null値は含めない）
        /// </summary>
        public PromptExecutionSettings CreateExecutionSettings()
        {
            var openAiSettings = GetOpenAISettings();
            var executionSettings = new PromptExecutionSettings();

            if (openAiSettings.Temperature.HasValue)
            {
                executionSettings.ExtensionData ??= new Dictionary<string, object>();
                executionSettings.ExtensionData["Temperature"] = openAiSettings.Temperature.Value;
            }

            if (openAiSettings.MaxTokens.HasValue)
            {
                executionSettings.ExtensionData ??= new Dictionary<string, object>();
                executionSettings.ExtensionData["MaxTokens"] = openAiSettings.MaxTokens.Value;
            }

            _logger.LogInformation(
                "Execution settings created - Temperature: {Temperature}, MaxTokens: {MaxTokens}",
                openAiSettings.Temperature.HasValue ? openAiSettings.Temperature.ToString() : "API default",
                openAiSettings.MaxTokens.HasValue ? openAiSettings.MaxTokens.ToString() : "API default");

            return executionSettings;
        }
    }
}
