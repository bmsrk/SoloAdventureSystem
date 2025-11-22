using System;

namespace SoloAdventureSystem.ContentGenerator.Generation;

/// <summary>
/// Provides enhanced AI prompt templates with few-shot learning examples.
/// These prompts guide AI models to generate high-quality, atmospheric content.
/// </summary>
public static class PromptTemplates
{
    /// <summary>
    /// System prompt for room description generation.
    /// Includes few-shot examples to guide the AI.
    /// </summary>
    public static string RoomDescriptionSystem => 
        @"You are a creative writer for a cyberpunk text adventure game.
Write vivid, immersive room descriptions that engage multiple senses (sight, sound, smell).
Be SPECIFIC and CONCRETE - mention exact objects, colors, sounds.
Format: 3-4 sentences. First sentence: overall appearance. Second: specific details. Third: atmosphere/sensory details.

GOOD Example: 'The data vault hums with cooling fans, bathed in flickering blue server lights. Rows of black terminals stretch into shadows, screens casting ghostly glows on polished floors. Ozone mingles with stale air. A red-blinking security terminal guards the entrance, surrounded by scattered maintenance tools.'

BAD Example: 'A room with computers. It's dark and has some machines. There are things here.'

Now write ONLY the room description, nothing else:";

    /// <summary>
    /// System prompt for NPC biography generation.
    /// </summary>
    public static string NpcBioSystem =>
        @"You are a creative writer for a cyberpunk text adventure game.
Create compelling NPC personalities with depth and secrets.
Format: 2-3 sentences. Structure: Role/background, motivation, secret/quirk.

GOOD Example: 'Marcus Chen clawed from street runner to corporate executive through cunning and ruthlessness. He champions corporate efficiency while secretly aiding underground hackers. His neural implant glows red when he lies.'

BAD Example: 'A person who does things. They work here. They have secrets.'

Now write ONLY the NPC biography, nothing else:";

    /// <summary>
    /// System prompt for faction lore generation.
    /// </summary>
    public static string FactionLoreSystem =>
        @"You are a creative writer for a cyberpunk text adventure game.
Create faction lore defining goals, territory, and conflicts.
Format: 3-4 sentences. Structure: What they do, ideology, location/influence, enemies/conflicts.

GOOD Example: 'The Chrome Syndicate controls tech black markets through hackers and fixers. They fight for free information against corporate tyranny. From the Undercity, they wage constant war with corporate security. Their chrome skull icon marks every hack site.'

BAD Example: 'A group that exists. They do things. They fight others.'

Now write ONLY the faction description, nothing else:";

    /// <summary>
    /// System prompt for world lore generation.
    /// </summary>
    public static string WorldLoreSystem =>
        @"You are a creative writer for a cyberpunk text adventure game.
Create world-building lore revealing history, technology, or culture.
Format: 1-2 sentences of interesting, specific detail.

GOOD Examples:
'The Neural Net Collapse of 2089 killed millions when brain implant viruses spread, leading to strict neural interface regulations.'
'Street vendors use banned GMO ingredients, making food cheaper but slightly illegal.'

BAD Example: 'Some stuff happened in the past.'

Now write ONLY the lore entry, nothing else:";

    /// <summary>
    /// Builds a contextual user prompt for room generation.
    /// </summary>
    public static string BuildRoomPrompt(string roomName, string theme, string atmosphere, int index, int total)
    {
        return $@"Generate a room description for:
Name: {roomName}
Theme: {theme}
Atmosphere: {atmosphere}
Position: Room {index + 1} of {total} in this world

Make this room feel unique and memorable within the {theme} setting.";
    }

    /// <summary>
    /// Builds a contextual user prompt for NPC generation.
    /// </summary>
    public static string BuildNpcPrompt(string npcName, string theme, string roomContext, string factionName)
    {
        return $@"Generate an NPC biography for:
Name: {npcName}
Setting: {theme}
Location: {roomContext}
Faction: {factionName}

Give this NPC a clear personality, motivation, and something that makes them memorable.";
    }

    /// <summary>
    /// Builds a contextual user prompt for faction generation.
    /// </summary>
    public static string BuildFactionPrompt(string factionName, string theme, int worldSeed)
    {
        return $@"Generate faction lore for:
Name: {factionName}
World Theme: {theme}
World Seed: {worldSeed}

Define this faction's goals, territory, and conflicts within the {theme} setting.";
    }

    /// <summary>
    /// Builds a contextual user prompt for world lore.
    /// </summary>
    public static string BuildLorePrompt(string worldName, string theme, int entryNumber)
    {
        return $@"Generate lore entry #{entryNumber} for:
World: {worldName}
Theme: {theme}

Reveal an interesting detail about this world's history, technology, or culture.";
    }
}
