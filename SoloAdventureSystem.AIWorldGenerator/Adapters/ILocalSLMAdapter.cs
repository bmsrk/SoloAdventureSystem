namespace SoloAdventureSystem.ContentGenerator
{
    public interface ILocalSLMAdapter
    {
        string GenerateRoomDescription(string context, int seed);
        string GenerateNpcBio(string context, int seed);
        string GenerateFactionFlavor(string context, int seed);
        List<string> GenerateLoreEntries(string context, int seed, int count);
    }
}
