using Terminal.Gui;

namespace SoloAdventureSystem.UI.Themes;

/// <summary>
/// Manages theme application and switching for the application
/// </summary>
public static class ThemeManager
{
    private static ITheme _currentTheme = new MinimalTheme();

    /// <summary>
    /// Gets the currently active theme
    /// </summary>
    public static ITheme Current => _currentTheme;

    /// <summary>
    /// Sets and applies a new theme
    /// </summary>
    public static void SetTheme(ITheme theme)
    {
        _currentTheme = theme;
        _currentTheme.Apply();
    }

    /// <summary>
    /// Applies the current theme
    /// </summary>
    public static void ApplyCurrentTheme()
    {
        _currentTheme.Apply();
    }
}
