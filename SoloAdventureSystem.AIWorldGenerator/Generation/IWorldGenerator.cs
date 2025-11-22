namespace SoloAdventureSystem.ContentGenerator
{
    public interface IWorldGenerator
    {
        WorldGenerationResult Generate(WorldGenerationOptions options);
    }
}
