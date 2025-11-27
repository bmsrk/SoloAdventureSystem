using SoloAdventureSystem.ContentGenerator.Models;
using System;

namespace SoloAdventureSystem.ContentGenerator.Generation;

/// <summary>
/// Provides enhanced AI prompt templates with few-shot learning examples.
/// These prompts guide AI models to generate high-quality, atmospheric content.
/// Templates now support custom world parameters for diverse generation and a
/// standardized schema: System + User + OutputSpec.
/// </summary>
public static class PromptTemplates
{
    private const string CommonConstraints =
        "Constraints:\n- Do NOT add any extra commentary, labels, or explanations.\n- Do NOT repeat the prompt or include instruction fragments in the output.\n- Output must strictly follow the JSON schema in the OUTPUT SPEC section.";

    // --- System prompts (role + brief requirements) ---
    public static string RoomDescriptionSystem =>
        @"You are a professional game writer for interactive text-adventure settings. Produce concise, vivid room descriptions focused on imagery and sensory detail.\n\nRequirements:\n- Provide only the data requested by the OUTPUT SPEC section.\n- Avoid examples, meta-commentary, or any additional text outside the JSON object.\n" + CommonConstraints;

    public static string NpcBioSystem =>
        @"You are a professional game writer for compact NPC biographies suitable for game entries. Keep language direct and useful for gameplay.\n\nRequirements:\n- Provide only the data requested by the OUTPUT SPEC section.\n- Avoid examples, meta-commentary, or any additional text outside the JSON object.\n" + CommonConstraints;

    public static string FactionLoreSystem =>
        @"You are a professional game writer for factions and organizations. Produce concise faction lore that can be used directly in-game.\n\nRequirements:\n- Provide only the data requested by the OUTPUT SPEC section.\n- Avoid examples, meta-commentary, or any additional text outside the JSON object.\n" + CommonConstraints;

    public static string WorldLoreSystem =>
        @"You are a professional game writer for short world-lore entries. Produce brief, evocative cultural or historical notes.\n\nRequirements:\n- Provide only the data requested by the OUTPUT SPEC section.\n- Avoid examples, meta-commentary, or any additional text outside the requested output.\n" + CommonConstraints;

    // --- Output specifications (explicit schema-only constraints) ---
    public static string RoomOutputSpec =>
        "OUTPUT SPEC: Return a single JSON object exactly matching this schema: { \"title\": string (1-4 words), \"description\": string (2-4 sentences). }\n" +
        "Do NOT include any other fields, examples, or explanation. Return ONLY the JSON object literal with those fields.";

    public static string NpcOutputSpec =>
        "OUTPUT SPEC: Return a single JSON object exactly matching this schema: { \"name\": string (short, human-readable), \"bio\": string (2 sentences). }\n" +
        "Do NOT include any other fields, examples, or explanation. Return ONLY the JSON object literal with those fields.";

    public static string FactionOutputSpec =>
        "OUTPUT SPEC: Return a single JSON object exactly matching this schema: { \"name\": string (short), \"description\": string (2-4 sentences), \"ideology\": string (single word). }\n" +
        "Do NOT include any other fields, examples, or explanation. Return ONLY the JSON object literal with those fields.";

    public static string LoreOutputSpec =>
        "OUTPUT SPEC: Return ONLY 1-2 sentences of plain text (no JSON). Provide a single evocative sentence or two focusing on one concrete detail or event. Do NOT include examples or commentary.";

    // --- Builders: produce User prompt combined with OutputSpec for use by adapters ---
    public static string BuildRoomPrompt(string roomName, WorldGenerationOptions options, string atmosphere, int index, int total)
    {
        // Sanitize user-provided fields to avoid prompt injection and excessive length
        var roomNameSafe = SanitizeField(roomName, 80);
        var worldDesc = SanitizeField(options.Description, 800);
        var flavor = SanitizeField(options.Flavor, 200);
        var time = SanitizeField(options.TimePeriod, 120);
        var plot = SanitizeField(options.MainPlotPoint, 400);
        var atmosphereSafe = SanitizeField(atmosphere, 120);

        var user = $@"Context:\nWorldName: {roomNameSafe} | WorldDescription: {worldDesc} | Mood: {flavor} | Time: {time} | Plot: {plot} | RoomIndex: {index + 1} of {total} | Atmosphere: {atmosphereSafe}\n\nProduce the requested JSON object according to the OUTPUT SPEC section. Do NOT include examples or commentary.";

        var combined = Combine(RoomDescriptionSystem, user, RoomOutputSpec);
        return combined;
    }

    public static string BuildNpcPrompt(string npcName, WorldGenerationOptions options, string roomContext, string factionName)
    {
        var npcNameSafe = SanitizeField(npcName, 80);
        var roomContextSafe = SanitizeField(roomContext, 200);
        var factionSafe = SanitizeField(factionName, 120);
        var worldDesc = SanitizeField(options.Description, 800);
        var flavor = SanitizeField(options.Flavor, 200);
        var time = SanitizeField(options.TimePeriod, 120);
        var society = SanitizeField(options.PowerStructure, 200);
        var plot = SanitizeField(options.MainPlotPoint, 400);

        var user = $@"Context:\nNPC: {npcNameSafe} | Location: {roomContextSafe} | Faction: {factionSafe} | World: {worldDesc} | Mood: {flavor} | Era: {time} | Society: {society} | Plot: {plot}\n\nProduce the requested JSON object according to the OUTPUT SPEC section. Do NOT include examples or commentary.";

        var combined = Combine(NpcBioSystem, user, NpcOutputSpec);
        return combined;
    }

    public static string BuildFactionPrompt(string factionName, WorldGenerationOptions options)
    {
        var factionSafe = SanitizeField(factionName, 120);
        var worldDesc = SanitizeField(options.Description, 800);
        var flavor = SanitizeField(options.Flavor, 200);
        var time = SanitizeField(options.TimePeriod, 120);
        var power = SanitizeField(options.PowerStructure, 200);
        var plot = SanitizeField(options.MainPlotPoint, 400);

        var user = $@"Context:\nFaction: {factionSafe} | World: {worldDesc} | Mood: {flavor} | Era: {time} | PowerStructure: {power} | Conflict: {plot}\n\nProduce the requested JSON object according to the OUTPUT SPEC section. Do NOT include examples or commentary.";

        var combined = Combine(FactionLoreSystem, user, FactionOutputSpec);
        return combined;
    }

    public static string BuildLorePrompt(string worldName, WorldGenerationOptions options, int entryNumber)
    {
        var worldSafe = SanitizeField(worldName, 200);
        var worldDesc = SanitizeField(options.Description, 800);
        var flavor = SanitizeField(options.Flavor, 200);
        var time = SanitizeField(options.TimePeriod, 120);
        var plot = SanitizeField(options.MainPlotPoint, 400);

        var user = $@"Context:\nWorld: {worldSafe} | Setting: {worldDesc} | Mood: {flavor} | Era: {time} | Context: {plot} | EntryNumber: {entryNumber}\n\nProduce the requested short lore text according to the OUTPUT SPEC section. Do NOT include examples or commentary.";

        var combined = Combine(WorldLoreSystem, user, LoreOutputSpec);
        return combined;
    }

    // Backwards-compatible obsolete overloads
    [Obsolete("Use BuildRoomPrompt with WorldGenerationOptions instead")]
    public static string BuildRoomPrompt(string roomName, string theme, string atmosphere, int index, int total)
    {
        var options = new WorldGenerationOptions { Theme = theme };
        return BuildRoomPrompt(roomName, options, atmosphere, index, total);
    }

    [Obsolete("Use BuildNpcPrompt with WorldGenerationOptions instead")]
    public static string BuildNpcPrompt(string npcName, string theme, string roomContext, string factionName)
    {
        var options = new WorldGenerationOptions { Theme = theme };
        return BuildNpcPrompt(npcName, options, roomContext, factionName);
    }

    [Obsolete("Use BuildFactionPrompt with WorldGenerationOptions instead")]
    public static string BuildFactionPrompt(String factionName, string theme, int worldSeed)
    {
        var options = new WorldGenerationOptions { Theme = theme };
        return BuildFactionPrompt(factionName, options);
    }

    [Obsolete("Use BuildLorePrompt with WorldGenerationOptions instead")]
    public static string BuildLorePrompt(string worldName, string theme, int entryNumber)
    {
        var options = new WorldGenerationOptions { Theme = theme };
        return BuildLorePrompt(worldName, options, entryNumber);
    }

    // --- Helpers ---
    private static string Combine(string system, string user, string outputSpec)
    {
        // Some adapters support system messages separately; for adapters that don't,
        // send a single combined string with clear sections. Keep concise ordering: system, user, outputspec.
        return system + "\n\n" + user + "\n\n" + outputSpec;
    }

    // Sanitize and tighten user-supplied fields to avoid prompt injection and overly long prompts
    private static string SanitizeField(string? input, int maxLen = 400)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;
        try
        {
            var s = input.Trim();
            // Collapse whitespace
            s = System.Text.RegularExpressions.Regex.Replace(s, "\\s+", " ");
            // Remove backticks, code fences and JSON markers
            s = System.Text.RegularExpressions.Regex.Replace(s, "```.*?```", "", System.Text.RegularExpressions.RegexOptions.Singleline);
            s = s.Replace("`", "");
            // Remove common instruction fragments that may leak into user text
            s = System.Text.RegularExpressions.Regex.Replace(s, "(?i)\\b(return only|return only the json object|return only the text|output spec|output:).*", "");
            // Strip leading/trailing punctuation
            s = s.Trim(' ', '\n', '\r', '\t', '"', '\'', ':');
            // Truncate to maxLen, preserving whole words where possible
            if (s.Length > maxLen)
            {
                var cut = s.Substring(0, maxLen);
                var lastSpace = cut.LastIndexOf(' ');
                if (lastSpace > 0) cut = cut.Substring(0, lastSpace);
                s = cut.Trim() + "...";
            }
            return s;
        }
        catch
        {
            return input.Substring(0, Math.Min(maxLen, input.Length));
        }
    }
}
