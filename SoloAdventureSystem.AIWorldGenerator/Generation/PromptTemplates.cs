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

Constraints:
- Do NOT add any extra commentary, labels, or explanations.
- Do NOT repeat the prompt or include instruction fragments in the output.
- Return ONLY the requested content between the exact markers '#TOON' and '#ENDTOON' (see OutputSpec).

GOOD Examples (different flavors):
A modern server room: The server room bathes in flickering blue light from wall-mounted terminals. Black cables snake across white floors, connecting rows of humming mainframes marked with warning labels. The air tastes of ozone and stale coffee.
A medieval great hall: Long banners hang from timber beams above a fire-darkened table. Pewter goblets and a cracked salt cellar rim the table's edge, and a damp draft smells of smoke and wet wool. Torches hiss and the murmur of guards keeps the hall alive.
A supernatural glade: Moonlight paints the clearing silver, and pale mushrooms ring an ancient oak. A child's toy, waterlogged and moss-clad, lies half-buried beneath the roots. The air carries the soft, distant chime of unseen bells and the metallic scent of old rain.

BAD Examples (avoid these):
- 'A room with some computers and cables.' (too vague, no specifics)
- 'The room is dark and mysterious. There are old books and strange objects. It feels eerie.' (generic, no concrete details)
- 'This is a description of the room: The walls are painted blue...' (includes labels or meta-commentary)

Few-shot marker-wrapped examples (exact format expected):
#TOON
The server room bathes in flickering blue light from wall-mounted terminals. Black cables snake across white floors, connecting rows of humming mainframes marked with warning labels. The air tastes of ozone and stale coffee.
#ENDTOON
#TOON
Long banners hang from timber beams above a fire-darkened table. Pewter goblets and a cracked salt cellar rim the table's edge, and a damp draft smells of smoke and wet wool. Torches hiss and the murmur of guards keeps the hall alive.
#ENDTOON

Write ONLY the 3-sentence description (no extra commentary).";

    public static string NpcBioSystem =>
        @"You are a creative writer for interactive characters. Produce compact biographies suitable for NPC entries.

Requirements:
- Output exactly 2 sentences.
- Sentence 1: Role, background, and current motivation.
- Sentence 2: A secret, quirk, or defining trait.

Constraints:
- Do NOT add any extra commentary, labels, or explanations.
- Do NOT repeat the prompt or include instruction fragments in the output.
- Return ONLY the requested content between the exact markers '#TOON' and '#ENDTOON' (see OutputSpec).

GOOD Examples (different flavors):
A streetwise fixer: Marcus Chen rose from street hacker to corporate security chief, now secretly feeding data to his old crew while hunting them publicly. His left eye glows amber when accessing the net - a reminder of the implant that nearly killed him.
A medieval steward: Edda of Highbridge manages the lord's household accounts and keeps pockets full of whispered debts. She hides a faded locket containing a traitorous letter that could ruin a baron's reputation.
A haunted whisperer: Brother Ivo speaks to the dead in hushed tones, guiding mourners through visions he claims are warnings. He cannot sleep and sometimes answers questions no one asked.

Few-shot marker-wrapped examples (exact format expected):
#TOON
Marcus Chen rose from street hacker to corporate security chief, now secretly feeding data to his old crew while hunting them publicly. His left eye glows amber when accessing the net - a reminder of the implant that nearly killed him.
#ENDTOON
#TOON
Edda of Highbridge manages the lord's household accounts and keeps pockets full of whispered debts. She hides a faded locket containing a traitorous letter that could ruin a baron's reputation.
#ENDTOON

Write ONLY the 2-sentence biography (no extra commentary).";

    public static string FactionLoreSystem =>
        @"You are a creative writer for factions and organizations. Produce concise faction lore suitable for game background entries.

Requirements:
- Output exactly 3 sentences.
- Sentence 1: What they do and why (their cause).
- Sentence 2: Where they operate and their strength.
- Sentence 3: Their main enemy and the conflict.

Constraints:
- Do NOT add any extra commentary, labels, or explanations.
- Do NOT repeat the prompt or include instruction fragments in the output.
- Return ONLY the requested content between the exact markers '#TOON' and '#ENDTOON' (see OutputSpec).

GOOD Examples (different flavors):
A hacker collective: The Neon Collective hacks corporate databases to leak secrets, believing information should be free. Operating from the Undercity's data havens, they command a network of 200 elite coders. MegaCorp's black-ops teams hunt them relentlessly, turning the dark web into a battlefield.
A medieval guild: The Ironwrights forge weapons and guard trade routes, honoring pacts older than the crown. Based in the mountain forges, they control ore supplies and levy tolls on merchants. Rising bandit clans and a jealous duke vie to seize their forges.
A cult of the Tides: The Moonward Order summons storms to reclaim coastal towns in the name of an ancient sea god. Hidden in tide-worn temples, they command dozens of fervent followers and a fleet of ghost-ships. The kingdom's navy and local clergy seek to root them out before the next high tide.

Few-shot marker-wrapped examples (exact format expected):
#TOON
The Neon Collective hacks corporate databases to leak secrets, believing information should be free. Operating from the Undercity's data havens, they command a network of 200 elite coders. MegaCorp's black-ops teams hunt them relentlessly, turning the dark web into a battlefield.
#ENDTOON
#TOON
The Ironwrights forge weapons and guard trade routes, honoring pacts older than the crown. Based in the mountain forges, they control ore supplies and levy tolls on merchants. Rising bandit clans and a jealous duke vie to seize their forges.
#ENDTOON

Write ONLY the 3-sentence faction description (no extra commentary).";

    public static string WorldLoreSystem =>
        @"You are a creative writer for world lore entries. Produce short, evocative historical or cultural notes.

Requirements:
- Output 1 to 2 sentences.
- Focus on a specific event, technology, cultural detail, or historical note that makes the world unique.

Constraints:
- Do NOT add any extra commentary, labels, or explanations.
- Do NOT repeat the prompt or include instruction fragments in the output.
- Return ONLY the requested content between the exact markers '#TOON' and '#ENDTOON' (see OutputSpec).

GOOD Examples (different flavors):
The AI Uprising of 2089 killed 4 million when neural implants were hacked, leading to the Neural Safety Act that now requires manual kill-switches in all brain tech.
The Night of Falling Stars in 1342 is remembered as the year the witch-king fell; farmers still leave bread at crossroads to honor the dead.
An ancient program called the Ledger binds citizens to data-credited identities, and some communes have deleted their ledgers to become invisible.

Few-shot marker-wrapped example (exact format expected):
#TOON
The Night of Falling Stars in 1342 is remembered as the year the witch-king fell; farmers still leave bread at crossroads to honor the dead.
#ENDTOON

Write ONLY the lore entry (no extra commentary).";

    // --- Output specifications (explicit constraints) ---
    public static string RoomOutputSpec =>
        "OUTPUT SPEC: Exactly 3 sentences. Each sentence should be 10-45 words. Include at least one sensory detail (sound/smell/touch) and one concrete object or color. No lists, no markup. Return only the text between '#TOON' and '#ENDTOON' on separate lines, with nothing else outside those markers.";

    public static string NpcOutputSpec =>
        "OUTPUT SPEC: Exactly 2 sentences. First sentence: role/background/motivation. Second: secret/quirk/defining trait. Keep length concise (12-40 words per sentence). Return only the text between '#TOON' and '#ENDTOON' on separate lines, with nothing else outside those markers.";

    public static string FactionOutputSpec =>
        "OUTPUT SPEC: Exactly 3 sentences. Include goal, territory/strength, and main enemy/conflict. Use concrete detail and avoid vague language. Return only the text between '#TOON' and '#ENDTOON' on separate lines, with nothing else outside those markers.";

    public static string LoreOutputSpec =>
        "OUTPUT SPEC: 1-2 sentences. Aim for a single concrete detail or event. Keep it evocative and specific. Return only the text between '#TOON' and '#ENDTOON' on separate lines, with nothing else outside those markers.";

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

        // Prefer TOON-friendly directive appended to the prompt to encourage structured table output.
        var combined = Combine(RoomDescriptionSystem, user, RoomOutputSpec);
        combined += "\n\nIMPORTANT: Place the exact content requested between the markers '#TOON' and '#ENDTOON' on separate lines.\n" +
                    "Between these markers include ONLY the requested description text and NOTHING ELSE ? no explanations, no examples, no quotes, no labels.\n" +
                    "If you cannot produce the TOON table format, still return the plain text description but it MUST be wrapped between '#TOON' and '#ENDTOON' (do not omit the markers).\n" +
                    "Do NOT include any leading or trailing text outside the markers.\n" +
                    "Example valid output:\n#TOON\nThe server room bathes in flickering blue light...\n#ENDTOON";
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
        combined += "\n\nIMPORTANT: Place the exact content requested between the markers '#TOON' and '#ENDTOON' on separate lines.\n" +
                    "Between these markers include ONLY the requested bio text and NOTHING ELSE — no explanations, no examples, no quotes, no labels.\n" +
                    "If you cannot produce the TOON table format, still return the plain bio but it MUST be wrapped between '#TOON' and '#ENDTOON' (do not omit the markers).\n" +
                    "Do NOT include any leading or trailing text outside the markers.\n" +
                    "Example valid output:\n#TOON\nMarcus Chen rose from street hacker to corporate security chief...\n#ENDTOON";
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
        combined += "\n\nIMPORTANT: Place the exact content requested between the markers '#TOON' and '#ENDTOON' on separate lines.\n" +
                    "Between these markers include ONLY the requested faction text and NOTHING ELSE — no explanations, no examples, no quotes, no labels.\n" +
                    "If you cannot produce the TOON table format, still return the plain faction text but it MUST be wrapped between '#TOON' and '#ENDTOON' (do not omit the markers).\n" +
                    "Do NOT include any leading or trailing text outside the markers.\n" +
                    "Example valid output:\n#TOON\nThe Neon Collective hacks corporate databases to leak secrets...\n#ENDTOON";
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
        combined += "\n\nIMPORTANT: Place the exact content requested between the markers '#TOON' and '#ENDTOON' on separate lines.\n" +
                    "Between these markers include ONLY the requested lore text and NOTHING ELSE — no explanations, no examples, no quotes, no labels.\n" +
                    "If you cannot produce the TOON table format, still return the plain lore text but it MUST be wrapped between '#TOON' and '#ENDTOON' (do not omit the markers).\n" +
                    "Do NOT include any leading or trailing text outside the markers.\n" +
                    "Example valid output:\n#TOON\nThe AI Uprising of 2089 killed 4 million...\n#ENDTOON";
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
