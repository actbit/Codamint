using Microsoft.SemanticKernel;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System.ComponentModel;
using System.Text;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Management.Automation.Runspaces;

namespace Codamint.Plugins
{
    /// <summary>
    /// C# コード実行とPowerShellシェル機能を提供するプラグイン（Roslyn使用）
    /// </summary>
    public class CodeExecutionPlugin
    {
        [KernelFunction, Description("Execute C# code snippet using Roslyn")]
        public async Task<string> ExecuteCSharpCode(
            [Description("The C# code to execute")] string code)
        {
            try
            {
                var result = await CSharpScript.EvaluateAsync(code);
                return $"Execution successful!\nResult: {result ?? "null"}";
            }
            catch (CompilationErrorException ex)
            {
                var sb = new StringBuilder();
                sb.AppendLine("Compilation Error:");
                foreach (var diagnostic in ex.Diagnostics)
                {
                    sb.AppendLine($"  Line {diagnostic.Location.GetLineSpan().StartLinePosition.Line + 1}: {diagnostic.GetMessage()}");
                }
                return sb.ToString();
            }
            catch (Exception ex)
            {
                return $"Execution Error: {ex.GetType().Name}\n{ex.Message}";
            }
        }

        [KernelFunction, Description("Validate C# code syntax")]
        public async Task<string> ValidateCSharpSyntax(
            [Description("The C# code to validate")] string code)
        {
            try
            {
                var script = CSharpScript.Create(code);
                var compilation = script.GetCompilation();
                var diagnostics = compilation.GetDiagnostics();

                if (!diagnostics.Any())
                {
                    return "✓ Syntax is valid! No compilation errors found.";
                }

                var sb = new StringBuilder();
                sb.AppendLine("Syntax Validation Issues:");
                foreach (var diagnostic in diagnostics)
                {
                    var lineSpan = diagnostic.Location.GetLineSpan();
                    var lineNumber = lineSpan.StartLinePosition.Line + 1;
                    var column = lineSpan.StartLinePosition.Character + 1;
                    sb.AppendLine($"  Line {lineNumber}, Column {column}: {diagnostic.GetMessage()}");
                }
                return sb.ToString();
            }
            catch (Exception ex)
            {
                return $"Validation Error: {ex.Message}";
            }
        }

        [KernelFunction, Description("Execute C# code with custom globals")]
        public async Task<string> ExecuteCSharpCodeWithGlobals(
            [Description("The C# code to execute")] string code,
            [Description("Global variables as JSON (e.g., {\"x\": 10, \"name\": \"test\"})")] string globals = "{}")
        {
            try
            {
                // Parse globals JSON (simplified implementation)
                var globalVars = new Dictionary<string, object>();
                if (!string.IsNullOrWhiteSpace(globals) && globals != "{}")
                {
                    // For simple globals, you might use JsonSerializer
                    // This is a simplified version
                    globalVars["globals"] = globals;
                }

                var result = await CSharpScript.EvaluateAsync(code, globals: globalVars);
                return $"Execution successful!\nResult: {result ?? "null"}";
            }
            catch (Exception ex)
            {
                return $"Execution Error: {ex.GetType().Name}\n{ex.Message}";
            }
        }

        [KernelFunction, Description("Get syntax errors in C# code")]
        public async Task<string> GetSyntaxErrors(
            [Description("The C# code to analyze")] string code)
        {
            try
            {
                var script = CSharpScript.Create(code);
                var compilation = script.GetCompilation();
                var diagnostics = compilation.GetDiagnostics()
                    .Where(d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error)
                    .ToList();

                if (!diagnostics.Any())
                {
                    return "No syntax errors found!";
                }

                var sb = new StringBuilder();
                sb.AppendLine($"Found {diagnostics.Count} syntax error(s):");
                sb.AppendLine();

                foreach (var diagnostic in diagnostics)
                {
                    var lineSpan = diagnostic.Location.GetLineSpan();
                    var lineNumber = lineSpan.StartLinePosition.Line + 1;
                    var column = lineSpan.StartLinePosition.Character + 1;
                    sb.AppendLine($"[Line {lineNumber}, Col {column}] {diagnostic.Id}: {diagnostic.GetMessage()}");
                }

                return sb.ToString();
            }
            catch (Exception ex)
            {
                return $"Analysis Error: {ex.Message}";
            }
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
