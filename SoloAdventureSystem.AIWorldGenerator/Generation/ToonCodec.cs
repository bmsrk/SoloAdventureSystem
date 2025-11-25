using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace SoloAdventureSystem.ContentGenerator.Generation;

/// <summary>
/// TOON parser implementation (read-only) with JSON fallback.
/// - Parses TOON tables and returns typed objects via JSON intermediary.
/// - Supports quoted fields, escaped quotes, numeric/bool/null/datetime parsing,
///   and JSON literals inside cells.
/// - Recognizes `#json\n` prefix to parse raw JSON directly.
/// </summary>
public static class ToonParser
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true, WriteIndented = false };

    /// <summary>
    /// Try to parse the input text as a TOON table or JSON and deserialize to T.
    /// Returns true if parsing + deserialization succeeded.
    /// </summary>
    public static bool TryParse<T>(string input, out T? result)
    {
        result = default;
        if (string.IsNullOrWhiteSpace(input)) return false;

        var text = input.Trim();

        // Allow explicit TOON markers: lines between #TOON and #ENDTOON
        if (text.IndexOf("#TOON", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            var start = text.IndexOf("#TOON", StringComparison.OrdinalIgnoreCase);
            var end = text.IndexOf("#ENDTOON", StringComparison.OrdinalIgnoreCase);
            if (start >= 0 && end > start)
            {
                var inner = text.Substring(start + 5, end - (start + 5));
                text = inner.Trim();
            }
        }

        // JSON marker shortcut
        if (text.StartsWith("#json\n", StringComparison.OrdinalIgnoreCase))
        {
            var json = text.Substring(6);
            try
            {
                result = JsonSerializer.Deserialize<T>(json, JsonOptions);
                return result != null;
            }
            catch
            {
                return false;
            }
        }

        // Normalize lines and drop comment lines starting with '#'
        var rawLines = text.Split(new[] { '\r', '\n' }, StringSplitOptions.None).Select(l => l.TrimEnd()).ToList();
        var lines = rawLines.Where(l => !string.IsNullOrWhiteSpace(l) && !l.TrimStart().StartsWith("#")).Select(l => l.Trim()).ToList();
        if (lines.Count == 0) return false;

        // Header pattern: name[N]{field1,field2,...}:
        var header = lines[0];
        var m = Regex.Match(header, "^(?<name>[A-Za-z0-9_]+)\\[(?<n>\\d+)\\]\\{(?<fields>[^}]+)\\}:?$");
        if (!m.Success)
        {
            // Not a table — try full-text JSON
            try
            {
                result = JsonSerializer.Deserialize<T>(text, JsonOptions);
                return result != null;
            }
            catch
            {
                return false;
            }
        }

        var fields = m.Groups["fields"].Value.Split(',').Select(s => s.Trim()).ToArray();
        var rowLines = lines.Skip(1).Where(l => !string.IsNullOrWhiteSpace(l)).ToList();

        var list = new List<Dictionary<string, object?>>();
        foreach (var row in rowLines)
        {
            var cols = SplitRow(row).ToArray();
            var dict = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < fields.Length; i++)
            {
                var raw = i < cols.Length ? cols[i] : string.Empty;
                var unq = Unquote(raw?.Trim() ?? string.Empty);
                var val = ParseValue(unq);
                dict[fields[i]] = val;
            }
            list.Add(dict);
        }

        // Convert to JSON then to target type
        try
        {
            var jsonList = JsonSerializer.Serialize(list, JsonOptions);
            result = JsonSerializer.Deserialize<T>(jsonList, JsonOptions);
            return result != null;
        }
        catch
        {
            result = default;
            return false;
        }
    }

    private static IEnumerable<string> SplitRow(string row)
    {
        if (string.IsNullOrEmpty(row)) yield break;
        var sb = new StringBuilder();
        bool inQuotes = false;
        for (int i = 0; i < row.Length; i++)
        {
            var c = row[i];
            if (c == '"')
            {
                if (inQuotes && i + 1 < row.Length && row[i + 1] == '"')
                {
                    sb.Append('"');
                    i++; // skip escaped quote
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (c == ',' && !inQuotes)
            {
                yield return sb.ToString();
                sb.Clear();
            }
            else
            {
                sb.Append(c);
            }
        }
        yield return sb.ToString();
    }

    private static string Unquote(string s)
    {
        if (string.IsNullOrEmpty(s)) return string.Empty;
        s = s.Trim();
        if (s.Length >= 2 && s[0] == '"' && s[^1] == '"')
        {
            var inner = s.Substring(1, s.Length - 2);
            // replace doubled quotes "" with single quote "
            return inner.Replace("\"\"", "\"");
        }
        return s;
    }

    private static object? ParseValue(string s)
    {
        if (s == null) return string.Empty;
        s = s.Trim();
        if (s.Length == 0) return string.Empty;

        // JSON object/array literal
        if ((s.StartsWith("{") && s.EndsWith("}")) || (s.StartsWith("[") && s.EndsWith("]")))
        {
            try
            {
                return JsonSerializer.Deserialize<object>(s, JsonOptions);
            }
            catch
            {
                // fall through
            }
        }

        if (string.Equals(s, "null", StringComparison.OrdinalIgnoreCase)) return null;
        if (string.Equals(s, "true", StringComparison.OrdinalIgnoreCase)) return true;
        if (string.Equals(s, "false", StringComparison.OrdinalIgnoreCase)) return false;

        if (long.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var li)) return li;
        if (double.TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out var dv)) return dv;
        if (DateTime.TryParse(s, null, DateTimeStyles.RoundtripKind, out var dt)) return dt;

        return s;
    }
}

/// <summary>
/// Compatibility wrapper to match previous API name `ToonCodec.Deserialize<T>` used elsewhere in the codebase.
/// This delegates to `ToonParser` and returns default(T) on failure.
/// </summary>
public static class ToonCodec
{
    public static T? Deserialize<T>(string input)
    {
        if (ToonParser.TryParse(input, out T? res)) return res;
        return default;
    }
}
