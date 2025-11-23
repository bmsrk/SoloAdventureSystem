using SoloAdventureSystem.ContentGenerator.Models;

namespace SoloAdventureSystem.ContentGenerator.Generation
{
    public interface IWorldGenerator
    {
        WorldGenerationResult Generate(WorldGenerationOptions options);
    }
}
