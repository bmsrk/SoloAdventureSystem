using System;

namespace SoloAdventureSystem.ContentGenerator.Configuration;

/// <summary>
/// Value object representing an AI provider with strong typing and validation.
/// Replaces magic strings for provider names.
/// </summary>
public sealed record AIProvider
{
    public string Name { get; }

    private AIProvider(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Provider name cannot be empty", nameof(name));
        
        Name = name;
    }

    // Well-known providers
    public static AIProvider Stub { get; } = new("Stub");
    public static AIProvider MaIN { get; } = new("MaIN.NET");
    public static AIProvider LLamaSharp { get; } = new("LLamaSharp");

    /// <summary>
    /// Parse provider from string (for configuration loading)
    /// </summary>
    public static AIProvider Parse(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Provider name cannot be empty", nameof(name));

        return name.ToLowerInvariant() switch
        {
            "stub" => Stub,
            "main.net" or "main" or "embedded" => MaIN,
            "llamasharp" or "llama" => LLamaSharp,
            _ => new AIProvider(name) // Allow custom providers
        };
    }

    public override string ToString() => Name;

    // Implicit conversion to string for backwards compatibility
    public static implicit operator string(AIProvider provider) => provider.Name;
}
