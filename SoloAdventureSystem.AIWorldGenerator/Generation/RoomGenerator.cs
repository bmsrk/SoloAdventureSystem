using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using SoloAdventureSystem.ContentGenerator.Adapters;
using SoloAdventureSystem.ContentGenerator.Models;

namespace SoloAdventureSystem.ContentGenerator.Generation;

/// <summary>
/// Specialized generator for room/location content.
/// Handles room creation with AI-generated descriptions and visual prompts.
/// </summary>
public class RoomGenerator : IContentGenerator<List<RoomModel>>
{
    private readonly ILocalSLMAdapter _slm;
    private readonly IImageAdapter _image;
    private readonly ILogger<RoomGenerator>? _logger;
    private readonly SoloAdventureSystem.ContentGenerator.Parsing.IStructuredOutputParser _structuredParser;

    public RoomGenerator(
        ILocalSLMAdapter slm,
        IImageAdapter image,
        ILogger<RoomGenerator>? logger = null,
        SoloAdventureSystem.ContentGenerator.Parsing.IStructuredOutputParser? structuredParser = null)
    {
        _slm = slm ?? throw new ArgumentNullException(nameof(slm));
        _image = image ?? throw new ArgumentNullException(nameof(image));
        _logger = logger;
        _structuredParser = structuredParser ?? new SoloAdventureSystem.ContentGenerator.Parsing.JsonStructuredOutputParser();
    }

    public List<RoomModel> Generate(WorldGenerationContext context)
    {
        var roomCount = context.GetRoomCount();
        _logger?.LogInformation("?? Generating {Count} rooms with enhanced quality...", roomCount);
        _logger?.LogDebug("Room generation context: Theme={Theme}, Flavor={Flavor}, Regions={Regions}", context.Options.Theme, context.Options.Flavor, context.Options.Regions);

        var rooms = new List<RoomModel>();

        for (int i = 0; i < roomCount; i++)
        {
            rooms.Add(GenerateRoom(context, i, roomCount));
        }

        _logger?.LogInformation("? Successfully generated {Count} rooms", rooms.Count);
        return rooms;
    }

    // Made internal to support deterministic per-room testing. Optional overrideSeed lets tests
    // generate a room with a specific seed rather than relying on context-derived seed.
    internal RoomModel GenerateRoom(WorldGenerationContext context, int index, int total, int? overrideSeed = null)
    {
        var roomId = $"room{index + 1}";
        // Use context.Random for per-run randomness; deterministic seeding removed from public API
        var roomSeed = overrideSeed ?? context.Random.Next();

        _logger?.LogDebug("Generating room {Index}/{Total} (seed {Seed})", index + 1, total, roomSeed);

        string descriptionRaw = string.Empty;
        string title = string.Empty;
        var items = new List<string>();
        var exits = new Dictionary<string, string>();
        Exception? lastEx = null;

        // Build primary prompt
        var primaryPrompt = PromptTemplates.BuildRoomPrompt(string.Empty, context.Options, context.Options.Flavor ?? string.Empty, index, total);
        var altPrompt = $@"Return a JSON object with exactly two fields: title and description.\nTitle: short (1-4 words). Description: 2-4 sentences with sensory details. World: {context.Options.Name}. Theme: {context.Options.Theme}. Return only JSON.";

        var rawStructured = GenerationValidator.EnsureStructuredOrFallback(
            p => _slm.GenerateRaw(p),
            primaryPrompt,
            new[] { altPrompt },
            raw => {
                if (string.IsNullOrWhiteSpace(raw)) return false;
                return raw.Contains("{") && raw.Contains("\"description\"");
            },
            () => _slm.GenerateRoomDescription(primaryPrompt),
            _logger);

        Dictionary<string, object>? parsed = null;
        try
        {
            if (!string.IsNullOrWhiteSpace(rawStructured))
            {
                _structuredParser.TryParse<Dictionary<string, object>>(rawStructured, out parsed);
            }
        }
        catch (Exception ex)
        {
            _logger?.LogDebug(ex, "Structured parser threw for room output");
            parsed = null;
        }

        if (parsed != null)
        {
            if (parsed.TryGetValue("title", out var t) && t != null) title = t.ToString() ?? string.Empty;
            if (parsed.TryGetValue("description", out var d) && d != null) descriptionRaw = d.ToString() ?? string.Empty;

            if (parsed.TryGetValue("items", out var itms) && itms != null)
            {
                if (itms is System.Text.Json.JsonElement jeItems && jeItems.ValueKind == System.Text.Json.JsonValueKind.Array)
                {
                    foreach (var el in jeItems.EnumerateArray())
                    {
                        if (el.ValueKind == System.Text.Json.JsonValueKind.String)
                            items.Add(el.GetString()!);
                        else
                            items.Add(el.ToString());
                    }
                }
            }

            if (parsed.TryGetValue("exits", out var exs) && exs != null)
            {
                if (exs is System.Text.Json.JsonElement jeExs && jeExs.ValueKind == System.Text.Json.JsonValueKind.Object)
                {
                    foreach (var prop in jeExs.EnumerateObject())
                    {
                        exits[prop.Name] = prop.Value.GetString() ?? string.Empty;
                    }
                }
            }
        }
        else
        {
            descriptionRaw = rawStructured ?? string.Empty;
            descriptionRaw = GenerationUtils.SanitizeGeneratedText(descriptionRaw);
        }

        // If description still empty, error
        if (string.IsNullOrWhiteSpace(descriptionRaw))
        {
            var msg = "Failed to generate description for room (non-empty generation failed)";
            if (lastEx != null) msg += ": " + lastEx.Message;

            // Capture diagnostic raw outputs to help troubleshooting
            try
            {
                var primaryRaw = string.Empty;
                var fallbackRaw = string.Empty;
                try { primaryRaw = _slm.GenerateRaw(primaryPrompt) ?? string.Empty; } catch (Exception e) { primaryRaw = "<generateRaw threw: " + e.Message + ">"; }
                try { fallbackRaw = _slm.GenerateRoomDescription(primaryPrompt) ?? string.Empty; } catch (Exception e) { fallbackRaw = "<generateRoomDescription threw: " + e.Message + ">"; }

                var pSnippet = primaryRaw.Length > 500 ? primaryRaw.Substring(0, 500) + "..." : primaryRaw;
                var fSnippet = fallbackRaw.Length > 500 ? fallbackRaw.Substring(0, 500) + "..." : fallbackRaw;

                _logger?.LogError("Room generation produced empty description. Primary raw (len={LenP}): {SnippetP}\nFallback raw (len={LenF}): {SnippetF}", primaryRaw.Length, pSnippet, fallbackRaw.Length, fSnippet);
            }
            catch (Exception logEx)
            {
                _logger?.LogDebug(logEx, "Failed to capture diagnostic raw outputs for room generation");
            }

            // Don't throw - use a safe default so world generation can continue and developers can inspect logs
            _logger?.LogWarning("Room generation failed for index {Index}; using fallback description.", index + 1);
            descriptionRaw = "A nondescript room with faded walls and a faint hum in the background. Details are unclear due to data corruption.";

            // Generate a fallback title
            if (string.IsNullOrWhiteSpace(title))
            {
                title = $"Room {index + 1}";
            }
        }

        // If no title provided by AI, derive short title from first sentence (fallback)
        if (string.IsNullOrWhiteSpace(title))
        {
            string GenerateTitleFrom(string desc)
            {
                if (string.IsNullOrWhiteSpace(desc)) return $"Room {index + 1}";
                var first = desc.Split(new[] {'.','!','?'}, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? desc;
                first = Regex.Replace(first, "(Could you|Please|Return only).*", "", RegexOptions.IgnoreCase).Trim();
                first = Regex.Replace(first, "(?i)\\b#?TOON\\b", "");
                first = Regex.Replace(first, "(?i)\\b#?ENDTOON\\b", "");
                first = Regex.Replace(first, "#\\w+", "");
                first = Regex.Replace(first, "\\(s\\)", "");
                var words = first.Split(new[] {' ', '\t'}, StringSplitOptions.RemoveEmptyEntries).Take(4).ToArray();
                if (words.Length == 0) return $"Room {index + 1}";
                var candidate = string.Join(' ', words);
                candidate = string.Join(' ', candidate.Split(' ').Select(w => char.ToUpperInvariant(w[0]) + (w.Length > 1 ? w.Substring(1) : string.Empty)));
                if (candidate.Length < 10 || candidate.Contains("Room", StringComparison.OrdinalIgnoreCase))
                {
                    candidate += $" {index + 1}";
                }
                return candidate;
            }

            title = GenerateTitleFrom(descriptionRaw);
            // Normalize whitespace for title
            title = GenerationUtils.NormalizeWhitespace(title);
        }

        string cleanedDescription = GenerationUtils.SanitizeGeneratedText(descriptionRaw);
        int sentenceCount = Regex.Split(cleanedDescription, "(?<=[\\.!?])\\s+").Where(s => !string.IsNullOrWhiteSpace(s)).Count();
        if (cleanedDescription.Length < 120 || sentenceCount < 2)
        {
            try
            {
                var expandPrompt = $@"Expand the following room description to exactly 3 vivid sentences, preserving details and tone. Return only the expanded description.\n\nOriginal: {cleanedDescription}";
                var expanded = _slm.GenerateRoomDescription(expandPrompt);
                expanded = GenerationUtils.SanitizeGeneratedText(expanded);
                if (!string.IsNullOrWhiteSpace(expanded) && expanded.Length > cleanedDescription.Length)
                {
                    cleanedDescription = expanded;
                }
            }
            catch { }
        }

        // Normalize final description whitespace
        descriptionRaw = GenerationUtils.NormalizeWhitespace(cleanedDescription);

        string visualPrompt;
        try
        {
            visualPrompt = _image.GenerateImagePrompt($"{title} in {context.Options.Name} - {context.Options.Theme}", roomSeed);
        }
        catch (Exception ex)
        {
            _logger?.LogWarning("Failed to generate image prompt for {RoomName}, using fallback: {Message}", title, ex.Message);
            visualPrompt = $"A scene of {title} in {context.Options.Name}";
        }

        var room = new RoomModel
        {
            Id = roomId,
            Title = title,
            BaseDescription = descriptionRaw,
            Exits = exits,
            Items = items,
            Npcs = new List<string>(),
            VisualPrompt = visualPrompt,
            UiPosition = new UiPosition { X = index % 3, Y = index / 3 }
        };

        _logger?.LogDebug("? Room {Index} generated: {RoomName}", index + 1, title);
        return room;
    }
}
