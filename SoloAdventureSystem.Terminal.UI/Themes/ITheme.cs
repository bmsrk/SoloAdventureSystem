using Terminal.Gui;

namespace SoloAdventureSystem.UI.Themes;

/// <summary>
/// Interface defining a complete UI theme for the application
/// </summary>
public interface ITheme
{
    /// <summary>
    /// Name of the theme
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Description of the theme
    /// </summary>
    string Description { get; }

    // Color Schemes
    ColorScheme WindowScheme { get; }
    ColorScheme DefaultScheme { get; }
    ColorScheme AccentScheme { get; }
    ColorScheme SuccessScheme { get; }
    ColorScheme WarningScheme { get; }
    ColorScheme ErrorScheme { get; }
    ColorScheme MutedScheme { get; }
    ColorScheme TitleScheme { get; }
    ColorScheme ButtonScheme { get; }
    ColorScheme PrimaryButtonScheme { get; }
    ColorScheme DangerButtonScheme { get; }

    /// <summary>
    /// Apply this theme to the application
    /// </summary>
    void Apply();
}
