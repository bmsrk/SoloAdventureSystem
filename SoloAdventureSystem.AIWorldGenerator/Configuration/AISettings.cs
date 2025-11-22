namespace SoloAdventureSystem.ContentGenerator.Configuration;

public class AISettings
{
    public const string SectionName = "AI";
    
    public string Provider { get; set; } = "Stub";  // Stub or LLamaSharp
    public string Endpoint { get; set; } = "https://models.inference.ai.azure.com";
    public string Model { get; set; } = "gpt-4o-mini";
    public string? Token { get; set; }
    public bool EnableCaching { get; set; } = true;
    public string CacheDirectory { get; set; } = ".aicache";
    public int MaxRetries { get; set; } = 3;
    public int TimeoutSeconds { get; set; } = 30;
    public double Temperature { get; set; } = 0.0;  // 0.0 for deterministic generation
    
    // Embedded Model Settings (for old ONNX - deprecated)
    public bool UseEmbeddedModel { get; set; } = false;
    public string EmbeddedModelPath { get; set; } = "";
    public bool AllowModelDownload { get; set; } = true;
    public int MaxInferenceThreads { get; set; } = 4;
    public bool UseGPU { get; set; } = false;
    
    // LLamaSharp-specific settings
    public string LLamaModelKey { get; set; } = "phi-3-mini-q4";  // phi-3-mini-q4, tinyllama-q4, llama-3.2-1b-q4
    public int? ContextSize { get; set; } = 2048;                 // Context window size
    public int? Seed { get; set; } = null;                        // Global seed (null = random)
}
