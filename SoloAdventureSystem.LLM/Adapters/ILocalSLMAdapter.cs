// Moved from SoloAdventureSystem.AIWorldGenerator/Adapters
namespace SoloAdventureSystem.ContentGenerator.Adapters
{
    public interface ILocalSLMAdapter
    {
        System.Threading.Tasks.Task InitializeAsync(IProgress<SoloAdventureSystem.ContentGenerator.EmbeddedModel.DownloadProgress>? progress = null);
        string GenerateRoomDescription(string context, int seed);
        string GenerateRaw(string prompt, int seed, int maxTokens = 150);
        string GenerateNpcBio(string context, int seed);
        string GenerateFactionFlavor(string context, int seed);
        System.Collections.Generic.List<string> GenerateLoreEntries(string context, int seed, int count);
        string GenerateDialogue(string prompt, int seed);
    }
}
