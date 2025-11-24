using System;
using Xunit;
using SoloAdventureSystem.ContentGenerator.Configuration;

namespace SoloAdventureSystem.Engine.Tests;

/// <summary>
/// Tests for ModelConfiguration value object
/// </summary>
public class ModelConfigurationTests
{
    [Fact]
    public void WellKnownModels_HaveCorrectProperties()
    {
        // Assert - Phi3Mini
        Assert.Equal("phi-3-mini-q4", ModelConfiguration.Phi3Mini.ModelKey);
        Assert.Equal(2048, ModelConfiguration.Phi3Mini.ContextSize);
        Assert.Equal(2_100_000_000, ModelConfiguration.Phi3Mini.ExpectedSizeBytes);

        // Assert - TinyLlama
        Assert.Equal("tinyllama-q4", ModelConfiguration.TinyLlama.ModelKey);
        Assert.Equal(2048, ModelConfiguration.TinyLlama.ContextSize);
        Assert.Equal(700_000_000, ModelConfiguration.TinyLlama.ExpectedSizeBytes);

        // Assert - Llama32
        Assert.Equal("llama-3.2-1b-q4", ModelConfiguration.Llama32.ModelKey);
        Assert.Equal(2048, ModelConfiguration.Llama32.ContextSize);
        Assert.Equal(800_000_000, ModelConfiguration.Llama32.ExpectedSizeBytes);
    }

    [Fact]
    public void Default_ReturnsPhi3Mini()
    {
        // Act
        var defaultModel = ModelConfiguration.Default;

        // Assert
        Assert.Equal(ModelConfiguration.Phi3Mini, defaultModel);
    }

    [Theory]
    [InlineData("phi-3-mini-q4", "phi-3-mini-q4")]
    [InlineData("phi3", "phi-3-mini-q4")]
    [InlineData("tinyllama-q4", "tinyllama-q4")]
    [InlineData("tinyllama", "tinyllama-q4")]
    [InlineData("llama-3.2-1b-q4", "llama-3.2-1b-q4")]
    [InlineData("llama32", "llama-3.2-1b-q4")]
    public void Parse_WithKnownModel_ReturnsCorrectConfiguration(string input, string expectedKey)
    {
        // Act
        var config = ModelConfiguration.Parse(input);

        // Assert
        Assert.Equal(expectedKey, config.ModelKey);
        Assert.True(config.ContextSize > 0);
        Assert.True(config.ExpectedSizeBytes > 0);
    }

    [Fact]
    public void Parse_WithUnknownModel_CreatesDefaultConfiguration()
    {
        // Act
        var config = ModelConfiguration.Parse("unknown-model");

        // Assert
        Assert.Equal("unknown-model", config.ModelKey);
        Assert.Equal(2048, config.ContextSize);
        Assert.Equal(1_000_000_000, config.ExpectedSizeBytes);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Parse_WithInvalidInput_ThrowsArgumentException(string? input)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => ModelConfiguration.Parse(input!));
    }

    [Fact]
    public void Create_WithValidParameters_ReturnsConfiguration()
    {
        // Act
        var config = ModelConfiguration.Create("custom-model", 4096, 5_000_000_000);

        // Assert
        Assert.Equal("custom-model", config.ModelKey);
        Assert.Equal(4096, config.ContextSize);
        Assert.Equal(5_000_000_000, config.ExpectedSizeBytes);
    }

    [Theory]
    [InlineData(null, 2048, 1000)]
    [InlineData("", 2048, 1000)]
    [InlineData("   ", 2048, 1000)]
    public void Create_WithInvalidModelKey_ThrowsArgumentException(string? modelKey, int contextSize, long size)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => ModelConfiguration.Create(modelKey!, contextSize, size));
    }

    [Theory]
    [InlineData("valid", 0, 1000)]
    [InlineData("valid", -1, 1000)]
    public void Create_WithInvalidContextSize_ThrowsArgumentException(string modelKey, int contextSize, long size)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => ModelConfiguration.Create(modelKey, contextSize, size));
    }

    [Theory]
    [InlineData("valid", 2048, 0)]
    [InlineData("valid", 2048, -1)]
    public void Create_WithInvalidSize_ThrowsArgumentException(string modelKey, int contextSize, long size)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => ModelConfiguration.Create(modelKey, contextSize, size));
    }

    [Fact]
    public void ToString_ReturnsModelKey()
    {
        // Arrange
        var config = ModelConfiguration.Phi3Mini;

        // Act
        var result = config.ToString();

        // Assert
        Assert.Equal("phi-3-mini-q4", result);
    }

    [Fact]
    public void ValueObjectEquality_SameModel_AreEqual()
    {
        // Arrange
        var config1 = ModelConfiguration.Phi3Mini;
        var config2 = ModelConfiguration.Phi3Mini;

        // Assert
        Assert.Equal(config1, config2);
    }

    [Fact]
    public void ValueObjectEquality_DifferentModels_AreNotEqual()
    {
        // Arrange
        var config1 = ModelConfiguration.Phi3Mini;
        var config2 = ModelConfiguration.TinyLlama;

        // Assert
        Assert.NotEqual(config1, config2);
    }
}
