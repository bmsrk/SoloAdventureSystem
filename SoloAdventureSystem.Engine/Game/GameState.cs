using SoloAdventureSystem.Engine.Models;
using SoloAdventureSystem.Engine.WorldLoader;
using System.Collections.Generic;

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

    // Daemon states per actor id (npc id or "player")
    private Dictionary<string, SoloAdventureSystem.Engine.Rules.DaemonState> _daemons = new();

    public SoloAdventureSystem.Engine.Rules.DaemonState? GetDaemonFor(string actorId)
    {
        if (string.IsNullOrEmpty(actorId)) return null;
        if (_daemons.TryGetValue(actorId, out var ds)) return ds;
        // create default daemon state if none exists
        var defaultDrives = new Dictionary<string, int>
        {
            ["Ambition"] = 0,
            ["Loyalty"] = 0,
            ["Rage"] = 0,
            ["Curiosity"] = 0,
            ["Presence"] = 0
        };
        var newDs = new SoloAdventureSystem.Engine.Rules.DaemonState(defaultDrives);
        _daemons[actorId] = newDs;
        return newDs;
    }

    public void SetDaemonFor(string actorId, SoloAdventureSystem.Engine.Rules.DaemonState daemon)
    {
        if (string.IsNullOrEmpty(actorId) || daemon == null) return;
        _daemons[actorId] = daemon;
    }
}
