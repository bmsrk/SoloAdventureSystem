using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using SoloAdventureSystem.ContentGenerator.Adapters;

namespace SoloAdventureSystem.ContentGenerator.Generation;

/// <summary>
/// Specialized generator for world lore content.
/// Creates background lore that enriches the world's atmosphere.
/// </summary>
public class LoreGenerator : IContentGenerator<List<string>>
{
    private readonly ILocalSLMAdapter _slm;
    private readonly ILogger<LoreGenerator>? _logger;
    private const int DefaultLoreEntryCount = 3;

    public LoreGenerator(ILocalSLMAdapter slm, ILogger<LoreGenerator>? logger = null)
    {
        _slm = slm ?? throw new ArgumentNullException(nameof(slm));
        _logger = logger;
    }

    public List<string> Generate(WorldGenerationContext context)
    {
        _logger?.LogInformation("?? Generating lore entries with improved quality...");

        try
        {
            var loreEntries = new List<string>();
            for (int i = 0; i < DefaultLoreEntryCount; i++)
            {
                var lorePrompt = PromptTemplates.BuildLorePrompt(
                    context.Options.Name, 
                    context.Options, 
                    i + 1);
                var entrySeed = context.GetSeedFor("Lore", i);
                var entry = _slm.GenerateLoreEntries(lorePrompt, entrySeed, 1)[0];
                loreEntries.Add(entry);
            }

            _logger?.LogInformation("? Generated {Count} lore entries", loreEntries.Count);
            return loreEntries;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Failed to generate lore entries. Error: {ex.Message}", ex);
        }
    }
}
