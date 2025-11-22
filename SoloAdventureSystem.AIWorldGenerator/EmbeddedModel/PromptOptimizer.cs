using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace SoloAdventureSystem.ContentGenerator.EmbeddedModel;

/// <summary>
/// Optimizes prompts for small embedded models.
/// Reduces token count while preserving quality.
/// </summary>
public class PromptOptimizer
{
    /// <summary>
    /// Optimizes a system prompt for smaller models.
    /// Removes examples if too long, condenses instructions.
    /// </summary>
    public string OptimizeSystemPrompt(string systemPrompt)
    {
        if (string.IsNullOrWhiteSpace(systemPrompt))
            return string.Empty;
        
        // For small models, we need to be more concise
        // Keep only the core instructions, remove verbose examples
        
        var lines = systemPrompt.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        var optimizedLines = lines
            .Where(line => !line.TrimStart().StartsWith("Example")) // Remove example lines
            .Select(line => line.Trim())
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .ToList();
        
        // If we removed examples, make sure we still have the core instruction
        var result = string.Join(" ", optimizedLines);
        
        // Condense multiple spaces
        result = Regex.Replace(result, @"\s+", " ");
        
        // Limit to reasonable length (small models have limited context)
        if (result.Length > 500)
        {
            // Keep first 500 chars (usually contains the core instruction)
            result = result.Substring(0, 500) + "...";
        }
        
        return result.Trim();
    }
    
    /// <summary>
    /// Creates a chat-formatted prompt for instruction-tuned models.
    /// </summary>
    public string FormatChatPrompt(string systemPrompt, string userPrompt)
    {
        // Phi-3 uses a specific chat template
        return $"<|system|>{systemPrompt}<|end|><|user|>{userPrompt}<|end|><|assistant|>";
    }
    
    /// <summary>
    /// Estimates token count (rough approximation).
    /// </summary>
    public int EstimateTokenCount(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return 0;
        
        // Rough estimate: ~4 characters per token on average
        return text.Length / 4;
    }
}
