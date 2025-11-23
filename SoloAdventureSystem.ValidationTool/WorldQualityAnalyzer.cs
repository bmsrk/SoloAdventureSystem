using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SoloAdventureSystem.ContentGenerator.Adapters;
using SoloAdventureSystem.ContentGenerator.Configuration;
using SoloAdventureSystem.ContentGenerator.EmbeddedModel;
using SoloAdventureSystem.ContentGenerator.Models;

namespace SoloAdventureSystem.ValidationTool;

/// <summary>
/// Analyzes already generated worlds to validate quality of AI-generated content.
/// Tests rooms, NPCs, and factions for quality metrics.
/// </summary>
public class WorldQualityAnalyzer
{
    private readonly ILogger? _logger;

    public WorldQualityAnalyzer(ILogger? logger = null)
    {
        _logger = logger;
    }

    /// <summary>
    /// Analyze a world ZIP file
    /// </summary>
    public WorldAnalysisResult AnalyzeWorld(string worldZipPath)
    {
        if (!File.Exists(worldZipPath))
        {
            throw new FileNotFoundException($"World file not found: {worldZipPath}");
        }

        Console.WriteLine($"\n?? Analyzing: {Path.GetFileName(worldZipPath)}");
        Console.WriteLine("??????????????????????????????????????????????????????????");

        var result = new WorldAnalysisResult
        {
            WorldPath = worldZipPath,
            WorldName = Path.GetFileNameWithoutExtension(worldZipPath)
        };

        try
        {
            // Extract to temp directory
            var tempDir = Path.Combine(Path.GetTempPath(), $"WorldAnalysis_{Guid.NewGuid()}");
            ZipFile.ExtractToDirectory(worldZipPath, tempDir);

            try
            {
                // Analyze world.json
                var worldJsonPath = Path.Combine(tempDir, "world.json");
                if (File.Exists(worldJsonPath))
                {
                    var worldJson = File.ReadAllText(worldJsonPath);
                    var world = JsonSerializer.Deserialize<WorldJsonModel>(worldJson);
                    
                    if (world != null)
                    {
                        result.WorldDescription = world.Description;
                        Console.WriteLine($"\n?? World: {world.Name}");
                        Console.WriteLine($"   Description: {world.Description}");
                        Console.WriteLine($"   Created: {world.CreatedAt:yyyy-MM-dd HH:mm}");
                    }
                }

                // Analyze rooms
                var roomsDir = Path.Combine(tempDir, "rooms");
                if (Directory.Exists(roomsDir))
                {
                    var roomFiles = Directory.GetFiles(roomsDir, "*.json");
                    Console.WriteLine($"\n?? Analyzing {roomFiles.Length} rooms...");
                    
                    foreach (var roomFile in roomFiles)
                    {
                        var roomJson = File.ReadAllText(roomFile);
                        var room = JsonSerializer.Deserialize<RoomModel>(roomJson);
                        
                        if (room != null)
                        {
                            var quality = AnalyzeRoomQuality(room);
                            result.RoomQualities.Add(quality);
                        }
                    }
                    
                    PrintRoomSummary(result.RoomQualities);
                }

                // Analyze NPCs
                var npcsDir = Path.Combine(tempDir, "npcs");
                if (Directory.Exists(npcsDir))
                {
                    var npcFiles = Directory.GetFiles(npcsDir, "*.json");
                    Console.WriteLine($"\n?? Analyzing {npcFiles.Length} NPCs...");
                    
                    foreach (var npcFile in npcFiles)
                    {
                        var npcJson = File.ReadAllText(npcFile);
                        var npc = JsonSerializer.Deserialize<NpcModel>(npcJson);
                        
                        if (npc != null)
                        {
                            var quality = AnalyzeNpcQuality(npc);
                            result.NpcQualities.Add(quality);
                        }
                    }
                    
                    PrintNpcSummary(result.NpcQualities);
                }

                // Analyze factions
                var factionsDir = Path.Combine(tempDir, "factions");
                if (Directory.Exists(factionsDir))
                {
                    var factionFiles = Directory.GetFiles(factionsDir, "*.json");
                    Console.WriteLine($"\n??  Analyzing {factionFiles.Length} factions...");
                    
                    foreach (var factionFile in factionFiles)
                    {
                        var factionJson = File.ReadAllText(factionFile);
                        var faction = JsonSerializer.Deserialize<FactionModel>(factionJson);
                        
                        if (faction != null)
                        {
                            var quality = AnalyzeFactionQuality(faction);
                            result.FactionQualities.Add(quality);
                        }
                    }
                    
                    PrintFactionSummary(result.FactionQualities);
                }

                // Calculate overall score
                result.CalculateOverallScore();
                
                // Print final summary
                PrintFinalSummary(result);
            }
            finally
            {
                // Cleanup
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, true);
                }
            }
        }
        catch (Exception ex)
        {
            result.Error = ex.Message;
            Console.WriteLine($"\n? Error analyzing world: {ex.Message}");
        }

        return result;
    }

    private ContentQuality AnalyzeRoomQuality(RoomModel room)
    {
        var quality = new ContentQuality
        {
            Name = room.Title,
            Content = room.BaseDescription,
            Type = "Room"
        };

        var desc = room.BaseDescription ?? "";

        // Length check
        quality.Length = desc.Length;
        quality.Checks["Length"] = desc.Length >= 100 && desc.Length <= 500;

        // Sentence count (approximate)
        var sentences = desc.Split('.', '!', '?').Where(s => !string.IsNullOrWhiteSpace(s)).Count();
        quality.SentenceCount = sentences;
        quality.Checks["SentenceCount"] = sentences >= 2 && sentences <= 5;

        // Check for vague language
        var vague = desc.Contains("some ", StringComparison.OrdinalIgnoreCase) || 
                    desc.Contains("maybe ", StringComparison.OrdinalIgnoreCase) || 
                    desc.Contains("things ", StringComparison.OrdinalIgnoreCase) ||
                    desc.Contains("stuff ", StringComparison.OrdinalIgnoreCase);
        quality.Checks["NoVague"] = !vague;

        // Check for specific details (colors, materials)
        var hasColors = desc.Contains("blue", StringComparison.OrdinalIgnoreCase) || 
                       desc.Contains("red", StringComparison.OrdinalIgnoreCase) || 
                       desc.Contains("neon", StringComparison.OrdinalIgnoreCase) || 
                       desc.Contains("green", StringComparison.OrdinalIgnoreCase) || 
                       desc.Contains("white", StringComparison.OrdinalIgnoreCase) || 
                       desc.Contains("black", StringComparison.OrdinalIgnoreCase) ||
                       desc.Contains("silver", StringComparison.OrdinalIgnoreCase) || 
                       desc.Contains("gold", StringComparison.OrdinalIgnoreCase) || 
                       desc.Contains("amber", StringComparison.OrdinalIgnoreCase);
        quality.Checks["HasColors"] = hasColors;

        // Check for sensory details
        var hasSensory = desc.Contains("smell", StringComparison.OrdinalIgnoreCase) || 
                        desc.Contains("taste", StringComparison.OrdinalIgnoreCase) ||
                        desc.Contains("sound", StringComparison.OrdinalIgnoreCase) || 
                        desc.Contains("hum", StringComparison.OrdinalIgnoreCase) ||
                        desc.Contains("flicker", StringComparison.OrdinalIgnoreCase) || 
                        desc.Contains("glow", StringComparison.OrdinalIgnoreCase);
        quality.Checks["HasSensory"] = hasSensory;

        // Not empty
        quality.Checks["NotEmpty"] = !string.IsNullOrWhiteSpace(desc);

        quality.CalculateScore();
        return quality;
    }

    private ContentQuality AnalyzeNpcQuality(NpcModel npc)
    {
        var quality = new ContentQuality
        {
            Name = npc.Name,
            Content = npc.Description,
            Type = "NPC"
        };

        var desc = npc.Description ?? "";

        // Length check
        quality.Length = desc.Length;
        quality.Checks["Length"] = desc.Length >= 80 && desc.Length <= 400;

        // Sentence count
        var sentences = desc.Split('.', '!', '?').Where(s => !string.IsNullOrWhiteSpace(s)).Count();
        quality.SentenceCount = sentences;
        quality.Checks["SentenceCount"] = sentences >= 1 && sentences <= 4;

        // Check for vague language
        var vague = desc.Contains("some ", StringComparison.OrdinalIgnoreCase) || 
                    desc.Contains("maybe ", StringComparison.OrdinalIgnoreCase) || 
                    desc.Contains("things ", StringComparison.OrdinalIgnoreCase);
        quality.Checks["NoVague"] = !vague;

        // Check for character trait/quirk indicators
        var hasTrait = desc.Contains("secret", StringComparison.OrdinalIgnoreCase) || 
                      desc.Contains("trait", StringComparison.OrdinalIgnoreCase) ||
                      desc.Contains("quirk", StringComparison.OrdinalIgnoreCase) || 
                      desc.Contains("implant", StringComparison.OrdinalIgnoreCase) ||
                      desc.Contains("scar", StringComparison.OrdinalIgnoreCase) || 
                      desc.Contains("eye", StringComparison.OrdinalIgnoreCase) || 
                      desc.Contains("tattoo", StringComparison.OrdinalIgnoreCase);
        quality.Checks["HasTrait"] = hasTrait;

        // Check for role/background
        var hasRole = desc.Length > 50; // Simple heuristic
        quality.Checks["HasRole"] = hasRole;

        // Not empty
        quality.Checks["NotEmpty"] = !string.IsNullOrWhiteSpace(desc);

        quality.CalculateScore();
        return quality;
    }

    private ContentQuality AnalyzeFactionQuality(FactionModel faction)
    {
        var quality = new ContentQuality
        {
            Name = faction.Name,
            Content = faction.Description,
            Type = "Faction"
        };

        var desc = faction.Description ?? "";

        // Length check
        quality.Length = desc.Length;
        quality.Checks["Length"] = desc.Length >= 100 && desc.Length <= 500;

        // Sentence count
        var sentences = desc.Split('.', '!', '?').Where(s => !string.IsNullOrWhiteSpace(s)).Count();
        quality.SentenceCount = sentences;
        quality.Checks["SentenceCount"] = sentences >= 2 && sentences <= 5;

        // Check for vague language
        var vague = desc.Contains("some ", StringComparison.OrdinalIgnoreCase) || 
                    desc.Contains("maybe ", StringComparison.OrdinalIgnoreCase) || 
                    desc.Contains("things ", StringComparison.OrdinalIgnoreCase);
        quality.Checks["NoVague"] = !vague;

        // Check for goals/ideology
        var hasGoals = desc.Contains("fight", StringComparison.OrdinalIgnoreCase) || 
                      desc.Contains("control", StringComparison.OrdinalIgnoreCase) ||
                      desc.Contains("rule", StringComparison.OrdinalIgnoreCase) || 
                      desc.Contains("free", StringComparison.OrdinalIgnoreCase) ||
                      desc.Contains("protect", StringComparison.OrdinalIgnoreCase) || 
                      desc.Contains("champion", StringComparison.OrdinalIgnoreCase);
        quality.Checks["HasGoals"] = hasGoals;

        // Check for conflict/enemies
        var hasConflict = desc.Contains("enemy", StringComparison.OrdinalIgnoreCase) || 
                         desc.Contains("fight", StringComparison.OrdinalIgnoreCase) ||
                         desc.Contains("war", StringComparison.OrdinalIgnoreCase) || 
                         desc.Contains("against", StringComparison.OrdinalIgnoreCase) ||
                         desc.Contains("hunt", StringComparison.OrdinalIgnoreCase) || 
                         desc.Contains("battle", StringComparison.OrdinalIgnoreCase);
        quality.Checks["HasConflict"] = hasConflict;

        // Not empty
        quality.Checks["NotEmpty"] = !string.IsNullOrWhiteSpace(desc);

        quality.CalculateScore();
        return quality;
    }

    private void PrintRoomSummary(List<ContentQuality> rooms)
    {
        if (rooms.Count == 0) return;

        var avgScore = rooms.Average(r => r.Score);
        var avgLength = (int)rooms.Average(r => r.Length);
        var avgSentences = rooms.Average(r => r.SentenceCount);

        Console.WriteLine($"\n   Average Score: {avgScore:F1}/100");
        Console.WriteLine($"   Average Length: {avgLength} chars");
        Console.WriteLine($"   Average Sentences: {avgSentences:F1}");
        
        var passed = rooms.Count(r => r.Score >= 70);
        Console.WriteLine($"   Quality: {passed}/{rooms.Count} passed (?70 score)");

        // Show worst room
        var worst = rooms.OrderBy(r => r.Score).First();
        if (worst.Score < 70)
        {
            Console.WriteLine($"\n   ??  Lowest: {worst.Name} ({worst.Score:F0}/100)");
            Console.WriteLine($"      Issues: {string.Join(", ", worst.Checks.Where(c => !c.Value).Select(c => c.Key))}");
        }
    }

    private void PrintNpcSummary(List<ContentQuality> npcs)
    {
        if (npcs.Count == 0) return;

        var avgScore = npcs.Average(n => n.Score);
        var avgLength = (int)npcs.Average(n => n.Length);
        var avgSentences = npcs.Average(n => n.SentenceCount);

        Console.WriteLine($"\n   Average Score: {avgScore:F1}/100");
        Console.WriteLine($"   Average Length: {avgLength} chars");
        Console.WriteLine($"   Average Sentences: {avgSentences:F1}");
        
        var passed = npcs.Count(n => n.Score >= 70);
        Console.WriteLine($"   Quality: {passed}/{npcs.Count} passed (?70 score)");

        var worst = npcs.OrderBy(n => n.Score).First();
        if (worst.Score < 70)
        {
            Console.WriteLine($"\n   ??  Lowest: {worst.Name} ({worst.Score:F0}/100)");
            Console.WriteLine($"      Issues: {string.Join(", ", worst.Checks.Where(c => !c.Value).Select(c => c.Key))}");
        }
    }

    private void PrintFactionSummary(List<ContentQuality> factions)
    {
        if (factions.Count == 0) return;

        var avgScore = factions.Average(f => f.Score);
        var avgLength = (int)factions.Average(f => f.Length);
        var avgSentences = factions.Average(f => f.SentenceCount);

        Console.WriteLine($"\n   Average Score: {avgScore:F1}/100");
        Console.WriteLine($"   Average Length: {avgLength} chars");
        Console.WriteLine($"   Average Sentences: {avgSentences:F1}");
        
        var passed = factions.Count(f => f.Score >= 70);
        Console.WriteLine($"   Quality: {passed}/{factions.Count} passed (?70 score)");

        var worst = factions.OrderBy(f => f.Score).First();
        if (worst.Score < 70)
        {
            Console.WriteLine($"\n   ??  Lowest: {worst.Name} ({worst.Score:F0}/100)");
            Console.WriteLine($"      Issues: {string.Join(", ", worst.Checks.Where(c => !c.Value).Select(c => c.Key))}");
        }
    }

    private void PrintFinalSummary(WorldAnalysisResult result)
    {
        Console.WriteLine("\n????????????????????????????????????????????????????????????");
        Console.WriteLine("? Overall Quality Summary                                  ?");
        Console.WriteLine("????????????????????????????????????????????????????????????");
        
        Console.WriteLine($"\n?? Overall Score: {result.OverallScore:F1}/100");
        Console.WriteLine($"   Rooms:    {result.RoomScore:F1}/100 ({result.RoomQualities.Count} items)");
        Console.WriteLine($"   NPCs:     {result.NpcScore:F1}/100 ({result.NpcQualities.Count} items)");
        Console.WriteLine($"   Factions: {result.FactionScore:F1}/100 ({result.FactionQualities.Count} items)");

        var grade = result.OverallScore switch
        {
            >= 90 => "A (Excellent)",
            >= 80 => "B (Good)",
            >= 70 => "C (Acceptable)",
            >= 60 => "D (Poor)",
            _ => "F (Failed)"
        };

        Console.WriteLine($"\n   Grade: {grade}");

        if (result.OverallScore >= 80)
        {
            Console.WriteLine("\n   ? This world meets high quality standards!");
        }
        else if (result.OverallScore >= 70)
        {
            Console.WriteLine("\n   ? This world meets acceptable quality standards");
        }
        else
        {
            Console.WriteLine("\n   ??  This world has quality issues - consider regenerating");
        }
    }
}

/// <summary>
/// Quality analysis result for a world
/// </summary>
public class WorldAnalysisResult
{
    public string WorldPath { get; set; } = "";
    public string WorldName { get; set; } = "";
    public string? WorldDescription { get; set; }
    public List<ContentQuality> RoomQualities { get; set; } = new();
    public List<ContentQuality> NpcQualities { get; set; } = new();
    public List<ContentQuality> FactionQualities { get; set; } = new();
    public float RoomScore { get; set; }
    public float NpcScore { get; set; }
    public float FactionScore { get; set; }
    public float OverallScore { get; set; }
    public string? Error { get; set; }

    public void CalculateOverallScore()
    {
        RoomScore = RoomQualities.Count > 0 ? RoomQualities.Average(r => r.Score) : 0;
        NpcScore = NpcQualities.Count > 0 ? NpcQualities.Average(n => n.Score) : 0;
        FactionScore = FactionQualities.Count > 0 ? FactionQualities.Average(f => f.Score) : 0;

        var scores = new List<float>();
        if (RoomQualities.Count > 0) scores.Add(RoomScore);
        if (NpcQualities.Count > 0) scores.Add(NpcScore);
        if (FactionQualities.Count > 0) scores.Add(FactionScore);

        OverallScore = scores.Count > 0 ? scores.Average() : 0;
    }
}

/// <summary>
/// Quality metrics for a piece of content
/// </summary>
public class ContentQuality
{
    public string Name { get; set; } = "";
    public string Content { get; set; } = "";
    public string Type { get; set; } = "";
    public int Length { get; set; }
    public int SentenceCount { get; set; }
    public Dictionary<string, bool> Checks { get; set; } = new();
    public float Score { get; set; }

    public void CalculateScore()
    {
        if (Checks.Count == 0)
        {
            Score = 0;
            return;
        }

        var passed = Checks.Count(c => c.Value);
        Score = (passed / (float)Checks.Count) * 100;
    }
}
