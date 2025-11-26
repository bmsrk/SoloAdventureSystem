namespace SoloAdventureSystem.LLM.Parsing
{
    public interface IStructuredOutputParser
    {
        bool TryParse<T>(string raw, out T? result);
    }
}
