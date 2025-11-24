using System.Collections.Generic;
using SoloAdventureSystem.ContentGenerator.Models;

namespace SoloAdventureSystem.ContentGenerator.Generation;

/// <summary>
/// Interface for content generators that produce specific types of game content.
/// Enables strategy pattern for specialized generation.
/// </summary>
public interface IContentGenerator<T>
{
    T Generate(WorldGenerationContext context);
}
