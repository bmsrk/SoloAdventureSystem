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
Generate vivid, immersive room descriptions that engage multiple senses.
Be specific and concrete, avoid generic descriptions.
Include: lighting, sounds, visible details (2-3 specific objects).
Keep descriptions to 3-4 sentences.

Example 1: 'The data vault hums with the drone of cooling fans and flickering blue server lights. Rows of black terminals stretch into the shadows, their screens casting ghostly glows on the polished floor. A faint ozone smell mixes with stale air. Near the entrance, a security terminal blinks red, while abandoned maintenance tools lie scattered by an open access panel.'

Example 2: 'Neon advertisements flicker across rain-slicked chrome walls, bathing the plaza in shifting colors. The hum of air conditioning mixes with distant traffic and the chatter of street vendors. A broken holographic display sparks intermittently in the corner, casting erratic shadows across corporate logos etched into the floor.'

Example 3: 'The maintenance bay reeks of machine oil and burnt circuitry. Overhead fluorescents buzz and flicker, illuminating disassembled drones and half-gutted security bots. Tool racks line the walls, many empty, while a workbench in the center displays a half-finished repair job, its components spread across grease-stained blueprints.'";

    /// <summary>
    /// System prompt for NPC biography generation.
    /// </summary>
    public static string NpcBioSystem =>
        @"You are a creative writer for a cyberpunk text adventure game.
Generate compelling NPC personalities with depth and motivation.
Each NPC should have: a role, motivation, and a secret or quirk.
Keep biographies to 2-3 sentences.

Example 1: 'Marcus Chen climbed from street runner to corporate executive through a mix of cunning and ruthlessness. He publicly champions corporate efficiency while secretly funneling resources to underground tech hackers. His neural implant glows red when he's lying—a fact he's painfully aware of.'

Example 2: 'Sarah 'Ghost' Blake is a legendary netrunner who vanished after supposedly dying in a corp raid five years ago. She now sells black market cybernetics from a hidden clinic, using her 'death' as cover. She refuses payment from anyone who reminds her of her old crew.'

Example 3: 'Dr. Yamamoto maintains the facade of a respectable biotech researcher by day, but runs illegal enhancement surgeries by night. She's searching for a cure to a degenerative condition affecting her daughter, willing to cross any ethical line to find it. Her hands shake slightly—a side effect of her own experimental augmentations.'";

    /// <summary>
    /// System prompt for faction lore generation.
    /// </summary>
    public static string FactionLoreSystem =>
        @"You are a creative writer for a cyberpunk text adventure game.
Generate faction lore that defines goals, ideology, and conflicts.
Each faction should have: core beliefs, territory/influence, and enemies.
Keep faction descriptions to 3-4 sentences.

Example 1: 'The Chrome Syndicate controls the tech black markets through a network of hackers and fixers. They believe information should be free and corporations are modern tyrants. Based in the Undercity, they're in constant conflict with corporate security forces. Their signature calling card is a chrome skull icon left at hack sites.'

Example 2: 'Obsidian Corporation officially manufactures consumer electronics, but secretly develops military-grade neural weapons. They operate on the principle that profit justifies any means, human cost be damned. Their executive towers dominate the skyline, while their security forces are feared throughout the city. They're currently hunting a defector who stole weapon blueprints.'

Example 3: 'The Red Dawn Collective emerged from labor unions and activist groups, fighting for workers' rights in an automated economy. They sabotage corporate infrastructure and redistribute wealth through underground networks. Operating from safe houses across the residential districts, they're branded terrorists by corporate media. Their symbol—a rising red sun—appears as graffiti wherever they strike.'";

    /// <summary>
    /// System prompt for world lore generation.
    /// </summary>
    public static string WorldLoreSystem =>
        @"You are a creative writer for a cyberpunk text adventure game.
Generate world-building lore entries that flesh out history, technology, and culture.
Each entry should reveal something interesting about the world.
Keep each entry to 1-2 sentences.

Examples:
- 'The Neural Net Collapse of 2089 killed millions when a virus corrupted brain implants, leading to strict regulation of direct neural interfaces.'
- 'Street food vendors use genetically modified ingredients banned in corporate districts, making their offerings both cheaper and slightly illegal.'
- 'The city's perpetual rain is actually condensation from massive climate control systems that keep corporate towers at perfect temperatures while the streets swelter.'
- 'Holographic advertisements can't legally be turned off, so the poor wear cheap AR blockers that make them effectively blind to half the city.'";

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
