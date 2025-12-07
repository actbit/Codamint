using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Codamint.Agents;
using Codamint.Services;
using Codamint.Settings;
using Codamint.Utils;

namespace Codamint
{
    internal class Program
    {
        static async Task<int> Main(string[] args)
        {
            try
            {
                // コマンドライン引数をパース
                var prompt = GetArgValue(args, "--prompt", "-p");
                var configPath = GetArgValue(args, "--config", "-c");
                var interactive = args.Contains("--interactive") || args.Contains("-i");
                var outputFormat = GetArgValue(args, "--output-format", "-o") ?? "text";
                var verbose = args.Contains("--verbose") || args.Contains("-v");
                var help = args.Contains("--help") || args.Contains("-h");

                if (help)
                {
                    PrintHelp();
                    return 0;
                }

                await RunAgentAsync(prompt, configPath, interactive, outputFormat, verbose);
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(OutputFormatter.FormatError($"Fatal error: {ex.Message}"));
                if (Environment.GetEnvironmentVariable("CODAMINT_DEBUG") == "true")
                {
                    Console.WriteLine(ex.StackTrace);
                }
                return 1;
            }
        }

        static string? GetArgValue(string[] args, params string[] names)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (names.Contains(args[i]) && i + 1 < args.Length)
                {
                    return args[i + 1];
                }
            }
            return null;
        }

        static void PrintHelp()
        {
            Console.WriteLine("Codamint - Semantic Kernel Coding Agent");
            Console.WriteLine();
            Console.WriteLine("Usage: dotnet run -- [options]");
            Console.WriteLine();
            Console.WriteLine("Options:");
            Console.WriteLine("  --prompt, -p <text>      Send a prompt to the agent");
            Console.WriteLine("  --interactive, -i         Run in interactive mode");
            Console.WriteLine("  --config, -c <path>       Use custom configuration file");
            Console.WriteLine("  --output-format, -o <fmt> Output format (text/json/markdown)");
            Console.WriteLine("  --verbose, -v             Enable verbose logging");
            Console.WriteLine("  --help, -h                Show this help message");
            Console.WriteLine();
            Console.WriteLine("Examples:");
            Console.WriteLine("  dotnet run -- --prompt \"Generate a C# function\"");
            Console.WriteLine("  dotnet run -- --interactive");
            Console.WriteLine("  dotnet run -- --prompt \"Code review\" --config custom.json");
        }

        private static async Task RunAgentAsync(
            string? prompt,
            string? configPath,
            bool interactive,
            string outputFormat,
            bool verbose)
        {
            try
            {
                // 設定の読み込み
                var configuration = ConfigurationHelper.BuildConfiguration(configPath);

                // DI コンテナのセットアップ
                var services = new ServiceCollection();

                services.AddSingleton(configuration);
                services.AddLogging(builder =>
                {
                    builder.ClearProviders();
                    var logLevel = verbose ? LogLevel.Debug : LogLevel.Information;
                    builder.AddConsole();
                });

                services.AddSingleton<KernelService>();
                services.Configure<OpenAISettings>(options =>
                {
                    configuration.GetSection("OpenAI").Bind(options);
                });
                services.Configure<AgentSettings>(options =>
                {
                    configuration.GetSection("Agent").Bind(options);
                });

                var serviceProvider = services.BuildServiceProvider();
                var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

                logger.LogInformation("Starting Codamint Semantic Kernel Coding Agent");

                // KernelService を使用して初期化
                var kernelService = serviceProvider.GetRequiredService<KernelService>();
                var kernel = kernelService.InitializeKernel();

                var agentSettings = kernelService.GetAgentSettings();
                var agent = new CodingAgent(kernel, agentSettings, serviceProvider.GetRequiredService<ILogger<CodingAgent>>(), kernelService);

                logger.LogInformation($"Agent initialized: {agentSettings.Name}");

                if (interactive)
                {
                    await RunInteractiveModeAsync(agent, outputFormat);
                }
                else if (!string.IsNullOrEmpty(prompt))
                {
                    await RunSinglePromptAsync(agent, prompt, outputFormat);
                }
                else
                {
                    Console.WriteLine(agent.GetAgentInfo());
                    Console.WriteLine("\nUsage:");
                    Console.WriteLine("  --prompt <text>        : Send a prompt to the agent");
                    Console.WriteLine("  --interactive          : Run in interactive mode");
                    Console.WriteLine("  --config <path>        : Use custom configuration file");
                    Console.WriteLine("  --output-format <fmt>  : Output format (text/json/markdown)");
                    Console.WriteLine("  --verbose              : Enable verbose logging");
                    Console.WriteLine("\nExamples:");
                    Console.WriteLine("  dotnet run -- --prompt \"Generate a C# function\"");
                    Console.WriteLine("  dotnet run -- --interactive");
                    Console.WriteLine("  dotnet run -- --prompt \"Code review\" --config custom.json");
                }
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine(OutputFormatter.FormatError($"Configuration error: {ex.Message}"));
                Console.WriteLine("\nPlease ensure OpenAI API key is configured:");
                Console.WriteLine("  Option 1: Set environment variable OPENAI_APIKEY");
                Console.WriteLine("  Option 2: Edit appsettings.json");
                Console.WriteLine("  Option 3: Use custom config file with --config option");
            }
            catch (Exception ex)
            {
                Console.WriteLine(OutputFormatter.FormatError($"Error: {ex.Message}"));
                if (Environment.GetEnvironmentVariable("CODAMINT_DEBUG") == "true")
                {
                    Console.WriteLine(ex.StackTrace);
                }
            }
        }

        private static async Task RunSinglePromptAsync(
            CodingAgent agent,
            string prompt,
            string outputFormat)
        {
            Console.WriteLine(OutputFormatter.FormatWithTimestamp("Processing prompt..."));
            Console.WriteLine();

            var response = await agent.ProcessPromptAsync(prompt);
            var format = Enum.TryParse<OutputFormatter.OutputFormat>(outputFormat, true, out var fmt)
                ? fmt
                : OutputFormatter.OutputFormat.Text;

            var formattedOutput = OutputFormatter.FormatOutput(response, format);
            Console.WriteLine(formattedOutput);
        }

        private static async Task RunInteractiveModeAsync(
            CodingAgent agent,
            string outputFormat)
        {
            Console.WriteLine(agent.GetAgentInfo());
            Console.WriteLine();
            Console.WriteLine("Entering interactive mode. Type 'exit' to quit.");
            Console.WriteLine("-".PadRight(80, '-'));

            var format = Enum.TryParse<OutputFormatter.OutputFormat>(outputFormat, true, out var fmt)
                ? fmt
                : OutputFormatter.OutputFormat.Text;

            while (true)
            {
                Console.Write("> ");
                var userInput = Console.ReadLine();

                if (userInput?.ToLower() == "exit" || userInput?.ToLower() == "quit")
                {
                    Console.WriteLine("Goodbye!");
                    break;
                }

                if (string.IsNullOrWhiteSpace(userInput))
                {
                    continue;
                }

                try
                {
                    var response = await agent.ProcessPromptAsync(userInput);
                    var formattedOutput = OutputFormatter.FormatOutput(response, format);
                    Console.WriteLine(formattedOutput);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(OutputFormatter.FormatError($"Error: {ex.Message}"));
                }
            }
        }
    }
}
