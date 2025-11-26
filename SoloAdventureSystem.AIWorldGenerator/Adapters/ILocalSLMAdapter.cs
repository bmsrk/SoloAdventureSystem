namespace SoloAdventureSystem.ContentGenerator.Adapters;

/// <summary>
/// Abstraction for local SLM adapters. Implementations provide synchronous helper
/// methods used throughout the generation pipeline. Initialization may perform
/// potentially long-running work (model download/load) so it is async.
/// </summary>
public interface ILocalSLMAdapter
{
    System.Threading.Tasks.Task InitializeAsync(IProgress<SoloAdventureSystem.ContentGenerator.EmbeddedModel.DownloadProgress>? progress = null);

    string GenerateRoomDescription(string context, int seed);

    string GenerateNpcBio(string context, int seed);

    string GenerateFactionFlavor(string context, int seed);

    System.Collections.Generic.List<string> GenerateLoreEntries(string context, int seed, int count);

    string GenerateDialogue(string prompt, int seed);

    /// <summary>
    /// Returns raw un-cleaned generation and may accept a maxTokens hint.
    /// </summary>
    string GenerateRaw(string prompt, int seed, int maxTokens = 150);
}