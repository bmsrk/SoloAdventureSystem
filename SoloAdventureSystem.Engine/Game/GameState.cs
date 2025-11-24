using SoloAdventureSystem.Engine.Models;
using SoloAdventureSystem.Engine.WorldLoader;

namespace SoloAdventureSystem.Engine.Game;

/// <summary>
/// Represents the current state of the game session
/// </summary>
public class GameState
{
    public WorldModel World { get; set; } = null!;
    public Location CurrentLocation { get; set; } = null!;
    public PlayerCharacter Player { get; set; } = new();
    public List<string> Inventory { get; set; } = new();
    public Dictionary<string, bool> Flags { get; set; } = new();
    public int TurnCount { get; set; }
    public bool IsInCombat { get; set; }
    public string? CurrentCombatant { get; set; }
}
