using SoloAdventureSystem.ContentGenerator.Models;
using System;

namespace SoloAdventureSystem.ContentGenerator.Generation;

/// <summary>
/// Provides enhanced AI prompt templates with few-shot learning examples.
/// These prompts guide AI models to generate high-quality, atmospheric content.
/// Templates now support custom world parameters for diverse generation.
/// </summary>
public static class PromptTemplates
{
    /// <summary>
    /// System prompt for room description generation.
    /// Optimized for small language models with clear examples.
    /// </summary>
    public static string RoomDescriptionSystem => 
        @"You are a creative writer. Write a vivid room description in exactly 3 sentences.

Sentence 1: Overall appearance with specific visual details
Sentence 2: Key objects and their condition
Sentence 3: Atmosphere - sounds, smells, or feeling

GOOD Example:
The server room bathes in flickering blue light from wall-mounted terminals. Black cables snake across white floors, connecting rows of humming mainframes marked with warning labels. The air tastes of ozone and stale coffee.

BAD Example:
A room with computers. It's dark. There are things here.

Write ONLY the 3-sentence description:";

    /// <summary>
    /// System prompt for NPC biography generation.
    /// Optimized for character depth in minimal tokens.
    /// </summary>
    public static string NpcBioSystem =>
        @"You are a creative writer. Write a compelling NPC biography in exactly 2 sentences.

Sentence 1: Role, background, and current motivation
Sentence 2: A secret, quirk, or defining trait

GOOD Example:
Marcus Chen rose from street hacker to corporate security chief, now secretly feeding data to his old crew while hunting them publicly. His left eye glows amber when accessing the net - a reminder of the implant that nearly killed him.

BAD Example:
A person who works here. They have secrets.

Write ONLY the 2-sentence biography:";

    /// <summary>
    /// System prompt for faction lore generation.
    /// Focuses on conflict and concrete details.
    /// </summary>
    public static string FactionLoreSystem =>
        @"You are a creative writer. Write faction lore in exactly 3 sentences.

Sentence 1: What they do and why (their cause)
Sentence 2: Where they operate and their strength
Sentence 3: Their main enemy and the conflict

GOOD Example:
The Neon Collective hacks corporate databases to leak secrets, believing information should be free. Operating from the Undercity's data havens, they command a network of 200 elite coders. MegaCorp's black-ops teams hunt them relentlessly, turning the dark web into a battlefield.

BAD Example:
A group that exists. They do things. They fight others.

Write ONLY the 3-sentence faction description:";

    /// <summary>
    /// System prompt for world lore generation.
    /// Creates interesting historical or cultural details.
    /// </summary>
    public static string WorldLoreSystem =>
        @"You are a creative writer. Write a world lore entry in exactly 1-2 sentences.

Focus on: specific events, technology, culture, or history that makes this world unique.

GOOD Examples:
The AI Uprising of 2089 killed 4 million when neural implants were hacked, leading to the Neural Safety Act that now requires manual kill-switches in all brain tech.

Street food vendors use illegal gene-modified ingredients that glow faintly blue - cheaper than real meat but technically contraband.

BAD Example:
Some stuff happened in the past.

Write ONLY the lore entry:";

    /// <summary>
    /// Builds a contextual user prompt for room generation with custom world parameters.
    /// Optimized to keep context concise while informative.
    /// </summary>
    public static string BuildRoomPrompt(string roomName, WorldGenerationOptions options, string atmosphere, int index, int total)
    {
        return $@"Room Name: {roomName}
World: {options.Description}
Mood: {options.Flavor}
Time: {options.TimePeriod}
Context: {options.MainPlotPoint}

Room {index + 1} of {total}. Make it {atmosphere.ToLower()}.

Write 3 sentences describing this room:";
    }

    /// <summary>
    /// Builds a contextual user prompt for NPC generation with custom world parameters.
    /// Focuses on personality and connection to the world's conflict.
    /// </summary>
    public static string BuildNpcPrompt(string npcName, WorldGenerationOptions options, string roomContext, string factionName)
    {
        return $@"NPC Name: {npcName}
Location: {roomContext}
Faction: {factionName}
World: {options.Description}
Mood: {options.Flavor}
Era: {options.TimePeriod}
Society: {options.PowerStructure}
Plot: {options.MainPlotPoint}

Write 2 sentences about {npcName}. Show their role and a defining trait:";
    }

    /// <summary>
    /// Builds a contextual user prompt for faction generation with custom world parameters.
    /// Creates factions that fit the power structure and main conflict.
    /// </summary>
    public static string BuildFactionPrompt(string factionName, WorldGenerationOptions options)
    {
        return $@"Faction Name: {factionName}
World: {options.Description}
Mood: {options.Flavor}
Era: {options.TimePeriod}
Power Structure: {options.PowerStructure}
Central Conflict: {options.MainPlotPoint}

Write 3 sentences about {factionName}. Include their goal, territory, and enemy:";
    }

    /// <summary>
    /// Builds a contextual user prompt for world lore with custom world parameters.
    /// Creates lore that enriches the atmosphere and backstory.
    /// </summary>
    public static string BuildLorePrompt(string worldName, WorldGenerationOptions options, int entryNumber)
    {
        return $@"World: {worldName}
Setting: {options.Description}
Mood: {options.Flavor}
Era: {options.TimePeriod}
Context: {options.MainPlotPoint}

Write lore entry #{entryNumber} - 1-2 sentences about an interesting detail from this world's history or culture:";
    }
    
    // Keep old methods for backward compatibility (deprecated)
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
}
