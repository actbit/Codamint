using System.Text;

namespace Codamint.Utils
{
    /// <summary>
    /// 出力フォーマッティングをサポートするヘルパークラス
    /// </summary>
    public static class OutputFormatter
    {
        public enum OutputFormat
        {
            Text,
            Json,
            Markdown
        }

        /// <summary>
        /// エージェント出力をフォーマット
        /// </summary>
        public static string FormatOutput(string content, OutputFormat format = OutputFormat.Text)
        {
            return format switch
            {
                OutputFormat.Json => FormatAsJson(content),
                OutputFormat.Markdown => FormatAsMarkdown(content),
                _ => FormatAsText(content)
            };
        }

        /// <summary>
        /// テキスト形式でフォーマット
        /// </summary>
        private static string FormatAsText(string content)
        {
            var sb = new StringBuilder();
            sb.AppendLine("=".PadRight(80, '='));
            sb.AppendLine(content);
            sb.AppendLine("=".PadRight(80, '='));
            return sb.ToString();
        }

        /// <summary>
        /// JSON形式でフォーマット（簡略版）
        /// </summary>
        private static string FormatAsJson(string content)
        {
            var escaped = content
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("\n", "\\n")
                .Replace("\r", "\\r");

            return $@"{{
  ""response"": ""{escaped}"",
  ""timestamp"": ""{DateTime.UtcNow:O}""
}}";
        }

        /// <summary>
        /// Markdown形式でフォーマット
        /// </summary>
        private static string FormatAsMarkdown(string content)
        {
            return $@"
---

{content}

---
";
        }

        /// <summary>
        /// コード分析結果をフォーマット
        /// </summary>
        public static string FormatAnalysisResult(string code, string analysis)
        {
            var sb = new StringBuilder();
            sb.AppendLine("CODE ANALYSIS REPORT");
            sb.AppendLine("=".PadRight(80, '='));
            sb.AppendLine();
            sb.AppendLine("ANALYZED CODE:");
            sb.AppendLine("-".PadRight(40, '-'));
            sb.AppendLine(code);
            sb.AppendLine();
            sb.AppendLine("ANALYSIS:");
            sb.AppendLine("-".PadRight(40, '-'));
            sb.AppendLine(analysis);
            sb.AppendLine();
            sb.AppendLine("=".PadRight(80, '='));
            return sb.ToString();
        }

        /// <summary>
        /// ファイル操作結果をフォーマット
        /// </summary>
        public static string FormatFileOperationResult(string operation, string filePath, bool success, string message = "")
        {
            var status = success ? "SUCCESS" : "FAILED";
            var sb = new StringBuilder();
            sb.AppendLine($"[{status}] {operation}");
            sb.AppendLine($"File: {filePath}");
            if (!string.IsNullOrEmpty(message))
            {
                sb.AppendLine($"Message: {message}");
            }
            return sb.ToString();
        }

        /// <summary>
        /// エラーをフォーマット
        /// </summary>
        public static string FormatError(string errorMessage)
        {
            var sb = new StringBuilder();
            sb.AppendLine("[ERROR]");
            sb.AppendLine("!".PadRight(80, '!'));
            sb.AppendLine(errorMessage);
            sb.AppendLine("!".PadRight(80, '!'));
            return sb.ToString();
        }

        /// <summary>
        /// 成功メッセージをフォーマット
        /// </summary>
        public static string FormatSuccess(string message)
        {
            var sb = new StringBuilder();
            sb.AppendLine("[SUCCESS]");
            sb.AppendLine("-".PadRight(80, '-'));
            sb.AppendLine(message);
            sb.AppendLine("-".PadRight(80, '-'));
            return sb.ToString();
        }

        /// <summary>
        /// テーブル形式でフォーマット
        /// </summary>
        public static string FormatTable(List<string> headers, List<List<string>> rows)
        {
            var sb = new StringBuilder();
            var columnWidths = new int[headers.Count];

            // カラム幅を計算
            for (int i = 0; i < headers.Count; i++)
            {
                columnWidths[i] = headers[i].Length;
                foreach (var row in rows)
                {
                    if (i < row.Count)
                    {
                        columnWidths[i] = Math.Max(columnWidths[i], row[i].Length);
                    }
                }
            }

            // ヘッダーを出力
            foreach (var (header, width) in headers.Zip(columnWidths))
            {
                sb.Append(header.PadRight(width) + " | ");
            }
            sb.AppendLine();

            // 区切り線
            foreach (var width in columnWidths)
            {
                sb.Append(new string('-', width) + "-+-");
            }
            sb.AppendLine();

            // データ行を出力
            foreach (var row in rows)
            {
                for (int i = 0; i < headers.Count; i++)
                {
                    var cell = i < row.Count ? row[i] : "";
                    sb.Append(cell.PadRight(columnWidths[i]) + " | ");
                }
                sb.AppendLine();
            }

            return sb.ToString();
        }

        /// <summary>
        /// 進捗バーをフォーマット
        /// </summary>
        public static string FormatProgressBar(int current, int total, int barLength = 40)
        {
            if (total == 0) total = 1;
            var percentage = (current * 100) / total;
            var filledLength = (current * barLength) / total;
            var bar = new string('█', filledLength) + new string('░', barLength - filledLength);

            return $"Progress: [{bar}] {percentage}% ({current}/{total})";
        }

        /// <summary>
        /// 区切り線を表示
        /// </summary>
        public static string GetSeparator(char character = '-', int length = 80)
        {
            return new string(character, length);
        }

        /// <summary>
        /// タイムスタンプ付きメッセージをフォーマット
        /// </summary>
        public static string FormatWithTimestamp(string message)
        {
            return $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}";
        }
    }
}
