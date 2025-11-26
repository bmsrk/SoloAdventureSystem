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
    private readonly SoloAdventureSystem.ContentGenerator.Parsing.IStructuredOutputParser _structuredParser;

    public FactionGenerator(ILocalSLMAdapter slm, ILogger<FactionGenerator>? logger = null, SoloAdventureSystem.ContentGenerator.Parsing.IStructuredOutputParser? structuredParser = null)
    {
        _slm = slm ?? throw new ArgumentNullException(nameof(slm));
        _logger = logger;
        _structuredParser = structuredParser ?? new SoloAdventureSystem.ContentGenerator.Parsing.JsonStructuredOutputParser();
    }

    public List<FactionModel> Generate(WorldGenerationContext context)
    {
        _logger?.LogInformation("??? Generating faction...");

        string factionName = string.Empty;
        string factionDescRaw = string.Empty;
        string ideology = "Neutral";
        Exception? lastEx = null;

        // Try several prompts to get a rich faction name and description
        for (int attempt = 0; attempt < 3; attempt++)
        {
            try
            {
                string prompt;
                if (attempt == 0)
                {
                    prompt = PromptTemplates.BuildFactionPrompt(string.Empty, context.Options);
                }
                else
                {
                    prompt = $@"Produce a JSON object with fields: name (short), description (2-4 sentences), ideology (single word).
World: {context.Options.Name}
Theme: {context.Options.Theme}
Plot: {context.Options.MainPlotPoint}
Return only JSON.";
                }

                // Use per-run randomness internally; do not rely on external seed
                var seed = context.Random.Next();
                factionDescRaw = _slm.GenerateFactionFlavor(prompt);

                try
                {
                    Dictionary<string, object>? parsed = null;
                    try
                    {
                        _structuredParser.TryParse<Dictionary<string, object>>(factionDescRaw, out parsed);
                    }
                    catch { parsed = null; }

                    if (parsed == null)
                    {
                        try
                        {
                            List<Dictionary<string, object>>? list = null;
                            _structuredParser.TryParse<List<Dictionary<string, object>>>(factionDescRaw, out list);
                            if (list != null && list.Count > 0) parsed = list[0];
                        }
                        catch { parsed = null; }
                    }

                    if (parsed != null)
                    {
                        if (parsed.TryGetValue("name", out var n) && n != null) factionName = n.ToString() ?? string.Empty;
                        if (parsed.TryGetValue("description", out var d) && d != null) factionDescRaw = d.ToString() ?? factionDescRaw;
                        if (parsed.TryGetValue("ideology", out var i) && i != null) ideology = i.ToString() ?? ideology;
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogDebug(ex, "Structured parse failed for faction output on attempt {Attempt}", attempt + 1);
                }

                if (!string.IsNullOrWhiteSpace(factionDescRaw)) break;
            }
            catch (Exception ex)
            {
                lastEx = ex;
                _logger?.LogWarning(ex, "Failed to generate faction on attempt {Attempt}", attempt + 1);
            }
        }

        if (string.IsNullOrWhiteSpace(factionDescRaw))
        {
            var msg = "Failed to generate faction description";
            if (lastEx != null) msg += $": {lastEx.Message}";
            throw new InvalidOperationException(msg);
        }

        if (string.IsNullOrWhiteSpace(factionName)) factionName = "The Collective";

        var faction = new FactionModel
        {
            Id = "faction1",
            Name = factionName,
            Description = factionDescRaw,
            Ideology = ideology,
            Relations = new Dictionary<string, int>()
        };
        // Note: do not append per-faction AI markers here; disclaimer is shown in the UI header

        _logger?.LogInformation("? Faction generated: {FactionName}", factionName);

        return new List<FactionModel> { faction };
    }
}
