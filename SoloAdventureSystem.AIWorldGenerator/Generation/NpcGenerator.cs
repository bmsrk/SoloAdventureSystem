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
        _logger?.LogDebug("NPC generation context: Theme={Theme}, Flavor={Flavor}, Faction={Faction}", context.Options.Theme, context.Options.Flavor, context.Factions.FirstOrDefault()?.Name ?? "None");

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

        // Build primary prompt
        var primaryPrompt = PromptTemplates.BuildNpcPrompt(string.Empty, context.Options, room.Title, faction.Name);

        // Alternate stricter prompt
        var altPrompt = $@"Produce a short JSON object with fields: name (short), bio (2 sentences), role (optional), trait (optional).\nContext: World={context.Options.Name}, Room={room.Title}, Faction={faction.Name}, Theme={context.Options.Theme}\nReturn only JSON.";

        // Use validator to attempt structured output, fall back to cleaned npc bio
        var rawStructured = GenerationValidator.EnsureStructuredOrFallback(
            p => _slm.GenerateRaw(p),
            primaryPrompt,
            new[] { altPrompt },
            raw => {
                // Quick structured validator: must contain { and "name"
                if (string.IsNullOrWhiteSpace(raw)) return false;
                return raw.Contains("{") && raw.Contains("\"name\"");
            },
            () => _slm.GenerateNpcBio(primaryPrompt),
            _logger);

        // Try parse structured
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
            _logger?.LogDebug(ex, "Structured parser threw for raw NPC output");
            parsed = null;
        }

        if (parsed != null)
        {
            name = ExtractValue(parsed, "name") ?? string.Empty;
            bioRaw = ExtractValue(parsed, "bio") ?? string.Empty;
            role = ExtractValue(parsed, "role") ?? string.Empty;
            trait = ExtractValue(parsed, "trait") ?? string.Empty;
        }
        else
        {
            // rawStructured may already be cleaned fallback (from GenerateNpcBio)
            bioRaw = rawStructured ?? string.Empty;
            bioRaw = GenerationUtils.SanitizeGeneratedText(bioRaw);
        }

        if (string.IsNullOrWhiteSpace(bioRaw))
        {
            // Capture diagnostic raw outputs to help troubleshooting
            try
            {
                var primaryRaw = string.Empty;
                var fallbackRaw = string.Empty;
                try { primaryRaw = _slm.GenerateRaw(primaryPrompt) ?? string.Empty; } catch (Exception e) { primaryRaw = "<generateRaw threw: " + e.Message + ">"; }
                try { fallbackRaw = _slm.GenerateNpcBio(primaryPrompt) ?? string.Empty; } catch (Exception e) { fallbackRaw = "<generateNpcBio threw: " + e.Message + ">"; }

                var pSnippet = primaryRaw.Length > 500 ? primaryRaw.Substring(0, 500) + "..." : primaryRaw;
                var fSnippet = fallbackRaw.Length > 500 ? fallbackRaw.Substring(0, 500) + "..." : fallbackRaw;

                _logger?.LogError("NPC generation produced empty bio for index {Index}. Primary raw (len={LenP}): {SnippetP}\nFallback raw (len={LenF}): {SnippetF}", index + 1, primaryRaw.Length, pSnippet, fallbackRaw.Length, fSnippet);
            }
            catch (Exception logEx)
            {
                _logger?.LogDebug(logEx, "Failed to capture diagnostic raw outputs for NPC generation");
            }

            // Use a safe default bio and name so world generation can continue
            _logger?.LogWarning("NPC bio generation failed for index {Index}; using fallback bio.", index + 1);
            bioRaw = "An unremarkable local who prefers to avoid attention; details lost to time.";
            if (string.IsNullOrWhiteSpace(name)) name = $"NPC{index + 1}";
        }

        // Ensure bio is cleaned up for in-game display
        bioRaw = GenerationUtils.NormalizeWhitespace(bioRaw);

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

    // Try to find a human-like name (prefer two capitalized words) in the bio. Filter out single short words
    // that look like prompt fragments (e.g., 'Can', 'Could', 'Please').
    private static string? ExtractNameFromBio(string bio)
    {
        if (string.IsNullOrWhiteSpace(bio)) return null;

        // Remove any role markers that might have leaked
        var cleaned = Regex.Replace(bio, @"<\|/?(user|assistant|system)\|>", "", RegexOptions.IgnoreCase);

        // Try to find two-word capitalized name first
        var twoWordMatch = Regex.Match(cleaned, @"\b([A-Z][\\p{L}'\\-]{1,20})\s+([A-Z][\\p{L}'\\-]{1,20})\b");
        if (twoWordMatch.Success)
        {
            var candidate = twoWordMatch.Groups[1].Value + " " + twoWordMatch.Groups[2].Value;
            if (candidate.Length <= 40) return candidate;
            return candidate.Substring(0, 40).Trim();
        }

        // Fall back to single capitalized word but exclude common prompt words
        var nameMatch = Regex.Match(cleaned, @"\b([A-Z][\\p{L}'\\-]{1,20})\b");
        if (nameMatch.Success)
        {
            var candidate = nameMatch.Groups[1].Value;
            var lower = candidate.ToLowerInvariant();
            var blacklist = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "can", "could", "please", "return", "example", "user", "assistant", "system" };
            if (blacklist.Contains(lower)) return null;
            if (candidate.Length <= 40) return candidate;
            return candidate.Substring(0, 40).Trim();
        }

        return null;
    }

    private static string? ExtractValue(Dictionary<string, object> parsed, string key)
    {
        if (!parsed.TryGetValue(key, out var val) || val == null) return null;

        if (val is System.Text.Json.JsonElement je)
        {
            if (je.ValueKind == JsonValueKind.String) return je.GetString();
            return je.ToString();
        }

        if (val is string s) return s;

        try { return val.ToString(); } catch { return null; }
    }
}
