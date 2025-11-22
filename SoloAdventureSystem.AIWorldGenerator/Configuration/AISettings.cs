namespace SoloAdventureSystem.ContentGenerator.Configuration;

public class AISettings
{
    public const string SectionName = "AI";
    
    public string Provider { get; set; } = "Stub";  // Stub, GitHubModels, AzureOpenAI, OpenAI
    public string Endpoint { get; set; } = "https://models.inference.ai.azure.com";
    public string Model { get; set; } = "gpt-4o-mini";
    public string? Token { get; set; }
    public bool EnableCaching { get; set; } = true;
    public string CacheDirectory { get; set; } = ".aicache";
    public int MaxRetries { get; set; } = 3;
    public int TimeoutSeconds { get; set; } = 30;
    public double Temperature { get; set; } = 0.0;  // 0.0 for deterministic generation
}
