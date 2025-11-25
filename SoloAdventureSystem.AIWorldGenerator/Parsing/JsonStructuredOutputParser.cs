using System;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace SoloAdventureSystem.ContentGenerator.Parsing
{
    public class JsonStructuredOutputParser : IStructuredOutputParser
    {
        public bool TryParse<T>(string raw, out T? result)
        {
            result = default;
            if (string.IsNullOrWhiteSpace(raw)) return false;

            // Try direct JSON
            try
            {
                result = JsonSerializer.Deserialize<T>(raw, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (result != null) return true;
            }
            catch { }

            // #json prefix
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

            // Extract first JSON substring
            try
            {
                var firstJson = ExtractJsonFromText(raw);
                if (!string.IsNullOrWhiteSpace(firstJson))
                {
                    result = JsonSerializer.Deserialize<T>(firstJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (result != null) return true;
                }
            }
            catch { }

            return false;
        }

        private static string? ExtractJsonFromText(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return null;
            int start = text.IndexOf('{');
            int startArr = text.IndexOf('[');
            if (start == -1 && startArr == -1) return null;

            if (start == -1 || (startArr >= 0 && startArr < start)) start = startArr;

            int depth = 0;
            bool inString = false;
            for (int i = start; i < text.Length; i++)
            {
                var c = text[i];
                if (c == '"') inString = !inString;
                if (inString) continue;
                if (c == '{' || c == '[') depth++;
                if (c == '}' || c == ']')
                {
                    depth--;
                    if (depth == 0)
                    {
                        return text.Substring(start, i - start + 1);
                    }
                }
            }

            return null;
        }
    }
}
