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

        string factionDescRaw;
        try
        {
            var factionPrompt = PromptTemplates.BuildFactionPrompt(factionName, context.Options);
            factionDescRaw = _slm.GenerateFactionFlavor(factionPrompt, context.Seed);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Failed to generate faction description. Error: {ex.Message}", ex);
        }

        string description = factionDescRaw;
        string ideology = "Neutral";

        try
        {
            var parsed = ToonCodec.Deserialize<Dictionary<string, object>>(factionDescRaw);
            if (parsed == null)
            {
                var list = ToonCodec.Deserialize<List<Dictionary<string, object>>>(factionDescRaw);
                if (list != null && list.Count > 0) parsed = list[0];
            }

            if (parsed != null)
            {
                if (parsed.TryGetValue("description", out var d) && d != null) description = d.ToString() ?? factionDescRaw;
                if (parsed.TryGetValue("ideology", out var i) && i != null) ideology = i.ToString() ?? ideology;
            }
        }
        catch (Exception ex)
        {
            _logger?.LogDebug(ex, "Structured parse failed for faction output; falling back to raw text");
        }

        var faction = new FactionModel
        {
            Id = "faction1",
            Name = factionName,
            Description = description,
            Ideology = ideology,
            Relations = new Dictionary<string, int>()
        };

        _logger?.LogInformation("? Faction generated: {FactionName}", factionName);

        return new List<FactionModel> { faction };
    }
}
