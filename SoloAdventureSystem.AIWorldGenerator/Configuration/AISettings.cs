namespace SoloAdventureSystem.ContentGenerator.Configuration;

/// <summary>
/// Configuration settings for AI providers used in world generation.
/// </summary>
public class AISettings
{
    public const string SectionName = "AI";
    
    // Provider Configuration
    public string Provider { get; set; } = "Stub";  // Stub or LLamaSharp
    public string Endpoint { get; set; } = "https://models.inference.ai.azure.com";
    public string Model { get; set; } = "gpt-4o-mini";
    public string? Token { get; set; }
    
    // Generation Settings
    public double Temperature { get; set; } = 0.0;  // 0.0 for deterministic generation
    
    // Caching Configuration
    public bool EnableCaching { get; set; } = true;
    public string CacheDirectory { get; set; } = ".aicache";
    
    // Retry & Timeout Configuration
    public int MaxRetries { get; set; } = 3;
    public int TimeoutSeconds { get; set; } = 30;
    
    // LLamaSharp-specific Settings
    public string LLamaModelKey { get; set; } = "phi-3-mini-q4";  // phi-3-mini-q4, tinyllama-q4, llama-3.2-1b-q4
    public int? ContextSize { get; set; } = 2048;                 // Context window size
    public int? Seed { get; set; } = null;                        // Global seed (null = random)
    public bool UseGPU { get; set; } = false;                     // Enable GPU acceleration
    public int MaxInferenceThreads { get; set; } = 4;             // CPU thread count
}
