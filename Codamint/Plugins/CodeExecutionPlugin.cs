using Microsoft.SemanticKernel;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System.ComponentModel;
using System.Text;

namespace Codamint.Plugins
{
    /// <summary>
    /// C# コード実行機能を提供するプラグイン（Roslyn使用）
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

    }
}
