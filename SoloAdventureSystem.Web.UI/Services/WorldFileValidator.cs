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
    }

    public class WorldFileValidator
    {
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
    }
}
