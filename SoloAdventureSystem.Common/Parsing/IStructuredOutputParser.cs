using System;

namespace SoloAdventureSystem.Common.Parsing
{
    public interface IStructuredOutputParser
    {
        bool TryParse<T>(string raw, out T? result);
    }
}
