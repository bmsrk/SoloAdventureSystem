namespace SoloAdventureSystem.LLM.Configuration
{
    public sealed record AIProvider(string Name)
    {
        public static AIProvider Stub { get; } = new("Stub");
        public static AIProvider LLamaSharp { get; } = new("LLamaSharp");
    }
}
