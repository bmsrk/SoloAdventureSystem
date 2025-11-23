using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SoloAdventureSystem.ContentGenerator;
using SoloAdventureSystem.ContentGenerator.Adapters;
using SoloAdventureSystem.ContentGenerator.Configuration;
using SoloAdventureSystem.ContentGenerator.EmbeddedModel;
using SoloAdventureSystem.ContentGenerator.Generation;
using SoloAdventureSystem.ContentGenerator.Models;

namespace SoloAdventureSystem.ValidationTool;

/// <summary>
/// Simple wrapper to test if LLamaSharp is answering to prompts correctly.
/// Quick validation of text generation without full test suite.
/// Now uses the actual prompt templates for realistic testing.
/// </summary>
public class PromptTester : IDisposable
{
    private readonly LLamaSharpAdapter _adapter;
    private readonly ILogger? _logger;

    public PromptTester(string modelKey = "tinyllama-q4", ILogger? logger = null)
    {
        _logger = logger;
        
        var settings = Options.Create(new AISettings
        {
            Provider = "LLamaSharp",
            Model = modelKey,
            LLamaModelKey = modelKey,
            ContextSize = 2048,
            UseGPU = false,
            MaxInferenceThreads = 4
        });

        _adapter = new LLamaSharpAdapter(settings, logger as ILogger<LLamaSharpAdapter>);
    }

    /// <summary>
    /// Initialize the adapter (downloads model if needed)
    /// </summary>
    public async Task<bool> InitializeAsync()
    {
        try
        {
            Console.WriteLine("?? Initializing LLamaSharp adapter...");
            
            var progress = new Progress<DownloadProgress>(p =>
            {
                if (p.PercentComplete % 10 == 0 || p.PercentComplete == 100)
                {
                    var downloadedMB = p.DownloadedBytes / 1024.0 / 1024.0;
                    var totalMB = p.TotalBytes / 1024.0 / 1024.0;
                    var speedMB = p.SpeedBytesPerSecond / 1024.0 / 1024.0;
                    Console.WriteLine($"   ?? {downloadedMB:F0}/{totalMB:F0} MB ({p.PercentComplete}%) - {speedMB:F1} MB/s");
                }
            });
            
            await _adapter.InitializeAsync(progress);
            Console.WriteLine("? Adapter initialized successfully");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"? Failed to initialize: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Test a single prompt and display the result
    /// </summary>
    public PromptTestResult TestPrompt(string userPrompt, int seed = 42, PromptType type = PromptType.RoomDescription)
    {
        var startTime = DateTime.UtcNow;
        
        try
        {
            string result = type switch
            {
                PromptType.RoomDescription => _adapter.GenerateRoomDescription(userPrompt, seed),
                PromptType.NpcBio => _adapter.GenerateNpcBio(userPrompt, seed),
                PromptType.FactionFlavor => _adapter.GenerateFactionFlavor(userPrompt, seed),
                _ => throw new ArgumentException($"Unknown prompt type: {type}")
            };

            var duration = DateTime.UtcNow - startTime;

            return new PromptTestResult
            {
                Success = true,
                Prompt = userPrompt,
                Response = result,
                Duration = duration,
                Seed = seed,
                Type = type,
                CharacterCount = result.Length
            };
        }
        catch (Exception ex)
        {
            var duration = DateTime.UtcNow - startTime;
            
            return new PromptTestResult
            {
                Success = false,
                Prompt = userPrompt,
                Response = null,
                Duration = duration,
                Seed = seed,
                Type = type,
                Error = ex.Message
            };
        }
    }

    /// <summary>
    /// Test multiple prompts and return all results
    /// </summary>
    public PromptTestResult[] TestPrompts(params (string prompt, int seed, PromptType type)[] tests)
    {
        var results = new PromptTestResult[tests.Length];
        
        for (int i = 0; i < tests.Length; i++)
        {
            var (prompt, seed, type) = tests[i];
            Console.WriteLine($"\n?? Test {i + 1}/{tests.Length}: {type}");
            results[i] = TestPrompt(prompt, seed, type);
        }
        
        return results;
    }

    /// <summary>
    /// Quick validation test - uses realistic world generation prompts
    /// </summary>
    public bool QuickValidation()
    {
        Console.WriteLine("\n????????????????????????????????????????????????????????????");
        Console.WriteLine("? Quick Validation Test - Using Real World Gen Prompts   ?");
        Console.WriteLine("????????????????????????????????????????????????????????????\n");

        // Create realistic world options
        var options = new WorldGenerationOptions
        {
            Name = "TestWorld",
            Seed = 12345,
            Theme = "Cyberpunk",
            Flavor = "Dark and gritty with neon highlights",
            Description = "A sprawling megacity where corporations rule everything",
            MainPlotPoint = "Uncover the conspiracy behind the neural implant murders",
            TimePeriod = "2089",
            PowerStructure = "Megacorporations, hackers, and street gangs"
        };

        var tests = new[]
        {
            (PromptTemplates.BuildRoomPrompt("Neural Nexus", options, "oppressive and high-tech", 0, 5), 42, PromptType.RoomDescription),
            (PromptTemplates.BuildNpcPrompt("Marcus Chen", options, "Neural Nexus", "Chrome Syndicate"), 123, PromptType.NpcBio),
            (PromptTemplates.BuildFactionPrompt("Chrome Syndicate", options), 456, PromptType.FactionFlavor)
        };

        var results = TestPrompts(tests);
        var allSuccess = true;
        var totalChars = 0;

        Console.WriteLine("\n????????????????????????????????????????????????????????????");
        Console.WriteLine("? Results                                                  ?");
        Console.WriteLine("????????????????????????????????????????????????????????????\n");

        foreach (var result in results)
        {
            result.PrintResult();
            Console.WriteLine();
            
            if (!result.Success)
            {
                allSuccess = false;
            }
            else
            {
                totalChars += result.CharacterCount;
            }
        }

        Console.WriteLine("????????????????????????????????????????????????????????????");
        Console.WriteLine("? Summary                                                  ?");
        Console.WriteLine("????????????????????????????????????????????????????????????\n");

        if (allSuccess)
        {
            var totalTime = results[0].Duration + results[1].Duration + results[2].Duration;
            Console.WriteLine($"? All {results.Length} validation tests passed!");
            Console.WriteLine($"?? Total generation time: {totalTime.TotalSeconds:F1}s");
            Console.WriteLine($"?? Total characters: {totalChars}");
            Console.WriteLine($"? Average speed: {totalChars / totalTime.TotalSeconds:F1} chars/sec");
            
            // Quality checks
            Console.WriteLine("\n?? Quality Checks:");
            var roomResult = results[0];
            var npcResult = results[1];
            var factionResult = results[2];
            
            // Check sentence counts (approximate)
            var roomSentences = roomResult.Response?.Split('.', '!', '?').Length ?? 0;
            var npcSentences = npcResult.Response?.Split('.', '!', '?').Length ?? 0;
            var factionSentences = factionResult.Response?.Split('.', '!', '?').Length ?? 0;
            
            Console.WriteLine($"   Room: ~{roomSentences} sentences (target: 3)");
            Console.WriteLine($"   NPC: ~{npcSentences} sentences (target: 2)");
            Console.WriteLine($"   Faction: ~{factionSentences} sentences (target: 3)");
            
            // Check for specificity
            var hasColors = (roomResult.Response?.Contains("blue") ?? false) || 
                           (roomResult.Response?.Contains("red") ?? false) ||
                           (roomResult.Response?.Contains("neon") ?? false);
            
            var hasSpecifics = (roomResult.Response?.Length ?? 0) > 100;
            
            Console.WriteLine($"\n   {(hasColors ? "?" : "?")} Contains specific details (colors, etc.)");
            Console.WriteLine($"   {(hasSpecifics ? "?" : "?")} Sufficient length (>100 chars)");
            Console.WriteLine($"   {(roomSentences <= 4 ? "?" : "?")} Room follows sentence count (~3)");
            Console.WriteLine($"   {(npcSentences <= 3 ? "?" : "?")} NPC follows sentence count (~2)");
            Console.WriteLine($"   {(factionSentences <= 4 ? "?" : "?")} Faction follows sentence count (~3)");
        }
        else
        {
            Console.WriteLine("? Some validation tests failed");
            Console.WriteLine("   Check the error messages above for details.");
        }

        return allSuccess;
    }

    public void Dispose()
    {
        _adapter?.Dispose();
        GC.SuppressFinalize(this);
    }
}

/// <summary>
/// Type of prompt being tested
/// </summary>
public enum PromptType
{
    RoomDescription,
    NpcBio,
    FactionFlavor
}

/// <summary>
/// Result of a prompt test
/// </summary>
public class PromptTestResult
{
    public bool Success { get; set; }
    public string Prompt { get; set; } = string.Empty;
    public string? Response { get; set; }
    public TimeSpan Duration { get; set; }
    public int Seed { get; set; }
    public PromptType Type { get; set; }
    public int CharacterCount { get; set; }
    public string? Error { get; set; }

    public void PrintResult()
    {
        var typeIcon = Type switch
        {
            PromptType.RoomDescription => "??",
            PromptType.NpcBio => "??",
            PromptType.FactionFlavor => "??",
            _ => "??"
        };
        
        Console.WriteLine($"{typeIcon} Type: {Type}");
        
        // Show shortened prompt
        var shortPrompt = Prompt.Length > 80 ? Prompt.Substring(0, 77) + "..." : Prompt;
        Console.WriteLine($"?? Prompt: \"{shortPrompt}\"");
        Console.WriteLine($"?? Seed: {Seed}");
        
        if (Success && Response != null)
        {
            Console.WriteLine($"? Success in {Duration.TotalSeconds:F1}s ({CharacterCount} chars)");
            Console.WriteLine("Response:");
            Console.WriteLine("???????????????????????????????????????????????????????????");
            
            // Wrap text nicely
            var lines = WrapText(Response, 57);
            foreach (var line in lines)
            {
                Console.WriteLine($"? {line,-55} ?");
            }
            
            Console.WriteLine("???????????????????????????????????????????????????????????");
            
            // Quick quality indicators
            var sentences = Response.Split('.', '!', '?').Length - 1;
            var hasSpecifics = Response.Length > 50;
            Console.WriteLine($"?? Quality: {sentences} sentences, {(hasSpecifics ? "specific" : "generic")}");
        }
        else
        {
            Console.WriteLine($"? Failed in {Duration.TotalSeconds:F1}s");
            Console.WriteLine($"? Error: {Error}");
        }
    }

    private string[] WrapText(string text, int maxWidth)
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
                    // Word is longer than max width, split it
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

    public bool IsValid()
    {
        return Success && 
               !string.IsNullOrWhiteSpace(Response) && 
               CharacterCount > 20;
    }
}
