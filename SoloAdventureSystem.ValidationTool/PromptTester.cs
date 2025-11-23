using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SoloAdventureSystem.ContentGenerator.Adapters;
using SoloAdventureSystem.ContentGenerator.Configuration;

namespace SoloAdventureSystem.Tools;

/// <summary>
/// Simple wrapper to test if LLamaSharp is answering to prompts correctly.
/// Quick validation of text generation without full test suite.
/// </summary>
public class PromptTester : IDisposable
{
    private readonly LLamaSharpAdapter _adapter;
    private readonly ILogger? _logger;

    public PromptTester(string modelKey = "phi-3-mini-q4", ILogger? logger = null)
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
            Console.WriteLine("? Initializing LLamaSharp adapter...");
            await _adapter.InitializeAsync();
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
    public PromptTestResult TestPrompt(string prompt, int seed = 42, PromptType type = PromptType.RoomDescription)
    {
        var startTime = DateTime.UtcNow;
        
        try
        {
            string result = type switch
            {
                PromptType.RoomDescription => _adapter.GenerateRoomDescription(prompt, seed),
                PromptType.NpcBio => _adapter.GenerateNpcBio(prompt, seed),
                PromptType.FactionFlavor => _adapter.GenerateFactionFlavor(prompt, seed),
                _ => throw new ArgumentException($"Unknown prompt type: {type}")
            };

            var duration = DateTime.UtcNow - startTime;

            return new PromptTestResult
            {
                Success = true,
                Prompt = prompt,
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
                Prompt = prompt,
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
            results[i] = TestPrompt(prompt, seed, type);
        }
        
        return results;
    }

    /// <summary>
    /// Quick validation test - tests one prompt of each type
    /// </summary>
    public bool QuickValidation()
    {
        Console.WriteLine("\n???????????????????????????????????????????????????????????");
        Console.WriteLine("? Quick Validation Test                                   ?");
        Console.WriteLine("???????????????????????????????????????????????????????????\n");

        var tests = new[]
        {
            ("Describe a cyberpunk room with neon lights.", 42, PromptType.RoomDescription),
            ("Create a bio for a hacker named Zero.", 123, PromptType.NpcBio),
            ("Describe a powerful corporate faction.", 456, PromptType.FactionFlavor)
        };

        var results = TestPrompts(tests);
        var allSuccess = true;

        foreach (var result in results)
        {
            result.PrintResult();
            Console.WriteLine();
            
            if (!result.Success)
            {
                allSuccess = false;
            }
        }

        if (allSuccess)
        {
            Console.WriteLine("? All quick validation tests passed!");
        }
        else
        {
            Console.WriteLine("? Some validation tests failed");
        }

        return allSuccess;
    }

    public void Dispose()
    {
        _adapter?.Dispose();
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
        Console.WriteLine($"Type: {Type}");
        Console.WriteLine($"Prompt: \"{Prompt}\"");
        Console.WriteLine($"Seed: {Seed}");
        
        if (Success && Response != null)
        {
            Console.WriteLine($"? Success in {Duration.TotalSeconds:F1}s ({CharacterCount} chars)");
            Console.WriteLine("Response:");
            Console.WriteLine("???????????????????????????????????????????????????????????");
            
            // Truncate if too long
            if (Response.Length > 200)
            {
                Console.WriteLine(Response.Substring(0, 200) + "...");
            }
            else
            {
                Console.WriteLine(Response);
            }
            Console.WriteLine("???????????????????????????????????????????????????????????");
        }
        else
        {
            Console.WriteLine($"? Failed in {Duration.TotalSeconds:F1}s");
            Console.WriteLine($"Error: {Error}");
        }
    }

    public bool IsValid()
    {
        return Success && 
               !string.IsNullOrWhiteSpace(Response) && 
               CharacterCount > 20;
    }
}
