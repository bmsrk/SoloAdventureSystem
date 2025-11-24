using System;

namespace SoloAdventureSystem.ContentGenerator.Configuration;

/// <summary>
/// Value object representing a model configuration with validation.
/// Encapsulates model key, context size, and expected download size.
/// </summary>
public sealed record ModelConfiguration
{
    public string ModelKey { get; }
    public int ContextSize { get; }
    public long ExpectedSizeBytes { get; }

    private ModelConfiguration(string modelKey, int contextSize, long sizeBytes)
    {
        if (string.IsNullOrWhiteSpace(modelKey))
            throw new ArgumentException("Model key cannot be empty", nameof(modelKey));
        if (contextSize <= 0)
            throw new ArgumentException("Context size must be positive", nameof(contextSize));
        if (sizeBytes <= 0)
            throw new ArgumentException("Expected size must be positive", nameof(sizeBytes));

        ModelKey = modelKey;
        ContextSize = contextSize;
        ExpectedSizeBytes = sizeBytes;
    }

    // Well-known model configurations
    public static ModelConfiguration Default => Phi3Mini;
    
    public static ModelConfiguration Phi3Mini { get; } = 
        new("phi-3-mini-q4", 2048, 2_100_000_000);
    
    public static ModelConfiguration TinyLlama { get; } = 
        new("tinyllama-q4", 2048, 700_000_000);
    
    public static ModelConfiguration Llama32 { get; } = 
        new("llama-3.2-1b-q4", 2048, 800_000_000);

    /// <summary>
    /// Parse model configuration from string (for configuration loading)
    /// </summary>
    public static ModelConfiguration Parse(string modelKey)
    {
        if (string.IsNullOrWhiteSpace(modelKey))
            throw new ArgumentException("Model key cannot be empty", nameof(modelKey));

        return modelKey.ToLowerInvariant() switch
        {
            "phi-3-mini-q4" or "phi3" => Phi3Mini,
            "tinyllama-q4" or "tinyllama" => TinyLlama,
            "llama-3.2-1b-q4" or "llama32" => Llama32,
            _ => new ModelConfiguration(modelKey, 2048, 1_000_000_000) // Default for unknown models
        };
    }

    /// <summary>
    /// Create custom model configuration
    /// </summary>
    public static ModelConfiguration Create(string modelKey, int contextSize, long expectedSize)
    {
        return new ModelConfiguration(modelKey, contextSize, expectedSize);
    }

    public override string ToString() => ModelKey;
}
