namespace MudVision.World.Models;

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
    public string Id { get; init; }
    public string Title { get; init; }
    public string Text { get; init; }
    public List<StoryChoice> Choices { get; init; }
}

public record StoryChoice
{
    public string Label { get; init; }
    public string Next { get; init; }
    public List<string> Effects { get; init; }
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
