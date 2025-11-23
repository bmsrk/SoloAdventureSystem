namespace SoloAdventureSystem.ContentGenerator.Adapters;

/// <summary>
/// Simple image adapter that generates descriptive prompts without rendering.
/// Used for generating visual descriptions for world content.
/// </summary>
public class SimpleImageAdapter : IImageAdapter
{
    public string GenerateImagePrompt(string context, int seed)
    {
        // Generate a descriptive prompt based on context
        return $"A detailed visualization of: {context} (rendered with seed {seed})";
    }

    public byte[]? RenderImage(string prompt, int seed)
    {
        // No actual image rendering - return null
        return null;
    }
}
