namespace Codamint.Settings
{
    /// <summary>
    /// エージェント全体設定
    /// </summary>
    public class AgentSettings
    {
        public string Name { get; set; } = "Coding Agent";

        public string Description { get; set; } = "AI-powered coding assistant";

        public bool EnableFileOperations { get; set; } = true;

        public bool EnableCodeExecution { get; set; } = true;

        public int MaxRetries { get; set; } = 3;

        public int TimeoutSeconds { get; set; } = 30;
    }
}
