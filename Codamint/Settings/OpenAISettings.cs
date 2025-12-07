namespace Codamint.Settings
{
    /// <summary>
    /// OpenAI API設定
    /// </summary>
    public class OpenAISettings
    {
        public string ApiKey { get; set; } = string.Empty;

        public string Model { get; set; } = "gpt-4o";

        /// <summary>
        /// 温度パラメータ（0-2）
        /// null の場合はAPIのデフォルト値を使用
        /// </summary>
        public double? Temperature { get; set; } = null;

        /// <summary>
        /// 最大トークン数
        /// null の場合はAPIのデフォルト値を使用
        /// </summary>
        public int? MaxTokens { get; set; } = null;

        /// <summary>
        /// APIエンドポイント URL
        /// デフォルト: https://api.openai.com/v1
        /// Azure OpenAI の場合: https://{resource-name}.openai.azure.com/
        /// </summary>
        public string Endpoint { get; set; } = "https://api.openai.com/v1";

        /// <summary>
        /// API プロバイダータイプ
        /// OpenAI | AzureOpenAI | Custom
        /// </summary>
        public string ProviderType { get; set; } = "OpenAI";

        /// <summary>
        /// Azure OpenAI の場合のデプロイメント名
        /// </summary>
        public string? AzureDeploymentName { get; set; }

        /// <summary>
        /// API バージョン
        /// </summary>
        public string ApiVersion { get; set; } = "2024-02-15-preview";

        /// <summary>
        /// リクエストタイムアウト（秒）
        /// </summary>
        public int TimeoutSeconds { get; set; } = 30;

        /// <summary>
        /// HTTP プロキシ URL（オプション）
        /// </summary>
        public string? ProxyUrl { get; set; }
    }
}
