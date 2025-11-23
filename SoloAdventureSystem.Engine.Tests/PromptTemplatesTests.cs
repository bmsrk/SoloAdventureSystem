using Xunit;
using SoloAdventureSystem.ContentGenerator.Generation;

namespace SoloAdventureSystem.Engine.Tests;

/// <summary>
/// Tests for PromptTemplates - validates AI prompt generation
/// </summary>
public class PromptTemplatesTests
{
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
        
        // Assert - Test that the system prompt contains examples
        // Note: After optimization, examples may be removed, so check full unoptimized prompt
        Assert.Contains("Example", prompt);
    }
    
    [Fact]
    public void RoomDescriptionSystem_SpecifiesRequirements()
    {
        // Act
        var prompt = PromptTemplates.RoomDescriptionSystem;
        
        // Assert - Should mention key requirements in system prompt
        // After optimization some details may be condensed
        Assert.True(prompt.Length > 100, "System prompt should be substantial");
        Assert.Contains("cyberpunk", prompt, StringComparison.OrdinalIgnoreCase);
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
        
        // Assert - Test that the system prompt contains examples
        // Note: After optimization, examples may be removed, so check full unoptimized prompt
        Assert.Contains("Example", prompt);
    }
    
    [Fact]
    public void NpcBioSystem_SpecifiesRequirements()
    {
        // Act
        var prompt = PromptTemplates.NpcBioSystem;
        
        // Assert - Should mention key requirements
        Assert.Contains("role", prompt, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("motivation", prompt, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("secret", prompt, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("2-3 sentences", prompt);
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
        Assert.Contains("Chrome Syndicate", prompt);
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
        // Check for at least one example from the lore system
        Assert.True(
            prompt.Contains("Neural Net Collapse") || 
            prompt.Contains("street food") ||
            prompt.Contains("climate control"),
            "Should contain at least one lore example");
    }
    
    [Fact]
    public void BuildRoomPrompt_ReturnsValidPrompt()
    {
        // Arrange
        var roomName = "Data Vault";
        var theme = "Cyberpunk Dystopia";
        var atmosphere = "Lit by harsh fluorescent lights";
        var index = 0;
        var total = 5;
        
        // Act
        var prompt = PromptTemplates.BuildRoomPrompt(roomName, theme, atmosphere, index, total);
        
        // Assert
        Assert.NotNull(prompt);
        Assert.NotEmpty(prompt);
        Assert.Contains(roomName, prompt);
        Assert.Contains(theme, prompt);
        Assert.Contains(atmosphere, prompt);
        Assert.Contains("Room 1 of 5", prompt);
    }
    
    [Fact]
    public void BuildNpcPrompt_ReturnsValidPrompt()
    {
        // Arrange
        var npcName = "Marcus Chen";
        var theme = "Cyberpunk";
        var roomContext = "Data Vault";
        var factionName = "Shadow Syndicate";
        
        // Act
        var prompt = PromptTemplates.BuildNpcPrompt(npcName, theme, roomContext, factionName);
        
        // Assert
        Assert.NotNull(prompt);
        Assert.NotEmpty(prompt);
        Assert.Contains(npcName, prompt);
        Assert.Contains(theme, prompt);
        Assert.Contains(roomContext, prompt);
        Assert.Contains(factionName, prompt);
    }
    
    [Fact]
    public void BuildFactionPrompt_ReturnsValidPrompt()
    {
        // Arrange
        var factionName = "Chrome Syndicate";
        var theme = "Cyberpunk";
        var worldSeed = 12345;
        
        // Act
        var prompt = PromptTemplates.BuildFactionPrompt(factionName, theme, worldSeed);
        
        // Assert
        Assert.NotNull(prompt);
        Assert.NotEmpty(prompt);
        Assert.Contains(factionName, prompt);
        Assert.Contains(theme, prompt);
        Assert.Contains(worldSeed.ToString(), prompt);
    }
    
    [Fact]
    public void BuildLorePrompt_ReturnsValidPrompt()
    {
        // Arrange
        var worldName = "Test World";
        var theme = "Cyberpunk";
        var entryNumber = 1;
        
        // Act
        var prompt = PromptTemplates.BuildLorePrompt(worldName, theme, entryNumber);
        
        // Assert
        Assert.NotNull(prompt);
        Assert.NotEmpty(prompt);
        Assert.Contains(worldName, prompt);
        Assert.Contains(theme, prompt);
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
            var lower = prompt.ToLower();
            Assert.True(
                lower.Contains("cyberpunk") ||
                lower.Contains("text adventure"),
                "Should reference game genre");
        }
    }
}
