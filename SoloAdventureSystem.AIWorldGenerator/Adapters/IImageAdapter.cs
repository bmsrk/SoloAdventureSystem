namespace SoloAdventureSystem.ContentGenerator.Adapters;

/// <summary>
/// Simple abstraction for image prompt generation and optional rendering
/// </summary>
public interface IImageAdapter
{
    string GenerateImagePrompt(string context, int seed);
    byte[]? RenderImage(string prompt, int seed);
}