using Microsoft.Extensions.Configuration;

namespace Codamint.Utils
{
    /// <summary>
    /// 設定読み込みをサポートするヘルパークラス
    /// </summary>
    public static class ConfigurationHelper
    {
        /// <summary>
        /// IConfigurationを作成
        /// </summary>
        public static IConfiguration BuildConfiguration(string? configPath = null)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            // 環境変数に応じて設定ファイルを追加
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
            if (environment != "Production")
            {
                builder.AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true);
            }

            // カスタム設定ファイルがある場合は追加
            if (!string.IsNullOrEmpty(configPath) && File.Exists(configPath))
            {
                builder.AddJsonFile(configPath, optional: true, reloadOnChange: true);
            }

            // 環境変数で設定値をオーバーライド
            builder.AddEnvironmentVariables();

            return builder.Build();
        }

        /// <summary>
        /// 環境を設定
        /// </summary>
        public static void SetEnvironment(string environment)
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", environment);
        }

        /// <summary>
        /// 現在の環境を取得
        /// </summary>
        public static string GetCurrentEnvironment()
        {
            return Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
        }

        /// <summary>
        /// 設定値が存在するか確認
        /// </summary>
        public static bool HasConfiguration(IConfiguration configuration, string key)
        {
            var value = configuration[key];
            return !string.IsNullOrEmpty(value);
        }

        /// <summary>
        /// 設定値を取得（存在しない場合はデフォルト値）
        /// </summary>
        public static string GetConfiguration(IConfiguration configuration, string key, string defaultValue = "")
        {
            return configuration[key] ?? defaultValue;
        }

        /// <summary>
        /// OpenAI APIキーが設定されているか確認
        /// </summary>
        public static bool HasOpenAIKey(IConfiguration configuration)
        {
            return HasConfiguration(configuration, "OpenAI:ApiKey");
        }

        /// <summary>
        /// OpenAI APIキーを取得
        /// </summary>
        public static string GetOpenAIKey(IConfiguration configuration)
        {
            var key = GetConfiguration(configuration, "OpenAI:ApiKey");
            if (string.IsNullOrEmpty(key))
            {
                throw new InvalidOperationException(
                    "OpenAI API Key is not configured. " +
                    "Please set OPENAI_APIKEY environment variable or configure appsettings.json");
            }
            return key;
        }
    }
}
