using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text.Json;
using System.Threading.Tasks;
using MudVision.World.Models;
using YamlDotNet.Serialization;
using System.Linq;

namespace MudVision.WorldLoader
{
    public class WorldLoaderService : IWorldLoader
    {
        private const int MaxZipSizeBytes = 20 * 1024 * 1024;

        public async Task<WorldModel> LoadFromZipAsync(Stream zipStream)
        {
            if (zipStream.Length > MaxZipSizeBytes)
                throw new InvalidDataException("Zip file exceeds 20MB limit.");

            using var archive = new ZipArchive(zipStream, ZipArchiveMode.Read, leaveOpen: true);
            var files = new Dictionary<string, ZipArchiveEntry>();
            foreach (var entry in archive.Entries)
            {
                var sanitized = SanitizePath(entry.FullName);
                files[sanitized] = entry;
            }

            if (!files.ContainsKey("world.json"))
                throw new InvalidDataException("Missing required world.json file.");

            var roomFiles = files.Keys.Where(f => f.StartsWith("rooms/") && f.EndsWith(".json")).ToList();
            var factionFiles = files.Keys.Where(f => f.StartsWith("factions/") && f.EndsWith(".json")).ToList();
            if (!roomFiles.Any()) throw new InvalidDataException("At least one room is required.");
            if (!factionFiles.Any()) throw new InvalidDataException("At least one faction is required.");

            var worldModel = new WorldModel();
            using var worldStream = files["world.json"].Open();
            using var worldDoc = await JsonDocument.ParseAsync(worldStream);
            worldModel.WorldDefinition = ParseWorld(worldDoc);

            worldModel.Rooms = new List<Location>();
            foreach (var roomFile in roomFiles)
            {
                using var s = files[roomFile].Open();
                using var doc = await JsonDocument.ParseAsync(s);
                worldModel.Rooms.Add(ParseRoom(doc));
            }

            worldModel.Factions = new List<Faction>();
            foreach (var factionFile in factionFiles)
            {
                using var s = files[factionFile].Open();
                using var doc = await JsonDocument.ParseAsync(s);
                worldModel.Factions.Add(ParseFaction(doc));
            }

            var npcFiles = files.Keys.Where(f => f.StartsWith("npcs/") && f.EndsWith(".json")).ToList();
            worldModel.Npcs = new List<NPC>();
            foreach (var npcFile in npcFiles)
            {
                using var s = files[npcFile].Open();
                using var doc = await JsonDocument.ParseAsync(s);
                worldModel.Npcs.Add(ParseNpc(doc));
            }

            var eventFiles = files.Keys.Where(f => f.StartsWith("events/") && f.EndsWith(".json")).ToList();
            worldModel.Events = new List<EventModel>();
            foreach (var eventFile in eventFiles)
            {
                using var s = files[eventFile].Open();
                using var doc = await JsonDocument.ParseAsync(s);
                worldModel.Events.Add(ParseEvent(doc));
            }

            var storyFiles = files.Keys.Where(f => f.StartsWith("story/") && f.EndsWith(".yaml")).ToList();
            worldModel.StoryNodes = new List<StoryNode>();
            foreach (var storyFile in storyFiles)
            {
                using var s = files[storyFile].Open();
                using var reader = new StreamReader(s);
                var yaml = await reader.ReadToEndAsync();
                try
                {
                    worldModel.StoryNodes.Add(ParseStoryNode(yaml));
                }
                catch (Exception ex)
                {
                    // Handle YAML error gracefully
                    throw new InvalidDataException($"Invalid YAML in {storyFile}: {ex.Message}");
                }
            }

            return worldModel;
        }

        public WorldDefinition ParseWorld(JsonDocument doc)
        {
            var root = doc.RootElement;
            return new WorldDefinition(
                root.GetProperty("Name").GetString() ?? string.Empty,
                root.GetProperty("Description").GetString() ?? string.Empty,
                root.GetProperty("Version").GetString() ?? string.Empty,
                root.GetProperty("Author").GetString() ?? string.Empty,
                root.GetProperty("CreatedAt").GetDateTime(),
                root.GetProperty("StartLocationId").GetString() ?? string.Empty,
                root.GetProperty("LocationIds").EnumerateArray().Select(x => x.GetString() ?? string.Empty).ToList(),
                root.GetProperty("NpcIds").EnumerateArray().Select(x => x.GetString() ?? string.Empty).ToList(),
                root.GetProperty("ItemIds").EnumerateArray().Select(x => x.GetString() ?? string.Empty).ToList(),
                root.GetProperty("FactionIds").EnumerateArray().Select(x => x.GetString() ?? string.Empty).ToList(),
                root.GetProperty("StoryNodeIds").EnumerateArray().Select(x => x.GetString() ?? string.Empty).ToList()
            );
        }

        public Location ParseRoom(JsonDocument doc)
        {
            var root = doc.RootElement;
            return new Location(
                root.GetProperty("Id").GetString() ?? string.Empty,
                root.GetProperty("Title").GetString() ?? string.Empty,
                root.GetProperty("BaseDescription").GetString() ?? string.Empty,
                root.GetProperty("Exits").EnumerateObject().ToDictionary(x => x.Name, x => x.Value.GetString() ?? string.Empty),
                root.GetProperty("Npcs").EnumerateArray().Select(x => x.GetString() ?? string.Empty).ToList(),
                root.GetProperty("Items").EnumerateArray().Select(x => x.GetString() ?? string.Empty).ToList()
            );
        }

        public Faction ParseFaction(JsonDocument doc)
        {
            var root = doc.RootElement;
            return new Faction(
                root.GetProperty("Id").GetString() ?? string.Empty,
                root.GetProperty("Name").GetString() ?? string.Empty,
                root.GetProperty("Description").GetString() ?? string.Empty,
                root.GetProperty("Ideology").GetString() ?? string.Empty,
                root.GetProperty("Relations").EnumerateObject().ToDictionary(x => x.Name, x => x.Value.GetInt32())
            );
        }

        public EventModel ParseEvent(JsonDocument doc)
        {
            var root = doc.RootElement;
            return new EventModel
            {
                Id = root.GetProperty("Id").GetString() ?? string.Empty,
                Name = root.GetProperty("Name").GetString() ?? string.Empty,
                Description = root.GetProperty("Description").GetString() ?? string.Empty
            };
        }

        public NPC ParseNpc(JsonDocument doc)
        {
            var root = doc.RootElement;
            var attributes = root.GetProperty("Attributes");
            return new NPC(
                root.GetProperty("Id").GetString() ?? string.Empty,
                root.GetProperty("Name").GetString() ?? string.Empty,
                root.GetProperty("Description").GetString() ?? string.Empty,
                root.GetProperty("FactionId").GetString() ?? string.Empty,
                Enum.Parse<Hostility>(root.GetProperty("Hostility").GetString() ?? "Neutral"),
                new Attributes(
                    attributes.GetProperty("Strength").GetInt32(),
                    attributes.GetProperty("Dexterity").GetInt32(),
                    attributes.GetProperty("Intelligence").GetInt32(),
                    attributes.GetProperty("Constitution").GetInt32(),
                    attributes.GetProperty("Wisdom").GetInt32(),
                    attributes.GetProperty("Charisma").GetInt32()
                ),
                Enum.Parse<Behavior>(root.GetProperty("Behavior").GetString() ?? "Static"),
                root.GetProperty("Inventory").EnumerateArray().Select(x => x.GetString() ?? string.Empty).ToList()
            );
        }

        public StoryNode ParseStoryNode(string yamlContent)
        {
            var deserializer = new DeserializerBuilder()
                .WithCaseInsensitivePropertyMatching()
                .Build();
            return deserializer.Deserialize<StoryNode>(yamlContent);
        }

        private string SanitizePath(string path)
        {
            return path.Replace("..", string.Empty).Replace("\\", "/").TrimStart('/');
        }
    }

    public class WorldModel
    {
        public MudVision.World.Models.WorldDefinition? WorldDefinition { get; set; }
        public List<MudVision.World.Models.Location>? Rooms { get; set; }
        public List<MudVision.World.Models.Faction>? Factions { get; set; }
        public List<EventModel>? Events { get; set; }
        public List<MudVision.World.Models.NPC>? Npcs { get; set; }
        public List<MudVision.World.Models.StoryNode>? StoryNodes { get; set; }
    }

    public class EventModel
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
