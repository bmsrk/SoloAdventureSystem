using System;
using Xunit;
using SoloAdventureSystem.ContentGenerator.Configuration;

namespace SoloAdventureSystem.Engine.Tests;

/// <summary>
/// Tests for AIProvider value object
/// </summary>
public class AIProviderTests
{
    [Fact]
    public void WellKnownProviders_HaveCorrectNames()
    {
        // Assert
        Assert.Equal("Stub", AIProvider.Stub.Name);
        Assert.Equal("MaIN.NET", AIProvider.MaIN.Name);
        Assert.Equal("LLamaSharp", AIProvider.LLamaSharp.Name);
    }

    [Theory]
    [InlineData("stub", "Stub")]
    [InlineData("STUB", "Stub")]
    [InlineData("main.net", "MaIN.NET")]
    [InlineData("main", "MaIN.NET")]
    [InlineData("llamasharp", "LLamaSharp")]
    [InlineData("llama", "LLamaSharp")]
    public void Parse_WithValidInput_ReturnsCorrectProvider(string input, string expectedName)
    {
        // Act
        var provider = AIProvider.Parse(input);

        // Assert
        Assert.Equal(expectedName, provider.Name);
    }

    [Fact]
    public void Parse_WithCustomProvider_CreatesNewInstance()
    {
        // Act
        var provider = AIProvider.Parse("CustomProvider");

        // Assert
        Assert.Equal("CustomProvider", provider.Name);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Parse_WithInvalidInput_ThrowsArgumentException(string? input)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => AIProvider.Parse(input!));
    }

    [Fact]
    public void ToString_ReturnsProviderName()
    {
        // Arrange
        var provider = AIProvider.MaIN;

        // Act
        var result = provider.ToString();

        // Assert
        Assert.Equal("MaIN.NET", result);
    }

    [Fact]
    public void ImplicitConversion_ToString_Works()
    {
        // Arrange
        AIProvider provider = AIProvider.Stub;

        // Act
        string providerString = provider;

        // Assert
        Assert.Equal("Stub", providerString);
    }

    [Fact]
    public void ValueObjectEquality_SameProvider_AreEqual()
    {
        // Arrange
        var provider1 = AIProvider.Stub;
        var provider2 = AIProvider.Stub;

        // Assert
        Assert.Equal(provider1, provider2);
        Assert.True(provider1 == provider2);
    }

    [Fact]
    public void ValueObjectEquality_DifferentProviders_AreNotEqual()
    {
        // Arrange
        var provider1 = AIProvider.Stub;
        var provider2 = AIProvider.MaIN;

        // Assert
        Assert.NotEqual(provider1, provider2);
        Assert.True(provider1 != provider2);
    }
}
