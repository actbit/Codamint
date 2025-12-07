using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace Codamint.Plugins
{
    /// <summary>
    /// コード生成機能を提供するプラグイン
    /// </summary>
    public class CodeGenerationPlugin
    {
        [KernelFunction, Description("Generate code based on the given requirements")]
        public async Task<string> GenerateCode(
            Kernel kernel,
            [Description("The requirements or description for the code to generate")] string requirements,
            [Description("The programming language")] string language = "C#")
        {
            var prompt = $@"You are an expert software developer. Generate {language} code based on the following requirements:

Requirements:
{requirements}

Please provide:
1. Clean, well-structured code
2. Appropriate error handling
3. Comments explaining key sections
4. Best practices for {language}

Generated Code:";

            var response = await kernel.InvokePromptAsync(prompt);
            var result = response.ToString();

            // JSON シリアライズ時のエスケープ問題を回避
            result = result.Replace("\r\n", "\n").Replace("\r", "\n");

            return result;
        }

        [KernelFunction, Description("Generate a function with specific signature")]
        public async Task<string> GenerateFunction(
            Kernel kernel,
            [Description("Function name")] string functionName,
            [Description("Function parameters")] string parameters,
            [Description("Function purpose")] string purpose,
            [Description("Programming language")] string language = "C#")
        {
            var prompt = $@"Generate a {language} function with the following specifications:
- Function name: {functionName}
- Parameters: {parameters}
- Purpose: {purpose}

Provide a complete, production-ready implementation with proper error handling and documentation.";

            var response = await kernel.InvokePromptAsync(prompt);
            var result = response.ToString();

            // JSON シリアライズ時のエスケープ問題を回避
            result = result.Replace("\r\n", "\n").Replace("\r", "\n");

            return result;
        }

        [KernelFunction, Description("Generate unit tests for given code")]
        public async Task<string> GenerateUnitTests(
            Kernel kernel,
            [Description("The code to generate tests for")] string code,
            [Description("The testing framework")] string framework = "xUnit")
        {
            var prompt = $@"Generate {framework} unit tests for the following code:

Code:
{code}

Requirements:
1. Cover happy path and edge cases
2. Use meaningful test names
3. Include setup and teardown if needed
4. Follow {framework} best practices

Generated Tests:";

            var response = await kernel.InvokePromptAsync(prompt);
            var result = response.ToString();

            // JSON シリアライズ時のエスケープ問題を回避
            result = result.Replace("\r\n", "\n").Replace("\r", "\n");

            return result;
        }
    }
}
