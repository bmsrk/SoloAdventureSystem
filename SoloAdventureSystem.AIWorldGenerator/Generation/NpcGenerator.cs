using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using SoloAdventureSystem.ContentGenerator.Adapters;
using SoloAdventureSystem.ContentGenerator.Models;
using System.Text.Json;

namespace SoloAdventureSystem.ContentGenerator.Generation;

/// <summary>
/// Specialized generator for NPC content.
/// Creates NPCs with AI-generated personalities and assigns them to rooms.
/// </summary>
public class NpcGenerator : IContentGenerator<List<NpcModel>>
{
    private readonly ILocalSLMAdapter _slm;
    private readonly ILogger<NpcGenerator>? _logger;
    private readonly SoloAdventureSystem.ContentGenerator.Parsing.IStructuredOutputParser _structuredParser;

    public NpcGenerator(ILocalSLMAdapter slm, ILogger<NpcGenerator>? logger = null, SoloAdventureSystem.ContentGenerator.Parsing.IStructuredOutputParser? structuredParser = null)
    {
        _slm = slm ?? throw new ArgumentNullException(nameof(slm));
        _logger = logger;
        _structuredParser = structuredParser ?? new SoloAdventureSystem.ContentGenerator.Parsing.JsonStructuredOutputParser();
    }

    public List<NpcModel> Generate(WorldGenerationContext context)
    {
        if (context.Rooms.Count == 0)
        {
            _logger?.LogWarning("No rooms available for NPC placement");
            return new List<NpcModel>();
        }

        if (context.Factions.Count == 0)
        {
            _logger?.LogWarning("No factions available for NPC assignment");
            return new List<NpcModel>();
        }

        var npcCount = context.Rooms.Count;
        _logger?.LogInformation("?? Generating {Count} NPCs with enhanced personalities...", npcCount);

        var npcs = new List<NpcModel>();
        var faction = context.Factions[0]; // Use first faction

        for (int i = 0; i < npcCount; i++)
        {
            var npc = GenerateNpc(context, i, faction, context.Rooms[i]);
            npcs.Add(npc);
            
            // Assign NPC to room
            context.Rooms[i].Npcs.Add(npc.Id);
        }

        _logger?.LogInformation("? Successfully generated {Count} NPCs", npcs.Count);
        return npcs;
    }

    // Made internal to support deterministic per-NPC testing. Optional overrideSeed lets tests
    // generate an NPC with a specific seed rather than relying on context-derived seed.
    internal NpcModel GenerateNpc(
        WorldGenerationContext context, 
        int index, 
        FactionModel faction,
        RoomModel room,
        int? overrideSeed = null)
    {
        var npcId = $"npc{index + 1}";
        // Use context.Random for per-run randomness; removing global seed exposure
        var npcSeed = overrideSeed ?? context.Random.Next();

        _logger?.LogDebug("Generating NPC {Index}/{Total} (seed {Seed})", index + 1, context.Rooms.Count, npcSeed);

        string name = string.Empty;
        string bioRaw = string.Empty;
        string role = string.Empty;
        string trait = string.Empty;

        // Try AI prompt to generate structured NPC with name and bio
        for (int attempt = 0; attempt < 3; attempt++)
        {
            try
            {
                string prompt;
                if (attempt == 0)
                {
                    prompt = PromptTemplates.BuildNpcPrompt(string.Empty, context.Options, room.Title, faction.Name);
                }
                else
                {
                    prompt = $@"Produce a short JSON object with fields: name (short), bio (2 sentences), role (optional), trait (optional).
Context: World={context.Options.Name}, Room={room.Title}, Faction={faction.Name}, Theme={context.Options.Theme}
Return only JSON.";
                }

                bioRaw = _slm.GenerateNpcBio(prompt, npcSeed + attempt);

                // Sanitize AI output before parsing or using it
                bioRaw = SanitizeGeneratedText(bioRaw);

                try
                {
                    Dictionary<string, object>? parsed = null;
                    try
                    {
                        _structuredParser.TryParse<Dictionary<string, object>>(bioRaw, out parsed);
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogDebug(ex, "Structured parser threw for NPC output on attempt {Attempt}", attempt + 1);
                        parsed = null;
                    }

                    if (parsed != null)
                    {
                        if (parsed.TryGetValue("name", out var n) && n != null) name = n.ToString() ?? string.Empty;
                        if (parsed.TryGetValue("bio", out var b) && b != null) bioRaw = b.ToString() ?? string.Empty;
                        if (parsed.TryGetValue("role", out var r) && r != null) role = r.ToString() ?? string.Empty;
                        if (parsed.TryGetValue("trait", out var t) && t != null) trait = t.ToString() ?? string.Empty;
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogDebug(ex, "Structured parse failed for NPC output on attempt {Attempt}", attempt + 1);
                }

                if (!string.IsNullOrWhiteSpace(bioRaw)) break;
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Failed to generate NPC bio on attempt {Attempt}", attempt + 1);
            }
        }

        if (string.IsNullOrWhiteSpace(bioRaw))
        {
            throw new InvalidOperationException($"Failed to generate non-empty bio for NPC index {index + 1}");
        }

        // Ensure bio is cleaned up for in-game display
        bioRaw = NormalizeWhitespace(bioRaw);

        if (string.IsNullOrWhiteSpace(name))
        {
            // Improved fallback: try to extract a likely proper name from the bio using regex
            var extracted = ExtractNameFromBio(bioRaw);
            if (!string.IsNullOrWhiteSpace(extracted))
            {
                name = extracted;
            }
            else
            {
                // Ultimate fallback: use generic NPC id
                name = $"NPC{index + 1}";
            }
        }

        var npc = new NpcModel
        {
            Id = npcId,
            Name = name,
            Description = bioRaw + (string.IsNullOrWhiteSpace(trait) ? string.Empty : " " + trait),
            FactionId = faction.Id,
            Hostility = "Neutral",
            Attributes = new NpcAttributes(),
            Behavior = string.IsNullOrWhiteSpace(role) ? "Static" : role,
            Inventory = new List<string>()
        };

        _logger?.LogDebug("? NPC {Index} generated: {NpcName}", index + 1, name);
        return npc;
    }

    // Basic sanitization to remove prompt artifacts and normalize whitespace
    private static string SanitizeGeneratedText(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;

        // Remove common instruction fragments that may leak into output
        var lines = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        var kept = new List<string>();
        foreach (var raw in lines)
        {
            var line = raw.Trim();
            if (string.IsNullOrWhiteSpace(line)) continue;

            // Skip lines that look like instructions or UI hints
            var lower = line.ToLowerInvariant();
            if (lower.StartsWith("could you") || lower.StartsWith("please") || lower.StartsWith("return only") || lower.StartsWith("produce") || lower.StartsWith("example"))
                continue;

            kept.Add(line);
        }

        var joined = string.Join(" ", kept);
        var cleaned = NormalizeWhitespace(joined);

        // Remove TOON markers and hashtag tokens
        try
        {
            cleaned = System.Text.RegularExpressions.Regex.Replace(cleaned, "(?i)\\b#?TOON\\b", "");
            cleaned = System.Text.RegularExpressions.Regex.Replace(cleaned, "(?i)\\b#?ENDTOON\\b", "");
            cleaned = System.Text.RegularExpressions.Regex.Replace(cleaned, "#\\w+", "");
            // Remove (s) plural markers like a(s) -> a
            cleaned = System.Text.RegularExpressions.Regex.Replace(cleaned, "\\(s\\)", "");

            // Dedupe repeated short sentences/fragments
            var sentences = System.Text.RegularExpressions.Regex.Split(cleaned, "(?<=[\\.!?])\\s+");
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var parts = new List<string>();
            foreach (var s in sentences)
            {
                var t = s.Trim();
                if (string.IsNullOrEmpty(t)) continue;
                var key = System.Text.RegularExpressions.Regex.Replace(t.ToLowerInvariant(), "[\\p{P}\\s]+", " ").Trim();
                if (!seen.Contains(key)) { parts.Add(t); seen.Add(key); }
            }
            if (parts.Count > 0) cleaned = string.Join(" ", parts);
            cleaned = NormalizeWhitespace(cleaned);
        }
        catch { }

        return cleaned;
    }

    private static string NormalizeWhitespace(string s)
    {
        if (string.IsNullOrEmpty(s)) return s;
        var outStr = Regex.Replace(s, "\\s+", " ").Trim();
        return outStr;
    }

    // Try to find a human-like name (one or two capitalized words) in the bio
    private static string? ExtractNameFromBio(string bio)
    {
        if (string.IsNullOrWhiteSpace(bio)) return null;

        // Look for patterns like 'Marcus Chen' or single capitalized name 'Marcus'
        var nameMatch = Regex.Match(bio, "\\b([A-Z][a-z]{1,20}(?: [A-Z][a-z]{1,20})?)\\b");
        if (nameMatch.Success)
        {
            var candidate = nameMatch.Groups[1].Value;
            if (candidate.Length <= 20) return candidate;
            return candidate.Substring(0, 20).Trim();
        }

        return null;
    }
}
