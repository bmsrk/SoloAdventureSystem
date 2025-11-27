using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace SoloAdventureSystem.ContentGenerator.Generation
{
    /// <summary>
    /// Helper to attempt structured generation with retries and fall back to legacy generation methods.
    /// </summary>
    public static class GenerationValidator
    {
        /// <summary>
        /// Attempts to obtain structured output by calling <paramref name="rawGenerator"/> with the
        /// primary prompt and any alternate prompts. The <paramref name="structuredValidator"/>
        /// determines whether a returned raw string qualifies as structured. If no structured output
        /// is produced, <paramref name="fallbackGenerator"/> is invoked (if provided) and its result
        /// is returned.
        /// </summary>
        public static string EnsureStructuredOrFallback(
            Func<string, string> rawGenerator,
            string primaryPrompt,
            IEnumerable<string>? alternatePrompts,
            Func<string, bool> structuredValidator,
            Func<string>? fallbackGenerator = null,
            ILogger? logger = null)
        {
            if (rawGenerator == null) throw new ArgumentNullException(nameof(rawGenerator));
            if (structuredValidator == null) throw new ArgumentNullException(nameof(structuredValidator));

            try
            {
                // Primary
                var raw = SafeGenerate(rawGenerator, primaryPrompt, logger);
                if (!string.IsNullOrWhiteSpace(raw) && structuredValidator(raw))
                {
                    logger?.LogDebug("Structured output obtained from primary prompt (len={Len})", raw.Length);
                    return raw;
                }

                // Alternates
                if (alternatePrompts != null)
                {
                    foreach (var alt in alternatePrompts)
                    {
                        try
                        {
                            var altRaw = SafeGenerate(rawGenerator, alt, logger);
                            if (!string.IsNullOrWhiteSpace(altRaw) && structuredValidator(altRaw))
                            {
                                logger?.LogDebug("Structured output obtained from alternate prompt (len={Len})", altRaw.Length);
                                return altRaw;
                            }
                        }
                        catch (Exception ex)
                        {
                            logger?.LogDebug(ex, "Alternate prompt generation failed");
                        }
                    }
                }

                // Final fallback to legacy generator
                if (fallbackGenerator != null)
                {
                    try
                    {
                        var fb = fallbackGenerator();
                        if (!string.IsNullOrWhiteSpace(fb))
                        {
                            logger?.LogDebug("Fallback generator returned non-empty output (len={Len})", fb.Length);
                            return fb;
                        }
                    }
                    catch (Exception ex)
                    {
                        logger?.LogDebug(ex, "Fallback generator threw");
                    }
                }

                // If nothing produced structured content, return last primary raw (may be empty)
                return rawGenerator(primaryPrompt) ?? string.Empty;
            }
            catch (Exception ex)
            {
                logger?.LogWarning(ex, "Generation validation failed unexpectedly");
                try
                {
                    return fallbackGenerator != null ? fallbackGenerator() ?? string.Empty : string.Empty;
                }
                catch
                {
                    return string.Empty;
                }
            }
        }

        private static string SafeGenerate(Func<string, string> gen, string prompt, ILogger? logger)
        {
            try
            {
                return gen(prompt) ?? string.Empty;
            }
            catch (Exception ex)
            {
                logger?.LogDebug(ex, "Generation call threw for prompt");
                return string.Empty;
            }
        }
    }
}
