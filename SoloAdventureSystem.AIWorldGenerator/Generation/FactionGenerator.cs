using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using SoloAdventureSystem.ContentGenerator.Adapters;
using SoloAdventureSystem.ContentGenerator.Models;

namespace SoloAdventureSystem.ContentGenerator.Generation;

/// <summary>
/// Specialized generator for faction content.
/// Handles faction creation with AI-generated lore.
/// </summary>
public class FactionGenerator : IContentGenerator<List<FactionModel>>
{
    private readonly ILocalSLMAdapter _slm;
    private readonly ILogger<FactionGenerator>? _logger;

    public FactionGenerator(ILocalSLMAdapter slm, ILogger<FactionGenerator>? logger = null)
    {
        _slm = slm ?? throw new ArgumentNullException(nameof(slm));
        _logger = logger;
    }

    public List<FactionModel> Generate(WorldGenerationContext context)
    {
        _logger?.LogInformation("??? Generating faction...");

        var factionName = ProceduralNames.GenerateFactionName(context.Seed);
        
        _logger?.LogDebug("Generated faction name: {FactionName}", factionName);

        string factionDescription;
        try
        {
            var factionPrompt = PromptTemplates.BuildFactionPrompt(factionName, context.Options);
            factionDescription = _slm.GenerateFactionFlavor(factionPrompt, context.Seed);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Failed to generate faction description. Error: {ex.Message}", ex);
        }

        var faction = new FactionModel
        {
            Id = "faction1",
            Name = factionName,
            Description = factionDescription,
            Ideology = "Neutral",
            Relations = new Dictionary<string, int>()
        };

        _logger?.LogInformation("? Faction generated: {FactionName}", factionName);

        return new List<FactionModel> { faction };
    }
}
