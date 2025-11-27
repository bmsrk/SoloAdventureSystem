using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Text;
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

                // Remove ChatML / role markers and similar tokens that sometimes leak
                text = Regex.Replace(text, @"<\|/?(user|assistant|system)\|>", "", RegexOptions.IgnoreCase);
                text = Regex.Replace(text, @"<\|im_start\|>|<\|im_end\|>", "", RegexOptions.IgnoreCase);
                text = Regex.Replace(text, @"<\|begin_of_text\|>|<\|end_of_text\|>", "", RegexOptions.IgnoreCase);

                // Remove leading '#json' marker lines
                text = Regex.Replace(text, @"^\s*#json\s*\n", "", RegexOptions.IgnoreCase);

                // Remove leading triple-backtick fences (``` or ```json)
                text = Regex.Replace(text, @"^\s*```(?:json)?\s*", "", RegexOptions.IgnoreCase);

                // Remove an initial leading double-quote if present
                if (text.Length > 0 && text[0] == '"') text = text.Substring(1);

                // Remove common prompt fragments that sometimes appear when model repeats prompt
                text = Regex.Replace(text, "You are a creative writer.*", "", RegexOptions.IgnoreCase);

                // Remove explicit output-spec lines and examples that sometimes get echoed
                text = Regex.Replace(text, @"^(Return ONLY.*|OUTPUT SPEC:.*|GOOD example:.*|BAD example:.*|Example output:).*\r?$", "", RegexOptions.IgnoreCase | RegexOptions.Multiline);

                // Normalize whitespace early
                text = Regex.Replace(text, @"\r\n|\r|\n", "\n");

                // Split into lines to strip obvious prompt-style lines
                var lines = text.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(l => l.Trim()).ToList();
                // Remove lines that are very long (likely contain prompt context) or start with instruction words
                for (int i = lines.Count - 1; i >= 0; i--)
                {
                    var l = lines[i];
                    if (string.IsNullOrWhiteSpace(l)) { lines.RemoveAt(i); continue; }

                    // If line starts with common instruction phrases, remove it
                    if (Regex.IsMatch(l, @"^(DO NOT|DO|PLEASE|RETURN|RETURN ONLY|OUTPUT|EXAMPLE|GOOD EXAMPLE|BAD EXAMPLE|PRODUCE|FORMAT|RETURN ONLY THE|RETURN ONLY THE JSON|RETURN ONLY THE TEXT)\b", RegexOptions.IgnoreCase))
                    {
                        lines.RemoveAt(i);
                        continue;
                    }

                    // If line is long (more than 40 words) and contains words like 'World:' 'Room:' 'Context:' it's probably a leaked prompt
                    var wordCount = l.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
                    if (wordCount > 40 && Regex.IsMatch(l, @"\b(World|Room|Context|Plot|Theme|Time|Unique ID|World:)\b", RegexOptions.IgnoreCase))
                    {
                        lines.RemoveAt(i);
                        continue;
                    }

                    // If line is mostly uppercase and length > 8, treat as instruction and remove
                    var letters = l.Count(char.IsLetter);
                    if (letters > 0)
                    {
                        var upperLetters = l.Count(c => char.IsUpper(c));
                        if (letters >= 4 && (double)upperLetters / letters > 0.6)
                        {
                            lines.RemoveAt(i);
                            continue;
                        }
                    }
                }

                // Rejoin lines and continue cleaning
                text = string.Join(" ", lines.Where(s => !string.IsNullOrWhiteSpace(s))).Trim();

                // Remove TOON/ENDTOON markers and hashtags
                text = Regex.Replace(text, @"\b#?TOON\b", "", RegexOptions.IgnoreCase);
                text = Regex.Replace(text, @"\b#?ENDTOON\b", "", RegexOptions.IgnoreCase);
                text = Regex.Replace(text, @"#\w+", "");
                text = Regex.Replace(text, @"\(s\)", "");

                // Collapse repeated sentences/fragments and remove instruction-like sentences
                var sentences = Regex.Split(text, @"(?<=[\.\!\?])\s+");
                var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                var kept = new List<string>();

                foreach (var s in sentences)
                {
                    var t = s.Trim();
                    if (string.IsNullOrEmpty(t)) continue;

                    // Remove sentences that look like instructions: start with DO NOT, PLEASE, RETURN, etc.
                    if (Regex.IsMatch(t, @"^(DO NOT|PLEASE|RETURN|RETURN ONLY|OUTPUT|EXAMPLE|PRODUCE|FORMAT|RETURN ONLY THE)\b", RegexOptions.IgnoreCase))
                        continue;

                    // Remove sentences that are mostly uppercase
                    var lettersCount = t.Count(char.IsLetter);
                    if (lettersCount >= 4)
                    {
                        var upperCount = t.Count(char.IsUpper);
                        if ((double)upperCount / lettersCount > 0.6)
                            continue;
                    }

                    // Drop excessively long sentences that look like prompts
                    var wc = t.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
                    if (wc > 60 && Regex.IsMatch(t, @"\b(World|Room|Context|Plot|Theme|Time|Unique ID)\b", RegexOptions.IgnoreCase))
                        continue;

                    var key = Regex.Replace(t.ToLowerInvariant(), @"[\p{P}\s]+", " ").Trim();
                    if (!seen.Contains(key)) { kept.Add(t); seen.Add(key); }
                }

                text = kept.Count > 0 ? string.Join(" ", kept) : string.Empty;

                // Final whitespace normalization and defensive cleanup
                text = Regex.Replace(text, @"\s+", " ").Trim();
                text = Regex.Replace(text, @"<\|/?(user|assistant|system)\|>", "", RegexOptions.IgnoreCase);
                text = Regex.Replace(text, @"(Return ONLY the JSON object|Return ONLY the text|Return only the JSON object|Return only the text)", "", RegexOptions.IgnoreCase);
            }
            catch
            {
                text = (text ?? string.Empty).Trim();
            }

            return text;
        }

        /// <summary>
        /// Normalize whitespace consistently across generators.
        /// </summary>
        public static string NormalizeWhitespace(string s)
        {
            if (string.IsNullOrEmpty(s)) return s;
            return Regex.Replace(s, @"\s+", " ").Trim();
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

        /// <summary>
        /// Cleans a parsed field (name, short description) by removing instruction-like sentences,
        /// stripping all-caps instruction fragments, and truncating to a sensible max length.
        /// </summary>
        public static string CleanParsedField(string? input, int maxLen = 400)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;
            var s = input.Trim();

            // Remove role markers and fences
            s = Regex.Replace(s, @"<\|/?(user|assistant|system)\|>", "", RegexOptions.IgnoreCase);
            s = Regex.Replace(s, @"^\s*```(?:json)?\s*|```\s*$", "", RegexOptions.IgnoreCase);
            s = Regex.Replace(s, @"^\s*#json\s*", "", RegexOptions.IgnoreCase);

            // Split into sentences and keep only those that are not instruction-like
            var sentences = Regex.Split(s, @"(?<=[\.\!\?])\s+");
            var kept = new List<string>();
            foreach (var sent in sentences)
            {
                var t = sent.Trim();
                if (string.IsNullOrEmpty(t)) continue;

                // Remove sentences starting with instruction verbs
                if (Regex.IsMatch(t, @"^(DO NOT|DO|PLEASE|RETURN|RETURN ONLY|OUTPUT|EXAMPLE|PRODUCE|FORMAT|USE|RETURN ONLY THE)\b", RegexOptions.IgnoreCase))
                    continue;

                // Remove sentences that are mostly uppercase
                var letters = t.Count(char.IsLetter);
                if (letters >= 4)
                {
                    var upper = t.Count(char.IsUpper);
                    if ((double)upper / letters > 0.6) continue;
                }

                kept.Add(t);
            }

            var result = kept.Count > 0 ? string.Join(" ", kept) : s;

            // Truncate to maxLen while preserving words
            if (result.Length > maxLen)
            {
                var cut = result.Substring(0, maxLen);
                var lastSpace = cut.LastIndexOf(' ');
                if (lastSpace > 0) cut = cut.Substring(0, lastSpace);
                result = cut.Trim() + "...";
            }

            // Final normalize
            result = Regex.Replace(result, @"\s+", " ").Trim();
            return result;
        }
    }
}
