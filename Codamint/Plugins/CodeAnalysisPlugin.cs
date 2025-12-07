using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace Codamint.Plugins
{
    /// <summary>
    /// コード分析・レビュー機能を提供するプラグイン
    /// </summary>
    public class CodeAnalysisPlugin
    {
        [KernelFunction, Description("Analyze code for bugs and issues")]
        public async Task<string> AnalyzeCode(
            Kernel kernel,
            [Description("The code to analyze")] string code,
            [Description("Programming language")] string language = "C#")
        {
            var prompt = $@"Analyze the following {language} code for potential issues, bugs, and improvements:

Code:
{code}

Please provide:
1. Identified bugs or issues
2. Performance concerns
3. Security vulnerabilities (if any)
4. Code quality improvements
5. Best practice violations

Analysis Report:";

            var response = await kernel.InvokePromptAsync(prompt);
            return response.ToString();
        }

        [KernelFunction, Description("Review code and provide suggestions")]
        public async Task<string> ReviewCode(
            Kernel kernel,
            [Description("The code to review")] string code,
            [Description("Review focus areas (e.g., 'readability,performance,security')")] string focusAreas = "readability,performance,maintainability")
        {
            var prompt = $@"Perform a comprehensive code review focusing on: {focusAreas}

Code:
{code}

Please provide:
1. Strengths of the code
2. Areas for improvement
3. Specific suggestions with code examples
4. Priority level for each suggestion (High/Medium/Low)

Code Review:";

            var response = await kernel.InvokePromptAsync(prompt);
            return response.ToString();
        }

        [KernelFunction, Description("Check code for security vulnerabilities")]
        public async Task<string> SecurityAnalysis(
            Kernel kernel,
            [Description("The code to analyze")] string code)
        {
            var prompt = $@"Perform a security analysis on the following code:

Code:
{code}

Check for:
1. SQL Injection vulnerabilities
2. XSS (Cross-Site Scripting) vulnerabilities
3. Authentication/Authorization issues
4. Sensitive data exposure
5. Dependency vulnerabilities
6. Input validation issues

Security Report:";

            var response = await kernel.InvokePromptAsync(prompt);
            return response.ToString();
        }

        [KernelFunction, Description("Suggest refactoring improvements")]
        public async Task<string> SuggestRefactoring(
            Kernel kernel,
            [Description("The code to refactor")] string code)
        {
            var prompt = $@"Suggest refactoring improvements for the following code:

Code:
{code}

Provide:
1. Identified code smells
2. Suggested design patterns
3. Refactored code examples
4. Benefits of each refactoring
5. Implementation difficulty (Easy/Medium/Hard)

Refactoring Suggestions:";

            var response = await kernel.InvokePromptAsync(prompt);
            return response.ToString();
        }
    }
}
