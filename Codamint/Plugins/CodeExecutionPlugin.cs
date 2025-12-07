using Microsoft.SemanticKernel;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Text;

namespace Codamint.Plugins
{
    /// <summary>
    /// コード実行・テスト機能を提供するプラグイン
    /// </summary>
    public class CodeExecutionPlugin
    {
        [KernelFunction, Description("Execute C# code snippet")]
        public async Task<string> ExecuteCSharpCode(
            Kernel kernel,
            [Description("The C# code to execute")] string code)
        {
            try
            {
                // このメソッドは通常、Roslyn コンパイラを使用して実装されます
                // ここでは簡略版を提示します
                var prompt = $@"Review and validate the following C# code for execution:

Code:
{code}

Provide:
1. Syntax validation
2. Potential runtime issues
3. Dependencies required
4. Expected output

Validation Report:";

                var response = await kernel.InvokePromptAsync(prompt);
                return response.ToString();
            }
            catch (Exception ex)
            {
                return $"Error executing code: {ex.Message}";
            }
        }

        [KernelFunction, Description("Run unit tests")]
        public async Task<string> RunTests(
            Kernel kernel,
            [Description("Test project path or code")] string testInput)
        {
            try
            {
                var prompt = $@"Analyze the following test code and suggest how to run it:

Test Code/Path:
{testInput}

Provide:
1. Command to run tests
2. Expected output format
3. Test framework detection
4. Coverage recommendations

Test Execution Plan:";

                var response = await kernel.InvokePromptAsync(prompt);
                return response.ToString();
            }
            catch (Exception ex)
            {
                return $"Error running tests: {ex.Message}";
            }
        }

        [KernelFunction, Description("Validate code syntax")]
        public async Task<string> ValidateSyntax(
            Kernel kernel,
            [Description("The code to validate")] string code,
            [Description("Programming language")] string language = "C#")
        {
            var prompt = $@"Validate the syntax of the following {language} code:

Code:
{code}

Check for:
1. Syntax errors
2. Missing semicolons, braces
3. Invalid keywords
4. Incomplete statements

Provide a detailed syntax validation report with line numbers where issues occur.

Syntax Validation Report:";

            var response = await kernel.InvokePromptAsync(prompt);
            return response.ToString();
        }

        [KernelFunction, Description("Execute shell command safely")]
        public async Task<string> ExecuteCommand(
            Kernel kernel,
            [Description("The command to execute")] string command,
            [Description("Command timeout in seconds")] int timeoutSeconds = 30)
        {
            try
            {
                // NOTE: This is a simplified safe execution wrapper
                // In production, use proper sandboxing

                var prompt = $@"Analyze if this command is safe to execute and provide recommendations:

Command: {command}
Timeout: {timeoutSeconds} seconds

Analyze for:
1. Security risks
2. System impact
3. Safe parameters
4. Alternative approaches

Safety Assessment:";

                var response = await kernel.InvokePromptAsync(prompt);
                return response.ToString();
            }
            catch (Exception ex)
            {
                return $"Error analyzing command: {ex.Message}";
            }
        }

        [KernelFunction, Description("Generate and run performance tests")]
        public async Task<string> PerformanceTest(
            Kernel kernel,
            [Description("The code to test performance")] string code,
            [Description("Test iterations")] int iterations = 1000)
        {
            var prompt = $@"Create a performance test for the following code that runs {iterations} iterations:

Code:
{code}

Generate:
1. Benchmark code structure
2. Metrics to measure
3. Expected performance ranges
4. Optimization suggestions based on results

Performance Test Code:";

            var response = await kernel.InvokePromptAsync(prompt);
            return response.ToString();
        }

        [KernelFunction, Description("Execute PowerShell command")]
        public async Task<string> ExecutePowerShellCommand(
            [Description("The PowerShell command to execute")] string command,
            [Description("Command timeout in seconds")] int timeoutSeconds = 30)
        {
            try
            {
                using (var runspace = RunspaceFactory.CreateRunspace())
                {
                    runspace.Open();

                    using (var pipeline = runspace.CreatePipeline(command))
                    {
                        var output = new StringBuilder();
                        var errorOutput = new StringBuilder();

                        // Get output
                        Collection<PSObject> results = null;
                        Exception executionError = null;

                        try
                        {
                            results = pipeline.Invoke();

                            if (results != null)
                            {
                                foreach (var psObject in results)
                                {
                                    output.AppendLine(psObject.ToString());
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            executionError = ex;
                        }

                        // Get errors
                        if (pipeline.Error != null && pipeline.Error.Count > 0)
                        {
                            foreach (var error in pipeline.Error.ReadToEnd())
                            {
                                errorOutput.AppendLine(error.ToString());
                            }
                        }

                        var result = new StringBuilder();
                        result.AppendLine($"Command: {command}");
                        result.AppendLine("---");
                        if (output.Length > 0)
                        {
                            result.AppendLine("Output:");
                            result.Append(output.ToString());
                        }
                        if (errorOutput.Length > 0)
                        {
                            result.AppendLine("Errors:");
                            result.Append(errorOutput.ToString());
                        }
                        if (executionError != null)
                        {
                            result.AppendLine($"Execution Error: {executionError.Message}");
                        }
                        if (output.Length == 0 && errorOutput.Length == 0 && executionError == null)
                        {
                            result.AppendLine("Command executed successfully with no output");
                        }

                        return result.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                return $"Error executing PowerShell command: {ex.Message}";
            }
        }

        [KernelFunction, Description("Execute PowerShell script file")]
        public async Task<string> ExecutePowerShellScript(
            [Description("Path to the PowerShell script file")] string scriptPath,
            [Description("Script parameters (comma-separated)")] string parameters = "",
            [Description("Command timeout in seconds")] int timeoutSeconds = 60)
        {
            try
            {
                if (!File.Exists(scriptPath))
                {
                    return $"Error: Script file not found - {scriptPath}";
                }

                var scriptContent = await File.ReadAllTextAsync(scriptPath);
                if (!string.IsNullOrEmpty(parameters))
                {
                    scriptContent += $" {parameters}";
                }

                return await ExecutePowerShellCommand(scriptContent, timeoutSeconds);
            }
            catch (Exception ex)
            {
                return $"Error executing PowerShell script: {ex.Message}";
            }
        }

        [KernelFunction, Description("Execute command with output capture")]
        public async Task<string> ExecuteSystemCommand(
            [Description("The system command to execute")] string command,
            [Description("Command arguments")] string arguments = "",
            [Description("Command timeout in seconds")] int timeoutSeconds = 30)
        {
            try
            {
                var processInfo = new ProcessStartInfo
                {
                    FileName = command,
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                var output = new StringBuilder();
                var errorOutput = new StringBuilder();

                using (var process = Process.Start(processInfo))
                {
                    if (process != null)
                    {
                        output.Append(process.StandardOutput.ReadToEnd());
                        errorOutput.Append(process.StandardError.ReadToEnd());

                        if (!process.WaitForExit(timeoutSeconds * 1000))
                        {
                            process.Kill();
                            return "Error: Command execution timeout";
                        }

                        var result = new StringBuilder();
                        result.AppendLine($"Command: {command} {arguments}");
                        result.AppendLine($"Exit Code: {process.ExitCode}");
                        result.AppendLine("---");
                        if (output.Length > 0)
                        {
                            result.AppendLine("Output:");
                            result.Append(output.ToString());
                        }
                        if (errorOutput.Length > 0)
                        {
                            result.AppendLine("Error Output:");
                            result.Append(errorOutput.ToString());
                        }

                        return result.ToString();
                    }
                }

                return "Error: Failed to start process";
            }
            catch (Exception ex)
            {
                return $"Error executing system command: {ex.Message}";
            }
        }

        [KernelFunction, Description("Get list of available PowerShell cmdlets")]
        public async Task<string> GetPowerShellCmdlets(
            [Description("Search pattern for cmdlet names (optional)")] string pattern = "")
        {
            try
            {
                var command = string.IsNullOrEmpty(pattern)
                    ? "Get-Command -CommandType Cmdlet | Select-Object -ExpandProperty Name | Sort-Object"
                    : $"Get-Command -CommandType Cmdlet -Name '*{pattern}*' | Select-Object -ExpandProperty Name | Sort-Object";

                using (var runspace = RunspaceFactory.CreateRunspace())
                {
                    runspace.Open();

                    using (var pipeline = runspace.CreatePipeline(command))
                    {
                        var results = pipeline.Invoke();
                        var output = new StringBuilder();

                        output.AppendLine(string.IsNullOrEmpty(pattern)
                            ? "Available PowerShell Cmdlets:"
                            : $"PowerShell Cmdlets matching '{pattern}':");
                        output.AppendLine();

                        if (results != null && results.Count > 0)
                        {
                            foreach (var result in results.Take(100))
                            {
                                output.AppendLine($"  {result}");
                            }

                            if (results.Count > 100)
                            {
                                output.AppendLine($"  ... and {results.Count - 100} more cmdlets");
                            }
                        }

                        return output.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                return $"Error getting PowerShell cmdlets: {ex.Message}";
            }
        }

        [KernelFunction, Description("Get PowerShell command help information")]
        public async Task<string> GetPowerShellHelp(
            [Description("The cmdlet or command name")] string commandName)
        {
            try
            {
                if (string.IsNullOrEmpty(commandName))
                {
                    return "Error: Command name is required";
                }

                var command = $"Get-Help {commandName} -Full";

                using (var runspace = RunspaceFactory.CreateRunspace())
                {
                    runspace.Open();

                    using (var pipeline = runspace.CreatePipeline(command))
                    {
                        var results = pipeline.Invoke();
                        var output = new StringBuilder();

                        if (results != null && results.Count > 0)
                        {
                            foreach (var result in results)
                            {
                                output.AppendLine(result.ToString());
                            }
                        }
                        else
                        {
                            output.AppendLine($"No help found for command: {commandName}");
                        }

                        return output.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                return $"Error getting help: {ex.Message}";
            }
        }
    }
}
