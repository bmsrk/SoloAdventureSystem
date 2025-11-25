namespace SoloAdventureSystem.ContentGenerator.Generation;

/// <summary>
/// Constants for generation token limits and parameters.
/// Replaces magic numbers scattered throughout the codebase.
/// </summary>
public static class GenerationLimits
{
    /// <summary>
    /// Token limit for room descriptions (needs 3 sentences + sensory details)
    /// </summary>
    public const int RoomDescriptionTokens = 200;

    /// <summary>
    /// Token limit for NPC biographies (needs 2 sentences + personality traits)
    /// </summary>
    public const int NpcBioTokens = 180;

    /// <summary>
    /// Token limit for faction lore (needs 3 sentences + conflict dynamics)
    /// </summary>
    public const int FactionLoreTokens = 200;

    /// <summary>
    /// Token limit for individual lore entries (needs 1-2 sentences)
    /// </summary>
    public const int LoreEntryTokens = 150;

    /// <summary>
    /// Token limit for dialogue generation (choices JSON)
    /// </summary>
    public const int DialogueTokens = 180;

    /// <summary>
    /// Minimum number of rooms required for a valid world
    /// </summary>
    public const int MinimumRooms = 3;

    /// <summary>
    /// Minimum number of factions required for a valid world
    /// </summary>
    public const int MinimumFactions = 1;

    /// <summary>
    /// Minimum number of story nodes required for a valid world
    /// </summary>
    public const int MinimumStoryNodes = 1;

    /// <summary>
    /// Maximum consecutive empty outputs before failing
    /// </summary>
    public const int MaxConsecutiveEmptyOutputs = 3;
}
