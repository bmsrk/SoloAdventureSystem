using System;
using System.Collections.Generic;
using SoloAdventureSystem.Engine.Rules;

namespace SoloAdventureSystem.Engine.Models;

public record class WorldDefinition(
    string Name,
    string Description,
    string Version,
    string Author,
    DateTime CreatedAt,
    string StartLocationId,
    List<string> LocationIds,
    List<string> NpcIds,
    List<string> ItemIds,
    List<string> FactionIds,
    List<string> StoryNodeIds
);

public record class Location(
    string Id,
    string Name,
    string Description,
    Dictionary<string, string> Connections,
    List<string> NpcIds,
    List<string> ItemIds
);

public enum Hostility
{
    Passive,
    Neutral,
    Hostile
}

public enum Behavior
{
    Static,
    Patrol,
    Aggressive,
    Friendly
}

public record class NPC(
    string Id,
    string Name,
    string Description,
    string FactionId,
    Hostility Hostility,
    Attributes Attributes,
    Behavior Behavior,
    List<string> Inventory
);

public record class Attributes(
    int Strength,
    int Dexterity,
    int Intelligence,
    int Constitution,
    int Wisdom,
    int Charisma
);

public enum ItemType
{
    Weapon,
    Consumable,
    Key,
    Misc
}

public record class Item(
    string Id,
    string Name,
    string Description,
    ItemType Type,
    double Weight,
    double Volume,
    int? Damage,
    int? Bonus
);

public record class Faction(
    string Id,
    string Name,
    string Description,
    string Ideology,
    Dictionary<string, int> Relations
);

public record StoryNode
{
    public string Id { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Text { get; init; } = string.Empty;
    public string OwnerNpcId { get; init; } = string.Empty;
    public List<StoryChoice> Choices { get; init; } = new();
}

public record StoryChoice
{
    public string Label { get; init; } = string.Empty;
    public string Next { get; init; } = string.Empty;
    public List<string> Effects { get; init; } = new();
    public SkillCheck? SkillCheck { get; init; }
}

public record SkillCheck
{
    public GameAttribute Attribute { get; init; } = GameAttribute.Soul;
    public Skill Skill { get; init; } = Skill.Social;
    public int TargetNumber { get; init; } = 10;
    public string OpponentNpcId { get; init; } = string.Empty;
}

public record class Alignment(
    int OrderChaos,
    int EmpathyColdness,
    int SpiritMaterial
);

public record class WorldPackage(
    WorldDefinition WorldDefinition,
    List<Location> Locations,
    List<NPC> Npcs,
    List<Item> Items,
    List<Faction> Factions,
    List<StoryNode> StoryNodes
);
