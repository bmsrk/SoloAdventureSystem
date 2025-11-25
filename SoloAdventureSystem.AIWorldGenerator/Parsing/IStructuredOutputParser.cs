using System;

namespace SoloAdventureSystem.ContentGenerator.Parsing
{
    public interface IStructuredOutputParser
    {
        /// <summary>
        /// Try to parse structured output from free-form generated text into the requested type T.
        /// Implementations should handle direct JSON, common prefixes, and heuristic extraction.
        /// </summary>
        bool TryParse<T>(string raw, out T? result);
    }
}
