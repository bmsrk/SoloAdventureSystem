using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using SoloAdventureSystem.ContentGenerator.Adapters;
using SoloAdventureSystem.ContentGenerator.Models;
using System.Text.Json;

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

        // Primary prompt and alternate
        var primaryPrompt = PromptTemplates.BuildFactionPrompt(string.Empty, context.Options);
        var altPrompt = $@"Produce a JSON object with fields: name (short), description (2-4 sentences), ideology (single word).\nWorld: {context.Options.Name}\nTheme: {context.Options.Theme}\nPlot: {context.Options.MainPlotPoint}\nReturn only JSON.";

        try
        {
            var rawStructured = GenerationValidator.EnsureStructuredOrFallback(
                p => _slm.GenerateRaw(p),
                primaryPrompt,
                new[] { altPrompt },
                raw => {
                    if (string.IsNullOrWhiteSpace(raw)) return false;
                    return raw.Contains("{") && (raw.Contains("\"name\"") || raw.Contains("\"description\""));
                },
                () => _slm.GenerateFactionFlavor(primaryPrompt),
                _logger);

            Dictionary<string, object>? parsed = null;
            try
            {
                if (!string.IsNullOrWhiteSpace(rawStructured))
                {
                    // Try parse as a single object
                    if (!_structuredParser.TryParse<Dictionary<string, object>>(rawStructured, out parsed))
                    {
                        parsed = null;
                    }

                    // If single-object parse failed, try parsing as array and take first element
                    if (parsed == null)
                    {
                        if (_structuredParser.TryParse<List<Dictionary<string, object>>>(rawStructured, out var list) && list != null && list.Count > 0)
                        {
                            parsed = list.First();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogDebug(ex, "Structured parse failed for faction output");
                parsed = null;
            }

            if (parsed != null)
            {
                factionName = GenerationUtils.CleanParsedField(ExtractValue(parsed, "name"));
                factionDescRaw = GenerationUtils.CleanParsedField(ExtractValue(parsed, "description"), 1200);
                ideology = GenerationUtils.CleanParsedField(ExtractValue(parsed, "ideology"), 40) ?? ideology;
            }
            else
            {
                // rawStructured may be the fallback cleaned text
                factionDescRaw = GenerationUtils.CleanParsedField(rawStructured, 1200);
            }
        }
        catch (Exception ex)
        {
            lastEx = ex;
            _logger?.LogWarning(ex, "Failed to generate faction");
        }

        if (string.IsNullOrWhiteSpace(factionDescRaw))
        {
            // Capture diagnostic raw outputs to help troubleshooting
            try
            {
                var primaryRaw = string.Empty;
                var fallbackRaw = string.Empty;
                try { primaryRaw = _slm.GenerateRaw(primaryPrompt) ?? string.Empty; } catch (Exception e) { primaryRaw = "<generateRaw threw: " + e.Message + ">"; }
                try { fallbackRaw = _slm.GenerateFactionFlavor(primaryPrompt) ?? string.Empty; } catch (Exception e) { fallbackRaw = "<generateFactionFlavor threw: " + e.Message + ">"; }

                var pSnippet = primaryRaw.Length > 500 ? primaryRaw.Substring(0, 500) + "..." : primaryRaw;
                var fSnippet = fallbackRaw.Length > 500 ? fallbackRaw.Substring(0, 500) + "..." : fallbackRaw;

                _logger?.LogError("Faction generation produced empty description. Primary raw (len={LenP}): {SnippetP}\nFallback raw (len={LenF}): {SnippetF}", primaryRaw.Length, pSnippet, fallbackRaw.Length, fSnippet);
            }
            catch (Exception logEx)
            {
                _logger?.LogDebug(logEx, "Failed to capture diagnostic raw outputs");
            }

            // Don't throw - return a safe default faction to keep pipeline running
            _logger?.LogWarning("Faction generation failed; using safe default faction to continue generation pipeline.");
            factionDescRaw = "A shadowy faction whose details are lost to rumor and time. Their true motives remain unclear.";
            if (string.IsNullOrWhiteSpace(factionName)) factionName = "The Collective";
            ideology = ideology ?? "Neutral";
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

    private static string? ExtractValue(Dictionary<string, object> parsed, string key)
    {
        if (!parsed.TryGetValue(key, out var val) || val == null) return null;

        if (val is JsonElement je)
        {
            if (je.ValueKind == JsonValueKind.String) return je.GetString();
            return je.ToString();
        }

        if (val is string s) return s;

        try { return val.ToString(); } catch { return null; }
    }
}
