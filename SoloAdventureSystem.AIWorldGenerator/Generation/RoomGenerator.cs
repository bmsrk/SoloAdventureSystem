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

        // Try up to 3 attempts: initial rich prompt, then simpler, then structured JSON prompt
        for (int attempt = 0; attempt < 3; attempt++)
        {
            try
            {
                string roomPrompt;
                if (attempt == 0)
                {
                    // Rich prompt using world options for context, ask model to produce a JSON-like object with title and description
                    roomPrompt = PromptTemplates.BuildRoomPrompt(
                        string.Empty, // let the model pick the room name/title
                        context.Options,
                        context.Options.Flavor ?? string.Empty,
                        index,
                        total);
                }
                else if (attempt == 1)
                {
                    // Simpler instruction asking for title + description in JSON
                    roomPrompt = $@"You are a creative writer for a text-adventure game.
World: {context.Options.Name}
Theme: {context.Options.Theme}
Mood: {context.Options.Flavor}
Plot: {context.Options.MainPlotPoint}

Produce a short JSON object with fields: title (a short, evocative room name) and description (2-4 sentences, vivid sensory detail). Example: {{ ""title"": ""Neon Alley"", ""description"": ""..."" }}
Return ONLY the JSON object.";
                }
                else
                {
                    // Enforce structured output
                    roomPrompt = $@"Return a JSON object with exactly two fields: title and description.
Title: short (1-4 words). Description: 2-4 sentences with sensory details. World: {context.Options.Name}. Theme: {context.Options.Theme}.
Return only JSON.";
                }

                descriptionRaw = _slm.GenerateRoomDescription(roomPrompt, roomSeed + attempt);

                // Request raw output and attempt structured parse locally (avoid double-cleaning)
                var raw = _slm.GenerateRaw(roomPrompt, roomSeed + attempt);

                // Try structured parsing against raw output first
                Dictionary<string, object>? source = null;
                try
                {
                    if (!string.IsNullOrWhiteSpace(raw))
                    {
                        _structuredParser.TryParse<Dictionary<string, object>>(raw, out source);
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogDebug(ex, "Structured parser threw for raw room output on attempt {Attempt}", attempt + 1);
                    source = null;
                }

                if (source != null)
                {
                    if (source.TryGetValue("title", out var t) && t != null) title = t.ToString() ?? string.Empty;
                    if (source.TryGetValue("description", out var d) && d != null) descriptionRaw = d.ToString() ?? string.Empty;

                    // items/exits optional (handle JsonElement cases defensively)
                    if (source.TryGetValue("items", out var itms) && itms != null)
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

                    if (source.TryGetValue("exits", out var exs) && exs != null)
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
                    // Structured parse failed - fall back to cleaned text from adapter
                    // Log raw output for debugging (trim large content)
                    try
                    {
                        var dbg = string.IsNullOrWhiteSpace(raw) ? "(empty)" : (raw.Length > 800 ? raw.Substring(0, 800) + "..." : raw);
                        _logger?.LogDebug("Raw generation (room attempt {Attempt}): {Raw}", attempt + 1, dbg);
                    }
                    catch { }

                    // Use cleaned/legacy method to obtain description
                    descriptionRaw = _slm.GenerateRoomDescription(roomPrompt, roomSeed + attempt);

                    // Apply improved light sanitization to cleaned text
                    descriptionRaw = SanitizeGeneratedText(descriptionRaw);
                }

                // If description present and non-empty, break
                if (!string.IsNullOrWhiteSpace(descriptionRaw))
                {
                    break;
                }

                _logger?.LogInformation("Attempt {Attempt} produced empty description, retrying...", attempt + 1);
            }
            catch (Exception ex)
            {
                lastEx = ex;
                _logger?.LogWarning(ex, "Failed to generate room on attempt {Attempt}", attempt + 1);
            }
        }

        if (string.IsNullOrWhiteSpace(descriptionRaw))
        {
            var msg = "Failed to generate description for room (non-empty generation failed)";
            if (lastEx != null) msg += ": " + lastEx.Message;
            throw new InvalidOperationException(msg, lastEx);
        }

        // If no title provided by AI, derive short title from first sentence (fallback)
        if (string.IsNullOrWhiteSpace(title))
        {
            var firstSentence = descriptionRaw.Split(new[] {'.','!','?'}, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? string.Empty;
            // Remove prompt-like trailing fragments from title
            firstSentence = Regex.Replace(firstSentence, "(Could you|Please|Return only).*", "", RegexOptions.IgnoreCase);
            // Clean common artifacts like hashtags, TOON markers, and (s)
            firstSentence = Regex.Replace(firstSentence, "(?i)\\b#?TOON\\b", "");
            firstSentence = Regex.Replace(firstSentence, "(?i)\\b#?ENDTOON\\b", "");
            firstSentence = Regex.Replace(firstSentence, "#\\w+", "");
            firstSentence = Regex.Replace(firstSentence, "\\(s\\)", "");
            title = firstSentence.Length > 0 ? (firstSentence.Length <= 30 ? firstSentence : firstSentence.Substring(0, 30).Trim()) : $"Room {index + 1}";
            title = title.Trim();
        }

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

    private static string SanitizeGeneratedText(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;

        // Collapse obvious '# #' artifacts and solitary hashes that leak from model output
        text = Regex.Replace(text, @"#\s*#", " ", RegexOptions.Compiled);
        text = Regex.Replace(text, @"\s#\s", " ", RegexOptions.Compiled);

        // Remove leading instruction markers like '#json', leading quote, or code fences (```json)
        text = Regex.Replace(text, @"^\s*(?:#json\n|""|```json|```)\.*", "", RegexOptions.IgnoreCase);

        // Remove 'Return only JSON' or similar trailing instructions

        // Remove common prompt fragments that sometimes appear when model repeats prompt
        text = Regex.Replace(text, "You are a creative writer.*", "", RegexOptions.IgnoreCase);

        // Trim and normalize whitespace
        text = Regex.Replace(text, @"\s+", " ").Trim();

        // Remove TOON/ENDTOON markers and hashtags
        try
        {
            text = Regex.Replace(text, "(?i)\\b#?TOON\\b", "");
            text = Regex.Replace(text, "(?i)\\b#?ENDTOON\\b", "");
            text = Regex.Replace(text, "#\\w+", "");
            text = Regex.Replace(text, "\\(s\\)", "");

            // Collapse repeated sentences/fragments
            var sentences = Regex.Split(text, "(?<=[\\.!?])\\s+");
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var sb = new List<string>();
            foreach (var s in sentences)
            {
                var t = s.Trim();
                if (string.IsNullOrEmpty(t)) continue;
                var key = Regex.Replace(t.ToLowerInvariant(), "[\\p{P}\\s]+", " ").Trim();
                if (!seen.Contains(key)) { sb.Add(t); seen.Add(key); }
            }
            if (sb.Count > 0) text = string.Join(" ", sb);
            text = Regex.Replace(text, "\\s+", " ").Trim();
        }
        catch { }

        return text;
    }
}
