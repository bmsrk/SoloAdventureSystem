namespace SoloAdventureSystem.ContentGenerator.Adapters
{
    public interface ILocalSLMAdapter
    {
        System.Threading.Tasks.Task InitializeAsync(IProgress<EmbeddedModel.DownloadProgress>? progress = null);
        string GenerateRoomDescription(string context, int seed);
        string GenerateNpcBio(string context, int seed);
        string GenerateFactionFlavor(string context, int seed);
        List<string> GenerateLoreEntries(string context, int seed, int count);
        // New: generate dialogue JSON (pregenerated dialogue nodes and choices)
        string GenerateDialogue(string prompt, int seed);
    }
}
