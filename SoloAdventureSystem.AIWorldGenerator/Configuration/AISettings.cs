namespace SoloAdventureSystem.ContentGenerator.Configuration;

/// <summary>
/// Configuration settings for AI providers used in world generation.
/// Now uses value objects for type safety and validation.
/// </summary>
public class AISettings
{
    public const string SectionName = "AI";
    
    // Provider Configuration (kept as string for serialization compatibility)
    private string _provider = "Stub";
    private string _model = "gpt-4o-mini";
    private string _llamaModelKey = "phi-3-mini-q4";
    
    public string Provider 
    { 
        get => _provider; 
        set => _provider = value ?? "Stub";
    }
    
    public string Endpoint { get; set; } = "https://models.inference.ai.azure.com";
    public string Model 
    { 
        get => _model; 
        set => _model = value ?? "gpt-4o-mini";
    }
    public string? Token { get; set; }
    
    // Generation Settings
    public double Temperature { get; set; } = 0.0;
    
    // Caching Configuration
    public bool EnableCaching { get; set; } = true;
    public string CacheDirectory { get; set; } = ".aicache";
    
    // Retry & Timeout Configuration
    public int MaxRetries { get; set; } = 3;
    public int TimeoutSeconds { get; set; } = 30;
    
    // LLamaSharp-specific Settings
    public string LLamaModelKey 
    { 
        get => _llamaModelKey; 
        set => _llamaModelKey = value ?? "phi-3-mini-q4";
    }
    public int? ContextSize { get; set; } = 2048;
    public int? Seed { get; set; }
    public bool UseGPU { get; set; }
    public int MaxInferenceThreads { get; set; } = 4;
    
    // Strongly-typed accessors (for runtime use)
    
    /// <summary>
    /// Get the provider as a strongly-typed value object
    /// </summary>
    public AIProvider GetProvider() => AIProvider.Parse(Provider);
    
    /// <summary>
    /// Get the model configuration as a strongly-typed value object
    /// </summary>
    public ModelConfiguration GetModelConfiguration() => 
        ModelConfiguration.Parse(LLamaModelKey);
    
    /// <summary>
    /// Get the hardware configuration as a strongly-typed value object
    /// </summary>
    public HardwareConfiguration GetHardwareConfiguration() =>
        HardwareConfiguration.Create(UseGPU, MaxInferenceThreads);
    
    /// <summary>
    /// Set provider using strongly-typed value object
    /// </summary>
    public void SetProvider(AIProvider provider)
    {
        Provider = provider?.Name ?? throw new ArgumentNullException(nameof(provider));
    }
    
    /// <summary>
    /// Set model using strongly-typed value object
    /// </summary>
    public void SetModel(ModelConfiguration model)
    {
        LLamaModelKey = model?.ModelKey ?? throw new ArgumentNullException(nameof(model));
        ContextSize = model.ContextSize;
    }
    
    /// <summary>
    /// Set hardware configuration using strongly-typed value object
    /// </summary>
    public void SetHardware(HardwareConfiguration hardware)
    {
        if (hardware == null) throw new ArgumentNullException(nameof(hardware));
        UseGPU = hardware.UseGPU;
        MaxInferenceThreads = hardware.MaxThreads;
    }
}
