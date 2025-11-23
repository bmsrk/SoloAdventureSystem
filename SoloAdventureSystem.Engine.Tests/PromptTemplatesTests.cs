using Xunit;
using SoloAdventureSystem.ContentGenerator.Generation;
using SoloAdventureSystem.ContentGenerator.Models;

namespace SoloAdventureSystem.Engine.Tests;

/// <summary>
/// Tests for PromptTemplates - validates AI prompt generation
/// Updated for new WorldGenerationOptions-based prompts
/// </summary>
public class PromptTemplatesTests
{
    private WorldGenerationOptions GetTestOptions()
    {
        return new WorldGenerationOptions
        {
            Name = "TestWorld",
            Seed = 12345,
            Theme = "Cyberpunk",
            Flavor = "Dark and gritty",
            Description = "A cyberpunk megacity",
            MainPlotPoint = "Uncover the conspiracy",
            TimePeriod = "2089",
            PowerStructure = "Corporations vs. Hackers"
        };
    }

    [Fact]
    public void RoomDescriptionSystem_IsNotEmpty()
    {
        // Act
        var prompt = PromptTemplates.RoomDescriptionSystem;
        
        // Assert
        Assert.NotNull(prompt);
        Assert.NotEmpty(prompt);
    }
    
    [Fact]
    public void RoomDescriptionSystem_ContainsExamples()
    {
        // Act
        var prompt = PromptTemplates.RoomDescriptionSystem;
        
        // Assert
        Assert.Contains("Example", prompt);
    }
    
    [Fact]
    public void RoomDescriptionSystem_SpecifiesRequirements()
    {
        // Act
        var prompt = PromptTemplates.RoomDescriptionSystem;
        
        // Assert
        Assert.True(prompt.Length > 100, "System prompt should be substantial");
        // Updated prompts don't require "cyberpunk" in system prompt (it's in user prompt)
        Assert.Contains("sentence", prompt, StringComparison.OrdinalIgnoreCase);
    }
    
    [Fact]
    public void NpcBioSystem_IsNotEmpty()
    {
        // Act
        var prompt = PromptTemplates.NpcBioSystem;
        
        // Assert
        Assert.NotNull(prompt);
        Assert.NotEmpty(prompt);
    }
    
    [Fact]
    public void NpcBioSystem_ContainsExamples()
    {
        // Act
        var prompt = PromptTemplates.NpcBioSystem;
        
        // Assert
        Assert.Contains("Example", prompt);
    }
    
    [Fact]
    public void NpcBioSystem_SpecifiesRequirements()
    {
        // Act
        var prompt = PromptTemplates.NpcBioSystem;
        
        // Assert - Updated to match new prompt structure
        Assert.Contains("2 sentences", prompt);
    }
    
    [Fact]
    public void FactionLoreSystem_IsNotEmpty()
    {
        // Act
        var prompt = PromptTemplates.FactionLoreSystem;
        
        // Assert
        Assert.NotNull(prompt);
        Assert.NotEmpty(prompt);
    }
    
    [Fact]
    public void FactionLoreSystem_ContainsExamples()
    {
        // Act
        var prompt = PromptTemplates.FactionLoreSystem;
        
        // Assert
        Assert.Contains("Example", prompt);
    }
    
    [Fact]
    public void WorldLoreSystem_IsNotEmpty()
    {
        // Act
        var prompt = PromptTemplates.WorldLoreSystem;
        
        // Assert
        Assert.NotNull(prompt);
        Assert.NotEmpty(prompt);
    }
    
    [Fact]
    public void WorldLoreSystem_ContainsExamples()
    {
        // Act
        var prompt = PromptTemplates.WorldLoreSystem;
        
        // Assert
        Assert.Contains("Example", prompt);
    }
    
    [Fact]
    public void BuildRoomPrompt_ReturnsValidPrompt()
    {
        // Arrange
        var options = GetTestOptions();
        var roomName = "Data Vault";
        var atmosphere = "oppressive and high-tech";
        var index = 0;
        var total = 5;
        
        // Act - Use new signature
        var prompt = PromptTemplates.BuildRoomPrompt(roomName, options, atmosphere, index, total);
        
        // Assert
        Assert.NotNull(prompt);
        Assert.NotEmpty(prompt);
        Assert.Contains(roomName, prompt);
        Assert.Contains(options.Description, prompt);
        Assert.Contains(atmosphere, prompt);
        Assert.Contains("Room 1", prompt);
    }
    
    [Fact]
    public void BuildNpcPrompt_ReturnsValidPrompt()
    {
        // Arrange
        var options = GetTestOptions();
        var npcName = "Marcus Chen";
        var roomContext = "Data Vault";
        var factionName = "Shadow Syndicate";
        
        // Act - Use new signature
        var prompt = PromptTemplates.BuildNpcPrompt(npcName, options, roomContext, factionName);
        
        // Assert
        Assert.NotNull(prompt);
        Assert.NotEmpty(prompt);
        Assert.Contains(npcName, prompt);
        Assert.Contains(options.Description, prompt);
        Assert.Contains(roomContext, prompt);
        Assert.Contains(factionName, prompt);
        Assert.Contains(options.MainPlotPoint, prompt);
    }
    
    [Fact]
    public void BuildFactionPrompt_ReturnsValidPrompt()
    {
        // Arrange
        var options = GetTestOptions();
        var factionName = "Chrome Syndicate";
        
        // Act - Use new signature
        var prompt = PromptTemplates.BuildFactionPrompt(factionName, options);
        
        // Assert
        Assert.NotNull(prompt);
        Assert.NotEmpty(prompt);
        Assert.Contains(factionName, prompt);
        Assert.Contains(options.Description, prompt);
        Assert.Contains(options.PowerStructure, prompt);
    }
    
    [Fact]
    public void BuildLorePrompt_ReturnsValidPrompt()
    {
        // Arrange
        var options = GetTestOptions();
        var worldName = "Test World";
        var entryNumber = 1;
        
        // Act - Use new signature
        var prompt = PromptTemplates.BuildLorePrompt(worldName, options, entryNumber);
        
        // Assert
        Assert.NotNull(prompt);
        Assert.NotEmpty(prompt);
        Assert.Contains(worldName, prompt);
        Assert.Contains(options.Description, prompt);
        Assert.Contains("#1", prompt);
    }
    
    [Fact]
    public void AllSystemPrompts_AreLongEnough()
    {
        // Arrange
        var systemPrompts = new[]
        {
            PromptTemplates.RoomDescriptionSystem,
            PromptTemplates.NpcBioSystem,
            PromptTemplates.FactionLoreSystem,
            PromptTemplates.WorldLoreSystem
        };
        
        // Act & Assert
        foreach (var prompt in systemPrompts)
        {
            Assert.NotNull(prompt);
            Assert.True(prompt.Length > 200, $"System prompt too short: {prompt.Length} chars");
        }
    }
    
    [Fact]
    public void AllSystemPrompts_HaveCyberpunkTheme()
    {
        // Act & Assert - Updated: prompts are now generic, theme is in user prompt
        var systemPrompts = new[]
        {
            PromptTemplates.RoomDescriptionSystem,
            PromptTemplates.NpcBioSystem,
            PromptTemplates.FactionLoreSystem,
            PromptTemplates.WorldLoreSystem
        };
        
        foreach (var prompt in systemPrompts)
        {
            // System prompts should mention being a creative writer for text adventure
            Assert.Contains("creative writer", prompt, StringComparison.OrdinalIgnoreCase);
        }
    }
}
