namespace SoloAdventureSystem.ContentGenerator.Adapters
{
    public interface ILocalSLMAdapter
    {
        System.Threading.Tasks.Task InitializeAsync(IProgress<EmbeddedModel.DownloadProgress>? progress = null);
        string GenerateRoomDescription(string context, int seed);
        // Return raw model output (un-cleaned) suitable for structured parsers to extract JSON/TOON
        string GenerateRaw(string prompt, int seed, int maxTokens = 150);
        string GenerateNpcBio(string context, int seed);
        string GenerateFactionFlavor(string context, int seed);
        List<string> GenerateLoreEntries(string context, int seed, int count);
        // New: generate dialogue JSON (pregenerated dialogue nodes and choices)
        string GenerateDialogue(string prompt, int seed);
    }
}
