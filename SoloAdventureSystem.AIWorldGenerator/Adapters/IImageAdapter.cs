namespace SoloAdventureSystem.ContentGenerator
{
    public interface IImageAdapter
    {
        string GenerateImagePrompt(string context, int seed);
        byte[]? RenderImage(string prompt, int seed);
    }
}
