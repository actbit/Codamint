using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.Text;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Management.Automation.Runspaces;

namespace Codamint.Plugins
{
    /// <summary>
    /// PowerShell実行機能を提供する専用プラグイン
    /// </summary>
    public class PowerShellPlugin
    {
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

    }
}
