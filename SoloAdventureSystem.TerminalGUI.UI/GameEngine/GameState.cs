using MudVision.WorldLoader;
using MudVision.World.Models;
using System.Linq;

namespace SoloAdventureSystem.TerminalGUI.GameEngine;

/// <summary>
/// Manages the current state of the game
/// </summary>
public class GameState
{
    public WorldModel World { get; set; } = null!;
    public Location CurrentLocation { get; set; } = null!;
    public Player Player { get; set; } = new();
    public List<string> GameLog { get; set; } = new();
    public Dictionary<string, int> Inventory { get; set; } = new();
    public Dictionary<string, bool> Flags { get; set; } = new();

    public void AddLog(string message)
    {
        GameLog.Add($"[{DateTime.Now:HH:mm:ss}] {message}");
        if (GameLog.Count > 100) // Keep last 100 messages
        {
            GameLog.RemoveAt(0);
        }
    }

    public Location? GetLocationById(string locationId)
    {
        return World?.Rooms?.FirstOrDefault(r => r.Id == locationId);
    }

    public NPC? GetNpcById(string npcId)
    {
        return World?.Npcs?.FirstOrDefault(n => n.Id == npcId);
    }

    public Faction? GetFactionById(string factionId)
    {
        return World?.Factions?.FirstOrDefault(f => f.Id == factionId);
    }
}

public class Player
{
    public string Name { get; set; } = "Adventurer";
    public int Health { get; set; } = 100;
    public int MaxHealth { get; set; } = 100;
    public int Level { get; set; } = 1;
    public int Experience { get; set; } = 0;
    public Attributes Attributes { get; set; } = new(10, 10, 10, 10, 10, 10);
}
