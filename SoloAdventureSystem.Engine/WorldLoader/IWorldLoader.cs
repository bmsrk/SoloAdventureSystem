using System.Text.Json;
using System.Threading.Tasks;
using System.IO;
using MudVision.World.Models;

namespace MudVision.WorldLoader
{
    public interface IWorldLoader
    {
        Task<WorldModel> LoadFromZipAsync(Stream zipStream);
        WorldDefinition ParseWorld(JsonDocument doc);
        Location ParseRoom(JsonDocument doc);
        Faction ParseFaction(JsonDocument doc);
        EventModel ParseEvent(JsonDocument doc);
        NPC ParseNpc(JsonDocument doc);
        StoryNode ParseStoryNode(string yamlContent);
    }
}