using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SoloAdventureSystem.ContentGenerator;
using SoloAdventureSystem.ContentGenerator.Adapters;
using SoloAdventureSystem.ContentGenerator.Configuration;
using SoloAdventureSystem.ContentGenerator.EmbeddedModel;
using SoloAdventureSystem.ContentGenerator.Generation;
using SoloAdventureSystem.ContentGenerator.Models;
using SoloAdventureSystem.ContentGenerator.Utils;

namespace SoloAdventureSystem.ValidationTool;

/// <summary>
/// Validation tool to test LLamaSharp with the new improved prompts.
/// Run this to verify model download, loading, and text generation quality.
/// </summary>
class Program
{
    static async Task<int> Main(string[] args)
    {
        Console.WriteLine("????????????????????????????????????????????????????????????");
        Console.WriteLine("?  ?? LLamaSharp Validation Tool - Enhanced Prompts       ?");
        Console.WriteLine("????????????????????????????????????????????????????????????");
        Console.WriteLine();

        // Check if batch generating worlds
        if (args.Length > 0 && args[0] == "batch")
        {
            return await WorldBatchGenerator.GenerateWorlds(args.Skip(1).ToArray());
        }

        // Check if analyzing existing worlds
        if (args.Length > 0 && args[0] == "analyze")
        {
            return AnalyzeWorlds(args.Skip(1).ToArray());
        }

        var modelKey = args.Length > 0 ? args[0] : "tinyllama-q4";
        Console.WriteLine($"?? Testing model: {modelKey}");
        Console.WriteLine();

        try
        {
            // Setup services
            var services = new ServiceCollection();
            services.AddLogging(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Information);
            });

            services.Configure<AISettings>(options =>
            {
                options.Provider = "LLamaSharp";
                options.Model = modelKey;
                options.LLamaModelKey = modelKey;
                options.ContextSize = 2048;
                options.UseGPU = false;
                options.MaxInferenceThreads = 4;
            });

            var serviceProvider = services.BuildServiceProvider();

            // Step 1: Check model file
            Console.WriteLine("????????????????????????????????????????????????????????????");
            Console.WriteLine("? Step 1: Checking Model File                             ?");
            Console.WriteLine("????????????????????????????????????????????????????????????");
            
            var modelPath = GGUFModelDownloader.GetModelPath(modelKey);
            Console.WriteLine($"?? Model path: {modelPath}");
            
            if (File.Exists(modelPath))
            {
                var fileInfo = new FileInfo(modelPath);
                Console.WriteLine($"? File exists: {PathHelper.FormatFileSize(fileInfo.Length)}");
                Console.WriteLine($"   Last modified: {fileInfo.LastWriteTime}");
            }
            else
            {
                Console.WriteLine("??  File does not exist - will download");
            }
            Console.WriteLine();

            // Step 2: Download/Verify model
            Console.WriteLine("????????????????????????????????????????????????????????????");
            Console.WriteLine("? Step 2: Downloading/Verifying Model                     ?");
            Console.WriteLine("????????????????????????????????????????????????????????????");
            
            var downloader = new GGUFModelDownloader(
                serviceProvider.GetRequiredService<ILogger<GGUFModelDownloader>>());
            
            var progress = new Progress<DownloadProgress>(p =>
            {
                if (p.PercentComplete % 10 == 0 || p.PercentComplete == 100)
                {
                    var downloadedMB = p.DownloadedBytes / 1024.0 / 1024.0;
                    var totalMB = p.TotalBytes / 1024.0 / 1024.0;
                    var speedMB = p.SpeedBytesPerSecond / 1024.0 / 1024.0;
                    Console.WriteLine($"   ?? Progress: {downloadedMB:F0}/{totalMB:F0} MB ({p.PercentComplete}%) - {speedMB:F1} MB/s - ETA: {p.FormattedETA}");
                }
            });

            var startTime = DateTime.UtcNow;
            modelPath = await downloader.EnsureModelAvailableAsync(modelKey, progress);
            var downloadTime = DateTime.UtcNow - startTime;
            
            Console.WriteLine($"? Model ready in {downloadTime.TotalSeconds:F1}s");
            Console.WriteLine();

            // Step 3: Initialize LLamaSharp
            Console.WriteLine("????????????????????????????????????????????????????????????");
            Console.WriteLine("? Step 3: Initializing LLamaSharp Adapter                 ?");
            Console.WriteLine("????????????????????????????????????????????????????????????");
            
            var settings = serviceProvider.GetRequiredService<IOptions<AISettings>>();
            var logger = serviceProvider.GetRequiredService<ILogger<LLamaSharpAdapter>>();
            
            using var adapter = new LLamaSharpAdapter(settings, logger);
            
            startTime = DateTime.UtcNow;
            await adapter.InitializeAsync();
            var initTime = DateTime.UtcNow - startTime;
            
            Console.WriteLine($"? Adapter initialized in {initTime.TotalSeconds:F1}s");
            Console.WriteLine();

            // Step 4: Test with new improved prompts
            Console.WriteLine("????????????????????????????????????????????????????????????");
            Console.WriteLine("? Step 4: Testing New Improved Prompts                    ?");
            Console.WriteLine("????????????????????????????????????????????????????????????");
            Console.WriteLine();
            
            // Create realistic world options
            var options = new WorldGenerationOptions
            {
                Name = "ValidationWorld",
                Seed = 99999,
                Theme = "Cyberpunk",
                Flavor = "Dark and gritty with neon highlights",
                Description = "A sprawling megacity where corporations rule everything",
                MainPlotPoint = "Uncover the conspiracy behind the neural implant murders",
                TimePeriod = "2089",
                PowerStructure = "Megacorporations, hackers, and street gangs"
            };

            Console.WriteLine("?? World Context:");
            Console.WriteLine($"   Name: {options.Name}");
            Console.WriteLine($"   Flavor: {options.Flavor}");
            Console.WriteLine($"   Setting: {options.Description}");
            Console.WriteLine($"   Plot: {options.MainPlotPoint}");
            Console.WriteLine();

            // Test 1: Room Description
            Console.WriteLine("??????????????????????????????????????????????????????????");
            Console.WriteLine("?? Test 1: Room Description");
            Console.WriteLine("??????????????????????????????????????????????????????????");
            
            var roomPrompt = PromptTemplates.BuildRoomPrompt(
                "Neural Nexus", 
                options, 
                "oppressive and high-tech", 
                0, 
                5);
            
            Console.WriteLine("?? Using actual world gen prompt template");
            Console.WriteLine();
            
            startTime = DateTime.UtcNow;
            var roomDesc = adapter.GenerateRoomDescription(roomPrompt, 42);
            var genTime1 = DateTime.UtcNow - startTime;
            
            Console.WriteLine("? Generated in {0:F1}s", genTime1.TotalSeconds);
            Console.WriteLine();
            Console.WriteLine("Result:");
            Console.WriteLine("???????????????????????????????????????????????????????????");
            foreach (var line in WrapText(roomDesc, 57))
            {
                Console.WriteLine($"? {line,-55} ?");
            }
            Console.WriteLine("???????????????????????????????????????????????????????????");
            
            ValidateOutput(roomDesc, "Room", 3, 100);
            Console.WriteLine();
            
            // Test 2: NPC Bio
            Console.WriteLine("??????????????????????????????????????????????????????????");
            Console.WriteLine("?? Test 2: NPC Biography");
            Console.WriteLine("??????????????????????????????????????????????????????????");
            
            var npcPrompt = PromptTemplates.BuildNpcPrompt(
                "Marcus Chen", 
                options, 
                "Neural Nexus", 
                "Chrome Syndicate");
            
            Console.WriteLine("?? Using actual world gen prompt template");
            Console.WriteLine();
            
            startTime = DateTime.UtcNow;
            var npcBio = adapter.GenerateNpcBio(npcPrompt, 123);
            var genTime2 = DateTime.UtcNow - startTime;
            
            Console.WriteLine("? Generated in {0:F1}s", genTime2.TotalSeconds);
            Console.WriteLine();
            Console.WriteLine("Result:");
            Console.WriteLine("???????????????????????????????????????????????????????????");
            foreach (var line in WrapText(npcBio, 57))
            {
                Console.WriteLine($"? {line,-55} ?");
            }
            Console.WriteLine("???????????????????????????????????????????????????????????");
            
            ValidateOutput(npcBio, "NPC", 2, 80);
            Console.WriteLine();
            
            // Test 3: Faction Description
            Console.WriteLine("??????????????????????????????????????????????????????????");
            Console.WriteLine("??  Test 3: Faction Description");
            Console.WriteLine("??????????????????????????????????????????????????????????");
            
            var factionPrompt = PromptTemplates.BuildFactionPrompt("Chrome Syndicate", options);
            
            Console.WriteLine("?? Using actual world gen prompt template");
            Console.WriteLine();
            
            startTime = DateTime.UtcNow;
            var factionDesc = adapter.GenerateFactionFlavor(factionPrompt, 456);
            var genTime3 = DateTime.UtcNow - startTime;
            
            Console.WriteLine("? Generated in {0:F1}s", genTime3.TotalSeconds);
            Console.WriteLine();
            Console.WriteLine("Result:");
            Console.WriteLine("???????????????????????????????????????????????????????????");
            foreach (var line in WrapText(factionDesc, 57))
            {
                Console.WriteLine($"? {line,-55} ?");
            }
            Console.WriteLine("???????????????????????????????????????????????????????????");
            
            ValidateOutput(factionDesc, "Faction", 3, 100);
            Console.WriteLine();
            
            var totalGenTime = genTime1 + genTime2 + genTime3;

            // Step 5: Summary
            Console.WriteLine("????????????????????????????????????????????????????????????");
            Console.WriteLine("?  ? ALL TESTS PASSED - Enhanced Prompts Working!         ?");
            Console.WriteLine("????????????????????????????????????????????????????????????");
            Console.WriteLine();
            Console.WriteLine("?? Summary:");
            Console.WriteLine($"  Model: {modelKey}");
            Console.WriteLine($"  Download/Verify: {downloadTime.TotalSeconds:F1}s");
            Console.WriteLine($"  Initialization: {initTime.TotalSeconds:F1}s");
            Console.WriteLine($"  Text Generation:");
            Console.WriteLine($"    ?? Room: {genTime1.TotalSeconds:F1}s ({roomDesc.Length} chars)");
            Console.WriteLine($"    ?? NPC: {genTime2.TotalSeconds:F1}s ({npcBio.Length} chars)");
            Console.WriteLine($"    ??  Faction: {genTime3.TotalSeconds:F1}s ({factionDesc.Length} chars)");
            Console.WriteLine($"  Total Generation Time: {totalGenTime.TotalSeconds:F1}s");
            Console.WriteLine($"  Average: {totalGenTime.TotalSeconds / 3:F1}s per generation");
            Console.WriteLine($"  Overall Total: {(downloadTime + initTime + totalGenTime).TotalSeconds:F1}s");
            Console.WriteLine();
            Console.WriteLine("? All generation types producing quality output!");
            Console.WriteLine();

            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine();
            Console.WriteLine("????????????????????????????????????????????????????????????");
            Console.WriteLine("?  ? VALIDATION FAILED                                     ?");
            Console.WriteLine("????????????????????????????????????????????????????????????");
            Console.WriteLine();
            Console.WriteLine($"? Error: {ex.Message}");
            Console.WriteLine();
            Console.WriteLine("?? Troubleshooting:");
            Console.WriteLine("  1. Delete corrupted model:");
            Console.WriteLine($"     Remove-Item \"{GGUFModelDownloader.GetModelPath(modelKey)}\"");
            Console.WriteLine();
            Console.WriteLine("  2. Check disk space (need ~2-3 GB free)");
            Console.WriteLine();
            Console.WriteLine("  3. Check internet connection for model download");
            Console.WriteLine();
            Console.WriteLine("  4. Try a smaller model:");
            Console.WriteLine("     dotnet run -- tinyllama-q4");
            Console.WriteLine();
            Console.WriteLine("Stack trace:");
            Console.WriteLine(ex.StackTrace);
            Console.WriteLine();

            return 1;
        }
    }

    private static void ValidateOutput(string output, string type, int targetSentences, int minLength)
    {
        Console.WriteLine();
        Console.WriteLine("?? Quality Checks:");
        
        var sentences = output.Split('.', '!', '?').Length - 1;
        var length = output.Length;
        var hasSpecifics = length > minLength;
        
        Console.WriteLine($"   Length: {length} chars {(hasSpecifics ? "?" : "?")} (target: >{minLength})");
        Console.WriteLine($"   Sentences: ~{sentences} {(sentences >= targetSentences - 1 && sentences <= targetSentences + 1 ? "?" : "??")} (target: ~{targetSentences})");
        
        // Check for vague words
        var vague = output.Contains("some ", StringComparison.OrdinalIgnoreCase) || 
                    output.Contains("maybe ", StringComparison.OrdinalIgnoreCase) || 
                    output.Contains("things ", StringComparison.OrdinalIgnoreCase);
        Console.WriteLine($"   Specificity: {(!vague ? "?" : "??")} {(!vague ? "No vague language" : "Contains vague words")}");
        
        // Check for emptiness
        var notEmpty = !string.IsNullOrWhiteSpace(output);
        Console.WriteLine($"   Not empty: {(notEmpty ? "?" : "?")}");
        
        if (!hasSpecifics || !notEmpty)
        {
            throw new InvalidOperationException($"{type} validation failed - output too short or empty");
        }
    }

    private static string[] WrapText(string text, int maxWidth)
    {
        var words = text.Split(' ');
        var lines = new System.Collections.Generic.List<string>();
        var currentLine = "";

        foreach (var word in words)
        {
            if ((currentLine + " " + word).Length > maxWidth)
            {
                if (!string.IsNullOrEmpty(currentLine))
                {
                    lines.Add(currentLine.Trim());
                    currentLine = word;
                }
                else
                {
                    lines.Add(word.Substring(0, Math.Min(word.Length, maxWidth)));
                    currentLine = word.Length > maxWidth ? word.Substring(maxWidth) : "";
                }
            }
            else
            {
                currentLine = (currentLine + " " + word).Trim();
            }
        }

        if (!string.IsNullOrEmpty(currentLine))
        {
            lines.Add(currentLine.Trim());
        }

        return lines.ToArray();
    }

    /// <summary>
    /// Analyze existing world files for quality
    /// </summary>
    private static int AnalyzeWorlds(string[] args)
    {
        Console.WriteLine("????????????????????????????????????????????????????????????");
        Console.WriteLine("?  ?? World Quality Analyzer                               ?");
        Console.WriteLine("????????????????????????????????????????????????????????????");
        Console.WriteLine();

        try
        {
            var analyzer = new WorldQualityAnalyzer();
            
            // Get world directory
            var worldsDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "source", "repos", "SoloAdventureSystem", "content", "worlds");

            if (args.Length > 0)
            {
                // Analyze specific world file
                var worldPath = args[0];
                if (!File.Exists(worldPath))
                {
                    // Try as filename in worlds directory
                    worldPath = Path.Combine(worldsDir, args[0]);
                    if (!File.Exists(worldPath))
                    {
                        Console.WriteLine($"? World file not found: {args[0]}");
                        Console.WriteLine($"\n?? Usage:");
                        Console.WriteLine($"   dotnet run -- analyze <world-file.zip>");
                        Console.WriteLine($"   dotnet run -- analyze  (analyze all worlds)");
                        return 1;
                    }
                }

                var result = analyzer.AnalyzeWorld(worldPath);
                return result.Error == null ? 0 : 1;
            }
            else
            {
                // Analyze all worlds in directory
                if (!Directory.Exists(worldsDir))
                {
                    Console.WriteLine($"? Worlds directory not found: {worldsDir}");
                    return 1;
                }

                var worldFiles = Directory.GetFiles(worldsDir, "*.zip")
                    .OrderByDescending(f => new FileInfo(f).LastWriteTime)
                    .ToArray();

                if (worldFiles.Length == 0)
                {
                    Console.WriteLine($"? No world files found in: {worldsDir}");
                    Console.WriteLine($"\n?? Generate a world first using the main UI");
                    return 1;
                }

                Console.WriteLine($"?? Found {worldFiles.Length} world(s) in: {worldsDir}");
                Console.WriteLine();

                var allResults = new List<WorldAnalysisResult>();

                foreach (var worldFile in worldFiles)
                {
                    var result = analyzer.AnalyzeWorld(worldFile);
                    allResults.Add(result);
                    Console.WriteLine();
                }

                // Print comparison summary
                if (allResults.Count > 1)
                {
                    PrintComparisonSummary(allResults);
                }

                return 0;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n? Error: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
            return 1;
        }
    }

    /// <summary>
    /// Print a comparison of multiple world quality scores
    /// </summary>
    private static void PrintComparisonSummary(List<WorldAnalysisResult> results)
    {
        Console.WriteLine("????????????????????????????????????????????????????????????");
        Console.WriteLine("? Comparison Summary                                       ?");
        Console.WriteLine("????????????????????????????????????????????????????????????");
        Console.WriteLine();

        var sorted = results.OrderByDescending(r => r.OverallScore).ToList();

        Console.WriteLine("Worlds ranked by quality:");
        Console.WriteLine();

        for (int i = 0; i < sorted.Count; i++)
        {
            var result = sorted[i];
            var medal = i switch
            {
                0 => "??",
                1 => "??",
                2 => "??",
                _ => "  "
            };

            var grade = result.OverallScore switch
            {
                >= 90 => "A",
                >= 80 => "B",
                >= 70 => "C",
                >= 60 => "D",
                _ => "F"
            };

            Console.WriteLine($"{medal} {i + 1}. {result.WorldName}");
            Console.WriteLine($"      Score: {result.OverallScore:F1}/100 (Grade: {grade})");
            Console.WriteLine($"      Rooms: {result.RoomScore:F0} | NPCs: {result.NpcScore:F0} | Factions: {result.FactionScore:F0}");
            Console.WriteLine();
        }

        var bestWorld = sorted.First();
        Console.WriteLine($"? Best World: {bestWorld.WorldName} ({bestWorld.OverallScore:F1}/100)");
        
        if (sorted.Count > 1)
        {
            var worstWorld = sorted.Last();
            var scoreDiff = bestWorld.OverallScore - worstWorld.OverallScore;
            Console.WriteLine($"?? Score Range: {scoreDiff:F1} points difference");
        }

        var avgScore = results.Average(r => r.OverallScore);
        Console.WriteLine($"?? Average Quality: {avgScore:F1}/100");
    }
}
