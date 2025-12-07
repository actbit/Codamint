using System.Text;

namespace Codamint.Utils
{
    /// <summary>
    /// プロンプト処理をサポートするヘルパークラス
    /// </summary>
    public static class PromptHelper
    {
        /// <summary>
        /// ファイルからプロンプトを読み込む
        /// </summary>
        public static async Task<string> LoadPromptFromFileAsync(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Prompt file not found: {filePath}");
            }

            return await File.ReadAllTextAsync(filePath);
        }

        /// <summary>
        /// プロンプトに変数を埋め込む
        /// </summary>
        public static string InterpolatePrompt(string prompt, Dictionary<string, string> variables)
        {
            var result = prompt;
            foreach (var variable in variables)
            {
                result = result.Replace($"{{{{{variable.Key}}}}}", variable.Value);
            }
            return result;
        }

        /// <summary>
        /// コードブロックをプロンプトにフォーマット
        /// </summary>
        public static string FormatCodeBlock(string code, string language = "csharp")
        {
            return $@"```{language}
{code}
```";
        }

        /// <summary>
        /// 長いプロンプトを短縮
        /// </summary>
        public static string TruncatePrompt(string prompt, int maxLength = 500)
        {
            if (prompt.Length <= maxLength)
            {
                return prompt;
            }

            return prompt.Substring(0, maxLength - 3) + "...";
        }

        /// <summary>
        /// プロンプトから重要なキーワードを抽出
        /// </summary>
        public static List<string> ExtractKeywords(string prompt, string[] keywords)
        {
            var found = new List<string>();
            var lowerPrompt = prompt.ToLower();

            foreach (var keyword in keywords)
            {
                if (lowerPrompt.Contains(keyword.ToLower()))
                {
                    found.Add(keyword);
                }
            }

            return found;
        }

        /// <summary>
        /// プロンプトからコードブロックを抽出
        /// </summary>
        public static List<string> ExtractCodeBlocks(string text)
        {
            var codeBlocks = new List<string>();
            var lines = text.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

            var inCodeBlock = false;
            var codeBlock = new StringBuilder();

            foreach (var line in lines)
            {
                if (line.TrimStart().StartsWith("```"))
                {
                    if (inCodeBlock)
                    {
                        codeBlocks.Add(codeBlock.ToString());
                        codeBlock.Clear();
                        inCodeBlock = false;
                    }
                    else
                    {
                        inCodeBlock = true;
                    }
                }
                else if (inCodeBlock)
                {
                    codeBlock.AppendLine(line);
                }
            }

            return codeBlocks;
        }

        /// <summary>
        /// ユーザー入力をサニタイズ
        /// </summary>
        public static string SanitizeInput(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            // 危険な文字をエスケープ
            return input
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("\n", "\\n")
                .Replace("\r", "\\r")
                .Trim();
        }

        /// <summary>
        /// プロンプトから言語を自動検出
        /// </summary>
        public static string DetectLanguage(string prompt)
        {
            var lowerPrompt = prompt.ToLower();

            if (lowerPrompt.Contains("python")) return "python";
            if (lowerPrompt.Contains("javascript") || lowerPrompt.Contains("js")) return "javascript";
            if (lowerPrompt.Contains("java")) return "java";
            if (lowerPrompt.Contains("c#") || lowerPrompt.Contains("csharp")) return "csharp";
            if (lowerPrompt.Contains("c++") || lowerPrompt.Contains("cpp")) return "cpp";
            if (lowerPrompt.Contains("go")) return "go";
            if (lowerPrompt.Contains("rust")) return "rust";
            if (lowerPrompt.Contains("ruby")) return "ruby";
            if (lowerPrompt.Contains("php")) return "php";
            if (lowerPrompt.Contains("sql")) return "sql";

            return "csharp"; // デフォルト
        }

        /// <summary>
        /// 会話履歴を管理
        /// </summary>
        public static class ConversationManager
        {
            private static List<(string role, string content)> _history = new();

            public static void AddMessage(string role, string content)
            {
                _history.Add((role, content));
            }

            public static void ClearHistory()
            {
                _history.Clear();
            }

            public static List<(string role, string content)> GetHistory()
            {
                return _history;
            }

            public static string GetLastMessage()
            {
                if (_history.Count == 0) return string.Empty;
                return _history.Last().content;
            }
        }
    }
}
