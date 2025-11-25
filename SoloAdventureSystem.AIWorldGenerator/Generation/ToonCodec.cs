using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace SoloAdventureSystem.ContentGenerator.Generation;

/// <summary>
/// Minimal TOON codec implementation tailored for this project.
/// - Tries to encode uniform arrays of objects as TOON tables.
/// - For complex objects falls back to embedding compact JSON prefixed with "#json\n".
/// - Decoder supports the simple table form and the JSON fallback.
/// This is a small, self-contained implementation to avoid external dependencies.
/// </summary>
public static class ToonCodec
{
    public static string Serialize<T>(T obj)
    {
        if (obj == null) return string.Empty;

        // If obj is a list/array of uniform objects, attempt table encoding
        if (obj is System.Collections.IEnumerable enumerable && !(obj is string))
        {
            var items = enumerable.Cast<object>().ToList();
            if (items.Count == 0)
            {
                return "#json\n[]";
            }

            // Try to extract property names from first item
            var first = items[0];
            var props = first.GetType().GetProperties()
                .Where(p => IsSimpleType(p.PropertyType))
                .Select(p => p.Name)
                .ToList();

            if (props.Count > 0 && items.All(it => it.GetType().GetProperties().Select(p => p.Name).Count() == first.GetType().GetProperties().Length))
            {
                // Build TOON table header: name[N]{fields}:
                var headerName = "items";
                var sb = new StringBuilder();
                sb.AppendLine($"{headerName}[{items.Count}]{{{string.Join(',', props)}}}:\n");
                foreach (var it in items)
                {
                    var values = props.Select(pn => SerializeSimpleValue(it.GetType().GetProperty(pn)?.GetValue(it))).ToArray();
                    sb.AppendLine("  " + string.Join(',', values));
                }

                return sb.ToString().TrimEnd();
            }
        }

        // Fallback: embed compact JSON with marker
        var json = JsonSerializer.Serialize(obj, new JsonSerializerOptions { WriteIndented = false });
        return "#json\n" + json;
    }

    public static T? Deserialize<T>(string toon)
    {
        if (string.IsNullOrWhiteSpace(toon)) return default;

        toon = toon.Trim();
        if (toon.StartsWith("#json\n", StringComparison.OrdinalIgnoreCase))
        {
            var json = toon.Substring(6);
            return JsonSerializer.Deserialize<T>(json);
        }

        // Attempt to parse simple table format
        // Header: name[N]{field1,field2,...}:
        var lines = toon.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(l => l.Trim()).ToList();
        if (lines.Count == 0) return default;

        var header = lines[0];
        var headerMatch = System.Text.RegularExpressions.Regex.Match(header, "^(?<name>[a-zA-Z0-9_]+)\\[(?<n>\\d+)\\]\\{(?<fields>[^}]+)\\}:?$");
        if (!headerMatch.Success)
        {
            // Not a table we can parse, try JSON parse of whole text
            try
            {
                return JsonSerializer.Deserialize<T>(toon);
            }
            catch
            {
                return default;
            }
        }

        var fields = headerMatch.Groups["fields"].Value.Split(',').Select(s => s.Trim()).ToArray();
        var itemLines = lines.Skip(1).Where(l => !string.IsNullOrWhiteSpace(l)).ToList();

        var list = new List<Dictionary<string, object?>>();
        foreach (var l in itemLines)
        {
            // remove leading indentation
            var row = l.TrimStart();
            // split by commas but keep quoted commas
            var values = SplitCsvRow(row).ToArray();
            var dict = new Dictionary<string, object?>();
            for (int i = 0; i < fields.Length; i++)
            {
                var val = i < values.Length ? Unquote(values[i]) : string.Empty;
                dict[fields[i]] = val;
            }
            list.Add(dict);
        }

        // Convert list of dictionaries to JSON then to target type
        var jsonList = JsonSerializer.Serialize(list);
        try
        {
            return JsonSerializer.Deserialize<T>(jsonList);
        }
        catch
        {
            return default;
        }
    }

    private static bool IsSimpleType(Type t)
    {
        return t.IsPrimitive || t == typeof(string) || t == typeof(decimal) || t == typeof(DateTime) || (Nullable.GetUnderlyingType(t) != null && IsSimpleType(Nullable.GetUnderlyingType(t)));
    }

    private static string SerializeSimpleValue(object? v)
    {
        if (v == null) return string.Empty;
        if (v is string s)
        {
            // Escape commas and newlines by quoting
            if (s.Contains(',') || s.Contains('\n') || s.Contains('\r') || s.Contains('"'))
            {
                var esc = s.Replace("\"", "\"\"");
                return '"' + esc + '"';
            }
            return s;
        }

        if (v is DateTime dt) return dt.ToString("o");
        if (v is bool b) return b ? "true" : "false";
        return Convert.ToString(v, System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty;
    }

    private static IEnumerable<string> SplitCsvRow(string row)
    {
        var sb = new StringBuilder();
        bool inQuotes = false;
        for (int i = 0; i < row.Length; i++)
        {
            var c = row[i];
            if (c == '"')
            {
                if (inQuotes && i + 1 < row.Length && row[i + 1] == '"')
                {
                    // escaped quote
                    sb.Append('"');
                    i++; // skip next
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
        if (s.Length >= 2 && s.StartsWith('"') && s.EndsWith('"'))
        {
            var inner = s.Substring(1, s.Length - 2);
            return inner.Replace("\"\"", "\"");
        }
        return s;
    }
}
