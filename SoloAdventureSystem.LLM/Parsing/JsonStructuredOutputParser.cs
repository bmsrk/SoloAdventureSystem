using System;
using System.Text.Json;

namespace SoloAdventureSystem.LLM.Parsing
{
    public class JsonStructuredOutputParser : IStructuredOutputParser
    {
        private static readonly JsonSerializerOptions _defaultOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        public bool TryParse<T>(string raw, out T? result)
        {
            result = default;
            if (string.IsNullOrWhiteSpace(raw)) return false;

            try
            {
                result = JsonSerializer.Deserialize<T>(raw, _defaultOptions);
                if (result != null) return true;
            }
            catch { }

            try
            {
                if (raw.StartsWith("#json\n", StringComparison.OrdinalIgnoreCase))
                {
                    var json = raw.Substring(6);
                    result = JsonSerializer.Deserialize<T>(json, _defaultOptions);
                    if (result != null) return true;
                }
            }
            catch { }

            return false;
        }
    }
}
