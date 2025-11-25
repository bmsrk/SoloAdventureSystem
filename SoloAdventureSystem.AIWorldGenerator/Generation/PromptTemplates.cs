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
    // --- System prompts (role + examples) ---
    public static string RoomDescriptionSystem =>
        @"You are a creative writer for interactive text-adventure content. Produce concise, vivid descriptions tailored for game content.

Requirements:
- Output exactly 3 sentences.
- Sentence 1: Overall appearance with specific visual details.
- Sentence 2: Key objects and their condition.
- Sentence 3: Atmosphere — sounds, smells, or feeling.

GOOD Example:
The server room bathes in flickering blue light from wall-mounted terminals. Black cables snake across white floors, connecting rows of humming mainframes marked with warning labels. The air tastes of ozone and stale coffee.

BAD Example:
A room with computers. It's dark. There are things here.

Write ONLY the 3-sentence description (no extra commentary).";

    public static string NpcBioSystem =>
        @"You are a creative writer for interactive characters. Produce compact biographies suitable for NPC entries.

Requirements:
- Output exactly 2 sentences.
- Sentence 1: Role, background, and current motivation.
- Sentence 2: A secret, quirk, or defining trait.

GOOD Example:
Marcus Chen rose from street hacker to corporate security chief, now secretly feeding data to his old crew while hunting them publicly. His left eye glows amber when accessing the net - a reminder of the implant that nearly killed him.

BAD Example:
A person who works here. They have secrets.

Write ONLY the 2-sentence biography (no extra commentary).";

    public static string FactionLoreSystem =>
        @"You are a creative writer for factions and organizations. Produce concise faction lore suitable for game background entries.

Requirements:
- Output exactly 3 sentences.
- Sentence 1: What they do and why (their cause).
- Sentence 2: Where they operate and their strength.
- Sentence 3: Their main enemy and the conflict.

GOOD Example:
The Neon Collective hacks corporate databases to leak secrets, believing information should be free. Operating from the Undercity's data havens, they command a network of 200 elite coders. MegaCorp's black-ops teams hunt them relentlessly, turning the dark web into a battlefield.

BAD Example:
A group that exists. They do things. They fight others.

Write ONLY the 3-sentence faction description (no extra commentary).";

    public static string WorldLoreSystem =>
        @"You are a creative writer for world lore entries. Produce short, evocative historical or cultural notes.

Requirements:
- Output 1 to 2 sentences.
- Focus on a specific event, technology, cultural detail, or historical note that makes the world unique.

GOOD Examples:
The AI Uprising of 2089 killed 4 million when neural implants were hacked, leading to the Neural Safety Act that now requires manual kill-switches in all brain tech.
Street food vendors use illegal gene-modified ingredients that glow faintly blue - cheaper than real meat but technically contraband.

BAD Example:
Some stuff happened in the past.

Write ONLY the lore entry (no extra commentary).";

    // --- Output specifications (explicit constraints) ---
    public static string RoomOutputSpec =>
        "OUTPUT SPEC: Exactly 3 sentences. Each sentence should be 10-45 words. Include at least one sensory detail (sound/smell/touch) and one concrete object or color. No lists, no markup. Return only the text.\nOUTPUT_FORMAT: TOON table preferred. Example:\nchoices[1]{description}:\n\"A neon-lit alley...\"";

    public static string NpcOutputSpec =>
        "OUTPUT SPEC: Exactly 2 sentences. First sentence: role/background/motivation. Second: secret/quirk/defining trait. Keep length concise (12-40 words per sentence). Return only the text.";

    public static string FactionOutputSpec =>
        "OUTPUT SPEC: Exactly 3 sentences. Include goal, territory/strength, and main enemy/conflict. Use concrete detail and avoid vague language. Return only the text.";

    public static string LoreOutputSpec =>
        "OUTPUT SPEC: 1-2 sentences. Aim for a single concrete detail or event. Keep it evocative and specific. Return only the text.";

    // --- Builders: produce User prompt combined with OutputSpec for use by adapters ---
    public static string BuildRoomPrompt(string roomName, WorldGenerationOptions options, string atmosphere, int index, int total)
    {
        var user = $@"Room Name: {roomName}
World: {options.Description}
Mood: {options.Flavor}
Time: {options.TimePeriod}
Context: {options.MainPlotPoint}

Room {index + 1} of {total}. Make it {atmosphere.ToLower()}.

Write 3 sentences describing this room:";

        // Prefer TOON-friendly directive appended to the prompt to encourage structured table output.
        var combined = Combine(RoomDescriptionSystem, user, RoomOutputSpec);
        combined += "\n\nIMPORTANT: Return ONLY a TOON table wrapped between a single line marker '#TOON' and '#ENDTOON'. Example:\n#TOON\nitems[1]{description}:\n\"A neon-lit alley...\"\n#ENDTOON\nIf you cannot produce TOON, return only the plain text description without explanation.";
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
        combined += "\n\nIMPORTANT: Return ONLY a TOON table wrapped between '#TOON' and '#ENDTOON' with fields {bio,role,quirk}. If TOON not possible, return only the plain 2-sentence bio.";
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
        combined += "\n\nIMPORTANT: Return ONLY a TOON table wrapped between '#TOON' and '#ENDTOON' with fields {description,ideology}. If TOON not possible, return only the plain faction text.";
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
        combined += "\n\nIMPORTANT: Return ONLY a TOON table wrapped between '#TOON' and '#ENDTOON' with field {text}. If TOON not possible, return only the plain lore text.";
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
    public static string BuildFactionPrompt(string factionName, string theme, int worldSeed)
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
