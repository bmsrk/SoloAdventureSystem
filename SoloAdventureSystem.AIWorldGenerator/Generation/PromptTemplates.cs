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
        "Constraints:\n- Do NOT add any extra commentary, labels, or explanations.\n- Do NOT repeat the prompt or include instruction fragments in the output.";

    private const string ReturnMarkers =
        "Return only the text between '#TOON' and '#ENDTOON' on separate lines, with nothing else outside those markers.";

    // --- System prompts (role + examples) ---
    public static string RoomDescriptionSystem =>
        @"You are a creative writer for interactive text-adventure content. Produce concise, vivid descriptions tailored for game content.

Requirements:
- Output exactly 3 sentences.
- Sentence 1: Overall appearance with specific visual details.
- Sentence 2: Key objects and their condition.
- Sentence 3: Atmosphere — sounds, smells, or feeling.

" + CommonConstraints;

    public static string NpcBioSystem =>
        @"You are a creative writer for interactive characters. Produce compact biographies suitable for NPC entries.

Requirements:
- Output exactly 2 sentences.
- Sentence 1: Role, background, and current motivation.
- Sentence 2: A secret, quirk, or defining trait.

" + CommonConstraints;

    public static string FactionLoreSystem =>
        @"You are a creative writer for factions and organizations. Produce concise faction lore suitable for game background entries.

Requirements:
- Output exactly 3 sentences.
- Sentence 1: What they do and why (their cause).
- Sentence 2: Where they operate and their strength.
- Sentence 3: Their main enemy and the conflict.

" + CommonConstraints;

    public static string WorldLoreSystem =>
        @"You are a creative writer for world lore entries. Produce short, evocative historical or cultural notes.

Requirements:
- Output 1 to 2 sentences.
- Focus on a specific event, technology, cultural detail, or historical note that makes the world unique.

" + CommonConstraints;

    // --- Output specifications (explicit constraints) ---
    public static string RoomOutputSpec =>
        "OUTPUT SPEC: Exactly 3 sentences. Each sentence should be 10-45 words. Include at least one sensory detail (sound/smell/touch) and one concrete object or color. No lists, no markup. "
        + ReturnMarkers;

    public static string NpcOutputSpec =>
        "OUTPUT SPEC: Exactly 2 sentences. First sentence: role/background/motivation. Second: secret/quirk/defining trait. Keep length concise (12-40 words per sentence). "
        + ReturnMarkers;

    public static string FactionOutputSpec =>
        "OUTPUT SPEC: Exactly 3 sentences. Include goal, territory/strength, and main enemy/conflict. Use concrete detail and avoid vague language. "
        + ReturnMarkers;

    public static string LoreOutputSpec =>
        "OUTPUT SPEC: 1-2 sentences. Aim for a single concrete detail or event. Keep it evocative and specific. "
        + ReturnMarkers;

    // --- Builders: produce User prompt combined with OutputSpec for use by adapters ---
    public static string BuildRoomPrompt(string roomName, WorldGenerationOptions options, string atmosphere, int index, int total)
    {
        // Add unique room identifier to prevent cache collisions and ensure variety
        var uniqueId = Guid.NewGuid().ToString().Substring(0, 8);
        var user = $@"Room Name: {roomName}
 World: {options.Description}
 Mood: {options.Flavor}
 Time: {options.TimePeriod}
 Context: {options.MainPlotPoint}
 Unique ID: {uniqueId}

 Room {index + 1} of {total}. Make it {atmosphere.ToLower()}. 

 Write 3 sentences describing this room:";

        var combined = Combine(RoomDescriptionSystem, user, RoomOutputSpec);
        return combined;
    }

    public static string BuildNpcPrompt(string npcName, WorldGenerationOptions options, string roomContext, string factionName)
    {
        var user = $@"NPC Name: {npcName}
Location: {roomContext}
Faction: {factionName}
World: {options.Description}
Mood: {options.Flavor}
Era: {options.TimePeriod}
Society: {options.PowerStructure}
Plot: {options.MainPlotPoint}

Write 2 sentences about {npcName}. Show their role and a defining trait:";

        var combined = Combine(NpcBioSystem, user, NpcOutputSpec);
        return combined;
    }

    public static string BuildFactionPrompt(string factionName, WorldGenerationOptions options)
    {
        var user = $@"Faction Name: {factionName}
World: {options.Description}
Mood: {options.Flavor}
Era: {options.TimePeriod}
Power Structure: {options.PowerStructure}
Central Conflict: {options.MainPlotPoint}

Write 3 sentences about {factionName}. Include their goal, territory, and enemy:";

        var combined = Combine(FactionLoreSystem, user, FactionOutputSpec);
        return combined;
    }

    public static string BuildLorePrompt(string worldName, WorldGenerationOptions options, int entryNumber)
    {
        var user = $@"World: {worldName}
Setting: {options.Description}
Mood: {options.Flavor}
Era: {options.TimePeriod}
Context: {options.MainPlotPoint}

Write lore entry #{entryNumber} - 1-2 sentences about an interesting detail from this world's history or culture:";

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
}
