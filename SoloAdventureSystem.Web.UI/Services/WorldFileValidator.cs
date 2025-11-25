using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace SoloAdventureSystem.Web.UI.Services
{
    public class WorldValidationResult
    {
        public string WorldName { get; set; } = "";
        public string WorldPath { get; set; } = "";
        public DateTime ValidatedAt { get; set; } = DateTime.UtcNow;
        public int RoomScore { get; set; }
        public int NpcScore { get; set; }
        public int FactionScore { get; set; }
        public int ConsistencyScore { get; set; }
        public int OverallScore { get; set; }
        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
        // New: batch test metrics
        public int UniquenessScore { get; set; } // 0-100: how unique titles/descriptions are
        public int TitlePresenceScore { get; set; } // 0-100: percentage of entities with proper titles
        public List<string> BatchErrors { get; set; } = new(); // Errors from batch analysis
        public List<string> BatchWarnings { get; set; } = new(); // Warnings from batch analysis
    }

    public class WorldFileValidator
    {
        /// <summary>
        /// Analyzes content quality for a single room description.
        /// Returns score 0-100 based on sentence count, sensory details, specifics.
        /// </summary>
        private static int ScoreRoomDescription(string description)
        {
            if (string.IsNullOrWhiteSpace(description)) return 0;

            var score = 0;
            var sentences = description.Split(new[] { '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);

            // Exact 3 sentences: +30
            if (sentences.Length == 3) score += 30;
            else if (sentences.Length >= 2 && sentences.Length <= 4) score += 20; // Close enough
            else score += 0;

            // Sensory details (sight, sound, smell, touch): +20
            var sensoryWords = new[] { "blue", "red", "green", "yellow", "black", "white", "humming", "buzzing", "smell", "taste", "touch", "feel", "sound", "light", "dark" };
            var hasSensory = sensoryWords.Any(word => description.ToLowerInvariant().Contains(word));
            if (hasSensory) score += 20;

            // Specific objects/details: +20
            var specificWords = new[] { "terminal", "cable", "server", "banner", "table", "goblet", "mushroom", "oak", "toy" };
            var hasSpecifics = specificWords.Any(word => description.ToLowerInvariant().Contains(word));
            if (hasSpecifics) score += 20;

            // No generic phrases: +10
            var genericPhrases = new[] { "some", "thing", "stuff", "etc", "and so on" };
            var hasGeneric = genericPhrases.Any(phrase => description.ToLowerInvariant().Contains(phrase));
            if (!hasGeneric) score += 10;

            // Length reasonable: +20
            if (description.Length >= 100 && description.Length <= 500) score += 20;

            return Math.Clamp(score, 0, 100);
        }

        /// <summary>
        /// Analyzes content quality for a single NPC bio.
        /// Returns score 0-100 based on sentence count, traits, specifics.
        /// </summary>
        private static int ScoreNpcBio(string bio)
        {
            if (string.IsNullOrWhiteSpace(bio)) return 0;

            var score = 0;
            var sentences = bio.Split(new[] { '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);

            // Exact 2 sentences: +30
            if (sentences.Length == 2) score += 30;
            else if (sentences.Length >= 1 && sentences.Length <= 3) score += 20;
            else score += 0;

            // Has defining trait/secret: +30
            var traitWords = new[] { "glows", "hides", "cannot", "secretly", "trait", "quirk", "implant", "locket" };
            var hasTrait = traitWords.Any(word => bio.ToLowerInvariant().Contains(word));
            if (hasTrait) score += 30;

            // Specific background/role: +20
            var roleWords = new[] { "hacker", "security", "steward", "whisperer", "collective", "guild" };
            var hasRole = roleWords.Any(word => bio.ToLowerInvariant().Contains(word));
            if (hasRole) score += 20;

            // No generic phrases: +10
            var genericPhrases = new[] { "some", "thing", "interesting", "complicated" };
            var hasGeneric = genericPhrases.Any(phrase => bio.ToLowerInvariant().Contains(phrase));
            if (!hasGeneric) score += 10;

            // Length reasonable: +10
            if (bio.Length >= 50 && bio.Length <= 300) score += 10;

            return Math.Clamp(score, 0, 100);
        }

        /// <summary>
        /// Analyzes content quality for a single faction description.
        /// Returns score 0-100 based on sentence count, structure, specifics.
        /// </summary>
        private static int ScoreFactionDescription(string description)
        {
            if (string.IsNullOrWhiteSpace(description)) return 0;

            var score = 0;
            var sentences = description.Split(new[] { '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);

            // Exact 3 sentences: +30
            if (sentences.Length == 3) score += 30;
            else if (sentences.Length >= 2 && sentences.Length <= 4) score += 20;
            else score += 0;

            // Has goal/cause: +20
            var goalWords = new[] { "hacks", "frees", "forges", "guards", "summons", "reclaim" };
            var hasGoal = goalWords.Any(word => description.ToLowerInvariant().Contains(word));
            if (hasGoal) score += 20;

            // Has territory/strength: +20
            var strengthWords = new[] { "havens", "network", "forges", "control", "temples", "followers" };
            var hasStrength = strengthWords.Any(word => description.ToLowerInvariant().Contains(word));
            if (hasStrength) score += 20;

            // Has enemy/conflict: +20
            var enemyWords = new[] { "hunt", "vie", "seek", "root" };
            var hasEnemy = enemyWords.Any(word => description.ToLowerInvariant().Contains(word));
            if (hasEnemy) score += 20;

            // No generic phrases: +10
            var genericPhrases = new[] { "some", "thing", "powerful", "many" };
            var hasGeneric = genericPhrases.Any(phrase => description.ToLowerInvariant().Contains(phrase));
            if (!hasGeneric) score += 10;

            return Math.Clamp(score, 0, 100);
        }

        /// <summary>
        /// Calculates uniqueness score (0-100) based on how many titles/descriptions are duplicates.
        /// Lower duplicates = higher score.
        /// </summary>
        private static int CalculateUniquenessScore(List<string> titles, List<string> descriptions)
        {
            if (titles.Count == 0 && descriptions.Count == 0) return 0;

            var titleDuplicates = titles.GroupBy(t => t.ToLowerInvariant().Trim()).Count(g => g.Count() > 1);
            var descDuplicates = descriptions.GroupBy(d => d.ToLowerInvariant().Trim()).Count(g => g.Count() > 1);

            var totalItems = titles.Count + descriptions.Count;
            var totalDuplicates = titleDuplicates + descDuplicates;

            // Score: 100 - (duplicates * penalty), clamped
            var penalty = totalItems > 0 ? (totalDuplicates * 100.0 / totalItems) : 0;
            return Math.Clamp(100 - (int)penalty, 0, 100);
        }

        /// <summary>
        /// Calculates title presence score (0-100) based on percentage of entities with non-generic titles.
        /// </summary>
        private static int CalculateTitlePresenceScore(List<string> titles)
        {
            if (titles.Count == 0) return 0;

            var goodTitles = titles.Count(t =>
                !string.IsNullOrWhiteSpace(t) &&
                t.Length > 3 &&
                !t.ToLowerInvariant().Contains("room") &&
                !t.ToLowerInvariant().Contains("npc") &&
                !t.ToLowerInvariant().StartsWith("room ") &&
                !t.ToLowerInvariant().StartsWith("npc "));

            return (int)((goodTitles * 100.0) / titles.Count);
        }

        /// <summary>
        /// Validate a world zip file with lightweight checks and persist a validation JSON
        /// next to the zip (zipPath + ".validation.json"). Returns the computed result.
        /// Accepts optional progress reporter for UI updates.
        /// </summary>
        public async Task<WorldValidationResult> ValidateWorldFileAsync(string zipPath, IProgress<string>? progress = null)
        {
            progress?.Report("5% - Starting validation");

            if (string.IsNullOrEmpty(zipPath) || !File.Exists(zipPath))
                throw new FileNotFoundException("World file not found", zipPath);

            var result = new WorldValidationResult
            {
                WorldPath = zipPath,
                ValidatedAt = DateTime.UtcNow
            };

            try
            {
                progress?.Report("20% - Opening package");
                using var zip = ZipFile.OpenRead(zipPath);

                // Read counts and optionally room/npc details
                progress?.Report("35% - Scanning contents");
                var roomEntries = zip.Entries.Where(e => e.FullName.StartsWith("rooms/") && e.FullName.EndsWith(".json")).ToList();
                var npcEntries = zip.Entries.Where(e => e.FullName.StartsWith("npcs/") && e.FullName.EndsWith(".json")).ToList();
                var factionEntries = zip.Entries.Where(e => e.FullName.StartsWith("factions/") && e.FullName.EndsWith(".json")).ToList();

                result.RoomScore = roomEntries.Count > 0 ? 100 : 0;
                result.NpcScore = npcEntries.Count > 0 ? 100 : 0;
                result.FactionScore = factionEntries.Count > 0 ? 100 : 0;

                // Parse room JSONs to check connectivity and npc references
                progress?.Report("50% - Analyzing rooms and NPC references");
                var roomIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                var npcIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                var roomExitsMissing = 0;
                var roomWithNoExits = 0;
                var missingNpcRefs = 0;

                // collect npc ids
                foreach (var nEntry in npcEntries)
                {
                    try
                    {
                        using var s = nEntry.Open();
                        using var sr = new StreamReader(s);
                        var json = await sr.ReadToEndAsync();
                        using var doc = JsonDocument.Parse(json);
                        if (doc.RootElement.TryGetProperty("Id", out var idProp) && idProp.ValueKind == JsonValueKind.String)
                        {
                            npcIds.Add(idProp.GetString() ?? "");
                        }
                    }
                    catch { }
                }

                // parse rooms
                foreach (var rEntry in roomEntries)
                {
                    try
                    {
                        using var s = rEntry.Open();
                        using var sr = new StreamReader(s);
                        var json = await sr.ReadToEndAsync();
                        using var doc = JsonDocument.Parse(json);

                        if (doc.RootElement.TryGetProperty("Id", out var idProp) && idProp.ValueKind == JsonValueKind.String)
                        {
                            var rid = idProp.GetString() ?? "";
                            roomIds.Add(rid);
                        }

                        // Exits
                        if (doc.RootElement.TryGetProperty("Exits", out var exitsProp) && exitsProp.ValueKind == JsonValueKind.Object)
                        {
                            if (exitsProp.EnumerateObject().Any() == false)
                            {
                                roomWithNoExits++;
                            }
                        }
                        else
                        {
                            roomWithNoExits++;
                        }

                        // Npcs referenced
                        if (doc.RootElement.TryGetProperty("Npcs", out var npcsProp) && npcsProp.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var el in npcsProp.EnumerateArray())
                            {
                                if (el.ValueKind == JsonValueKind.String)
                                {
                                    var nid = el.GetString() ?? "";
                                    if (!npcIds.Contains(nid)) missingNpcRefs++;
                                }
                            }
                        }
                    }
                    catch { }
                }

                // Basic consistency scoring
                progress?.Report("75% - Scoring consistency");
                var score = 100;
                if (roomWithNoExits > 0)
                {
                    result.Warnings.Add($"Found {roomWithNoExits} room(s) with no exits");
                    score -= Math.Min(40, roomWithNoExits * 10);
                }

                if (missingNpcRefs > 0)
                {
                    result.Warnings.Add($"Found {missingNpcRefs} NPC reference(s) pointing to missing NPCs");
                    score -= Math.Min(30, missingNpcRefs * 5);
                }

                if (roomEntries.Count == 0)
                {
                    result.Errors.Add("No rooms found in world package");
                    score = 0;
                }

                if (npcEntries.Count == 0)
                {
                    result.Warnings.Add("No NPCs found in world package");
                    score -= 20;
                }

                if (factionEntries.Count == 0)
                {
                    result.Warnings.Add("No factions found in world package");
                    score -= 10;
                }

                result.ConsistencyScore = Math.Clamp(score, 0, 100);

                // Overall average of available metrics
                var metrics = new List<int> { result.RoomScore, result.NpcScore, result.FactionScore, result.ConsistencyScore };
                result.OverallScore = (int)Math.Round(metrics.Average());

                // set readable world name if possible
                progress?.Report("90% - Finalizing");
                var worldEntry = zip.GetEntry("world.json");
                if (worldEntry != null)
                {
                    try
                    {
                        using var s = worldEntry.Open();
                        using var sr = new StreamReader(s);
                        var json = await sr.ReadToEndAsync();
                        using var doc = JsonDocument.Parse(json);
                        if (doc.RootElement.TryGetProperty("Name", out var nameProp) && nameProp.ValueKind == JsonValueKind.String)
                        {
                            result.WorldName = nameProp.GetString() ?? Path.GetFileNameWithoutExtension(zipPath);
                        }
                    }
                    catch { }
                }

                if (string.IsNullOrEmpty(result.WorldName)) result.WorldName = Path.GetFileNameWithoutExtension(zipPath);

                // persist to validation json
                var validationPath = zipPath + ".validation.json";
                var opts = new JsonSerializerOptions { WriteIndented = true };
                var jsonOut = JsonSerializer.Serialize(result, opts);
                await File.WriteAllTextAsync(validationPath, jsonOut);

                progress?.Report("100% - Validation complete");
                return result;
            }
            catch (Exception ex)
            {
                result.Errors.Add("Validation exception: " + ex.Message);
                result.OverallScore = 0;
                var validationPath = zipPath + ".validation.json";
                var opts = new JsonSerializerOptions { WriteIndented = true };
                var jsonOut = JsonSerializer.Serialize(result, opts);
                await File.WriteAllTextAsync(validationPath, jsonOut);
                progress?.Report($"100% - Validation failed: {ex.Message}");
                return result;
            }
        }

        /// <summary>
        /// Enhanced validation with content quality analysis.
        /// Analyzes descriptions for quality metrics and updates scores accordingly.
        /// </summary>
        public async Task<WorldValidationResult> ValidateWorldFileWithQualityAsync(string zipPath, IProgress<string>? progress = null)
        {
            var result = await ValidateWorldFileAsync(zipPath, progress);

            if (result.OverallScore == 0) return result; // Skip quality if basic validation failed

            progress?.Report("60% - Analyzing content quality");

            try
            {
                using var zip = ZipFile.OpenRead(zipPath);

                var roomEntries = zip.Entries.Where(e => e.FullName.StartsWith("rooms/") && e.FullName.EndsWith(".json")).ToList();
                var npcEntries = zip.Entries.Where(e => e.FullName.StartsWith("npcs/") && e.FullName.EndsWith(".json")).ToList();
                var factionEntries = zip.Entries.Where(e => e.FullName.StartsWith("factions/") && e.FullName.EndsWith(".json")).ToList();

                // Collect data for uniqueness and title checks
                var allTitles = new List<string>();
                var allDescriptions = new List<string>();

                // Analyze room quality
                var roomScores = new List<int>();
                foreach (var rEntry in roomEntries)
                {
                    try
                    {
                        using var s = rEntry.Open();
                        using var sr = new StreamReader(s);
                        var json = await sr.ReadToEndAsync();
                        using var doc = JsonDocument.Parse(json);
                        var title = "";
                        var desc = "";
                        if (doc.RootElement.TryGetProperty("Title", out var titleProp) && titleProp.ValueKind == JsonValueKind.String)
                        {
                            title = titleProp.GetString() ?? "";
                        }
                        if (doc.RootElement.TryGetProperty("BaseDescription", out var descProp) && descProp.ValueKind == JsonValueKind.String)
                        {
                            desc = descProp.GetString() ?? "";
                            // Remove AI-generated indicator for scoring
                            desc = desc.Replace("[AI-generated content]", "").Trim();
                            roomScores.Add(ScoreRoomDescription(desc));
                        }
                        allTitles.Add(title);
                        allDescriptions.Add(desc);
                    }
                    catch { }
                }
                result.RoomScore = roomScores.Any() ? (int)roomScores.Average() : 0;

                // Analyze NPC quality
                var npcScores = new List<int>();
                foreach (var nEntry in npcEntries)
                {
                    try
                    {
                        using var s = nEntry.Open();
                        using var sr = new StreamReader(s);
                        var json = await sr.ReadToEndAsync();
                        using var doc = JsonDocument.Parse(json);
                        var title = "";
                        var desc = "";
                        if (doc.RootElement.TryGetProperty("Name", out var titleProp) && titleProp.ValueKind == JsonValueKind.String)
                        {
                            title = titleProp.GetString() ?? "";
                        }
                        if (doc.RootElement.TryGetProperty("Description", out var descProp) && descProp.ValueKind == JsonValueKind.String)
                        {
                            desc = descProp.GetString() ?? "";
                            // Remove AI-generated indicator for scoring
                            desc = desc.Replace("[AI-generated content]", "").Trim();
                            npcScores.Add(ScoreNpcBio(desc));
                        }
                        allTitles.Add(title);
                        allDescriptions.Add(desc);
                    }
                    catch { }
                }
                result.NpcScore = npcScores.Any() ? (int)npcScores.Average() : 0;

                // Analyze faction quality
                var factionScores = new List<int>();
                foreach (var fEntry in factionEntries)
                {
                    try
                    {
                        using var s = fEntry.Open();
                        using var sr = new StreamReader(s);
                        var json = await sr.ReadToEndAsync();
                        using var doc = JsonDocument.Parse(json);
                        var title = "";
                        var desc = "";
                        if (doc.RootElement.TryGetProperty("Name", out var titleProp) && titleProp.ValueKind == JsonValueKind.String)
                        {
                            title = titleProp.GetString() ?? "";
                        }
                        if (doc.RootElement.TryGetProperty("Description", out var descProp) && descProp.ValueKind == JsonValueKind.String)
                        {
                            desc = descProp.GetString() ?? "";
                            // Remove AI-generated indicator for scoring
                            desc = desc.Replace("[AI-generated content]", "").Trim();
                            factionScores.Add(ScoreFactionDescription(desc));
                        }
                        allTitles.Add(title);
                        allDescriptions.Add(desc);
                    }
                    catch { }
                }
                result.FactionScore = factionScores.Any() ? (int)factionScores.Average() : 0;

                // Calculate new batch metrics
                result.UniquenessScore = CalculateUniquenessScore(allTitles, allDescriptions);
                result.TitlePresenceScore = CalculateTitlePresenceScore(allTitles);

                // Add batch warnings/errors
                if (result.UniquenessScore < 50)
                {
                    result.BatchWarnings.Add($"Low uniqueness score ({result.UniquenessScore}): Many duplicate titles or descriptions detected.");
                }
                if (result.TitlePresenceScore < 80)
                {
                    result.BatchWarnings.Add($"Low title presence ({result.TitlePresenceScore}%): Some entities have generic or missing titles.");
                }

                // Recalculate overall score with quality metrics
                var metrics = new List<int> { result.RoomScore, result.NpcScore, result.FactionScore, result.ConsistencyScore, result.UniquenessScore, result.TitlePresenceScore };
                result.OverallScore = (int)Math.Round(metrics.Average());

                // Persist updated result
                var validationPath = zipPath + ".validation.json";
                var opts = new JsonSerializerOptions { WriteIndented = true };
                var jsonOut = JsonSerializer.Serialize(result, opts);
                await File.WriteAllTextAsync(validationPath, jsonOut);

                progress?.Report("85% - Quality analysis complete");
            }
            catch (Exception ex)
            {
                result.Warnings.Add("Quality analysis failed: " + ex.Message);
            }

            return result;
        }

        /// <summary>
        /// Batch validates all world ZIP files in a directory.
        /// Returns a list of validation results and summary statistics.
        /// </summary>
        public async Task<List<WorldValidationResult>> BatchValidateWorldsAsync(string worldsDirectory, IProgress<string>? progress = null)
        {
            var results = new List<WorldValidationResult>();
            if (!Directory.Exists(worldsDirectory))
            {
                progress?.Report("Directory not found: " + worldsDirectory);
                return results;
            }

            var zipFiles = Directory.GetFiles(worldsDirectory, "*.zip");
            progress?.Report($"Found {zipFiles.Length} world files to validate");

            for (int i = 0; i < zipFiles.Length; i++)
            {
                var zipPath = zipFiles[i];
                progress?.Report($"Validating {i + 1}/{zipFiles.Length}: {Path.GetFileName(zipPath)}");

                try
                {
                    var result = await ValidateWorldFileWithQualityAsync(zipPath);
                    results.Add(result);
                }
                catch (Exception ex)
                {
                    var errorResult = new WorldValidationResult
                    {
                        WorldPath = zipPath,
                        WorldName = Path.GetFileNameWithoutExtension(zipPath),
                        Errors = new List<string> { $"Validation failed: {ex.Message}" },
                        OverallScore = 0
                    };
                    results.Add(errorResult);
                }
            }

            progress?.Report($"Batch validation complete: {results.Count} worlds processed");

            // Summary statistics
            var validResults = results.Where(r => r.Errors.Count == 0).ToList();
            if (validResults.Any())
            {
                var avgOverall = validResults.Average(r => r.OverallScore);
                var avgUniqueness = validResults.Average(r => r.UniquenessScore);
                var avgTitlePresence = validResults.Average(r => r.TitlePresenceScore);
                progress?.Report($"Summary: Avg Overall Score: {avgOverall:F1}, Avg Uniqueness: {avgUniqueness:F1}, Avg Title Presence: {avgTitlePresence:F1}%");
            }

            return results;
        }
    }
}
