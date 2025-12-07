using Microsoft.SemanticKernel;
using Microsoft.Extensions.Logging;

namespace Codamint.Agents
{
    /// <summary>
    /// ツール呼び出しをインターセプトして表示するフィルター
    /// </summary>
    public class ToolCallInterceptionFilter : IFunctionInvocationFilter
    {
        private readonly ILogger<CodingAgent> _logger;

        public ToolCallInterceptionFilter(ILogger<CodingAgent> logger)
        {
            _logger = logger;
        }

        public async Task OnFunctionInvocationAsync(FunctionInvocationContext context, Func<FunctionInvocationContext, Task> next)
        {
            // ツール関数の呼び出し前に情報を出力
            var function = context.Function;
            var arguments = context.Arguments;

            if (function.PluginName != null && function.PluginName != "")
            {
                Console.WriteLine();
                Console.WriteLine($"[Tool Call] {function.PluginName}.{function.Name}");

                // 引数を表示
                if (arguments.Count > 0)
                {
                    Console.WriteLine("Arguments:");
                    foreach (var arg in arguments)
                    {
                        var value = arg.Value?.ToString() ?? "null";
                        if (value.Length > 100)
                        {
                            value = value.Substring(0, 100) + "...";
                        }
                        Console.WriteLine($"  {arg.Key}: {value}");
                    }
                }

                _logger.LogInformation("Calling tool: {PluginName}.{FunctionName}", function.PluginName, function.Name);
            }

            try
            {
                // ツールを実行
                await next(context);

                // ツール実行後に結果を表示
                if (function.PluginName != null && function.PluginName != "")
                {
                    var result = context.Result.ToString();
                    if (result.Length > 200)
                    {
                        result = result.Substring(0, 200) + "...";
                    }
                    Console.WriteLine($"Result: {result}");
                    Console.WriteLine();

                    _logger.LogInformation("Tool executed successfully: {PluginName}.{FunctionName}", function.PluginName, function.Name);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Tool execution failed: {PluginName}.{FunctionName}", function.PluginName, function.Name);
                throw;
            }
        }
    }
}
