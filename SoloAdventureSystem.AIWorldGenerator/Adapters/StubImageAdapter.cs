namespace SoloAdventureSystem.ContentGenerator
{
    public class StubImageAdapter : IImageAdapter
    {
        public string GenerateImagePrompt(string context, int seed)
        {
            return $"Image prompt for '{context}' (seed {seed})";
        }
        public byte[]? RenderImage(string prompt, int seed)
        {
            // Return null for stub (no image rendered)
            return null;
        }
    }
}
