using System;
using System.Text.Json;

namespace SoloAdventureSystem.LLM.Parsing
{
    public class JsonStructuredOutputParser : IStructuredOutputParser
    {
        public bool TryParse<T>(string raw, out T? result)
        {
            result = default;
            if (string.IsNullOrWhiteSpace(raw)) return false;

            try
            {
                result = JsonSerializer.Deserialize<T>(raw, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (result != null) return true;
            }
            catch { }

            try
            {
                if (raw.StartsWith("#json\n", StringComparison.OrdinalIgnoreCase))
                {
                    var json = raw.Substring(6);
                    result = JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (result != null) return true;
                }
            }
            catch { }

            return false;
        }
    }
}
