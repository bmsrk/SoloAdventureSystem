using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using SoloAdventureSystem.ContentGenerator.Models;
using SoloAdventureSystem.Engine.Rules;

namespace SoloAdventureSystem.ContentGenerator.Generation
{
    /// <summary>
    /// Shared utilities for generation: sanitization and parsing helpers extracted from generators.
    /// </summary>
    public static class GenerationUtils
    {
        public static string SanitizeGeneratedText(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return string.Empty;

            try
            {
                // Collapse obvious '# #' artifacts and solitary hashes that leak from model output
                text = Regex.Replace(text, @"#\s*#", " ", RegexOptions.Compiled);
                text = Regex.Replace(text, @"\s#\s", " ", RegexOptions.Compiled);

                // Remove leading instruction markers like '#json', leading quote, or code fences (```json)
                text = Regex.Replace(text, "^\\s*(?:#json\\n|\"|```json|```)\\.*", "", RegexOptions.IgnoreCase);

                // Remove common prompt fragments that sometimes appear when model repeats prompt
                text = Regex.Replace(text, "You are a creative writer.*", "", RegexOptions.IgnoreCase);

                // Trim and normalize whitespace
                text = Regex.Replace(text, "\\s+", " ").Trim();

                // Remove TOON/ENDTOON markers and hashtags
                text = Regex.Replace(text, @"\b#?TOON\b", "", RegexOptions.IgnoreCase);
                text = Regex.Replace(text, @"\b#?ENDTOON\b", "", RegexOptions.IgnoreCase);
                text = Regex.Replace(text, @"#\w+", "");
                text = Regex.Replace(text, @"\(s\)", "");

                // Collapse repeated sentences/fragments
                var sentences = Regex.Split(text, "(?<=[\\.!?])\\s+");
                var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                var sb = new List<string>();
                foreach (var s in sentences)
                {
                    var t = s.Trim();
                    if (string.IsNullOrEmpty(t)) continue;
                    var key = Regex.Replace(t.ToLowerInvariant(), "[\\p{P}\\s]+", " ").Trim();
                    if (!seen.Contains(key)) { sb.Add(t); seen.Add(key); }
                }
                if (sb.Count > 0) text = string.Join(" ", sb);
                text = Regex.Replace(text, "\\s+", " ").Trim();
            }
            catch
            {
                // best-effort: if something goes wrong, return a trimmed version
                text = (text ?? string.Empty).Trim();
            }

            return text;
        }

        /// <summary>
        /// Attempts to extract a JSON array from a text blob. If the text contains a single JSON object,
        /// it will be wrapped into an array. Returns null if no JSON-like content is found.
        /// </summary>
        public static string? ExtractJsonArraySnippet(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return null;
            var trimmed = text.Trim();
            var startIdx = trimmed.IndexOf('[');
            var endIdx = trimmed.LastIndexOf(']');
            if (startIdx >= 0 && endIdx > startIdx)
            {
                return trimmed.Substring(startIdx, endIdx - startIdx + 1);
            }

            // Some models return a single object; wrap it to normalize to array
            if (trimmed.StartsWith("{") && !trimmed.StartsWith("["))
            {
                return "[" + trimmed + "]";
            }

            // Try to find embedded JSON object and wrap
            var objStart = trimmed.IndexOf('{');
            var objEnd = trimmed.LastIndexOf('}');
            if (objStart >= 0 && objEnd > objStart)
            {
                return "[" + trimmed.Substring(objStart, objEnd - objStart + 1) + "]";
            }

            return null;
        }

        /// <summary>
        /// Parses a JSON array representing dialogue choices produced by an LLM into StoryChoice list.
        /// Accepts several field name variants and performs defensive sanitation.
        /// </summary>
        public static List<StoryChoice>? ParseChoicesFromJson(string rawJson, string npcId, ILogger? logger = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(rawJson)) return null;

                var cleaned = rawJson;
                try
                {
                    cleaned = Regex.Replace(cleaned, @"#\s*#", " ", RegexOptions.Compiled);
                    cleaned = Regex.Replace(cleaned, @"\s#\s", " ", RegexOptions.Compiled);
                    cleaned = Regex.Replace(cleaned, @"\b#?TOON\b", "", RegexOptions.IgnoreCase);
                    cleaned = Regex.Replace(cleaned, @"\b#?ENDTOON\b", "", RegexOptions.IgnoreCase);
                    cleaned = Regex.Replace(cleaned, @"#\w+", "");
                }
                catch { }

                var json = ExtractJsonArraySnippet(cleaned) ?? cleaned;

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                List<ChoiceDto>? choicesDto = null;
                try
                {
                    choicesDto = JsonSerializer.Deserialize<List<ChoiceDto>>(json, options);
                }
                catch (JsonException) { /* leave null to trigger fallback */ }

                if (choicesDto == null || choicesDto.Count == 0) return null;

                var result = new List<StoryChoice>();
                foreach (var c in choicesDto)
                {
                    var label = (c.label ?? c.text ?? c.option)?.Trim() ?? string.Empty;
                    if (string.IsNullOrEmpty(label)) continue;

                    var nextCandidate = (c.nextSuffix ?? c.next ?? string.Empty).Trim();
                    string next;
                    if (string.IsNullOrEmpty(nextCandidate))
                    {
                        next = $"dialogue_{npcId}_end";
                    }
                    else
                    {
                        if (nextCandidate.StartsWith("dialogue_", StringComparison.OrdinalIgnoreCase))
                        {
                            next = Regex.Replace(nextCandidate, "[^a-zA-Z0-9_\\-]", "").Trim();
                        }
                        else
                        {
                            var suffix = Regex.Replace(nextCandidate, "[^a-zA-Z0-9_\\-]", "").Trim();
                            next = string.IsNullOrEmpty(suffix) ? $"dialogue_{npcId}_end" : $"dialogue_{npcId}_" + suffix;
                        }
                    }

                    SkillCheckModel? sc = null;
                    if (c.skill_check != null)
                    {
                        var attrStr = c.skill_check.attribute ?? "Soul";
                        var skillStr = c.skill_check.skill ?? "Social";
                        var tn = c.skill_check.target_number;
                        if (tn <= 0) tn = 10;
                        tn = Math.Max(3, Math.Min(20, tn));

                        if (Enum.TryParse<GameAttribute>(attrStr, true, out var attr))
                        {
                            if (Enum.TryParse<Skill>(skillStr, true, out var sk))
                            {
                                sc = new SkillCheckModel
                                {
                                    Attribute = attr,
                                    Skill = sk,
                                    TargetNumber = tn,
                                    OpponentNpcId = npcId
                                };
                            }
                        }
                    }

                    var effects = c.effects?.Where(e => !string.IsNullOrWhiteSpace(e)).Select(e => e.Trim()).ToList() ?? new List<string>();

                    result.Add(new StoryChoice
                    {
                        Label = label,
                        Next = next,
                        Effects = effects,
                        SkillCheck = sc
                    });
                }

                return result.Count == 0 ? null : result;
            }
            catch (Exception ex)
            {
                logger?.LogWarning(ex, "Failed to parse dialogue JSON for NPC {NpcId}: {Message}", npcId, ex.Message);
                return null;
            }
        }
    }
}
