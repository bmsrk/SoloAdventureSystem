using Terminal.Gui;

namespace SoloAdventureSystem.UI.Themes;

/// <summary>
/// Minimal clean theme for Terminal.Gui applications with high contrast
/// </summary>
public class MinimalTheme : ITheme
{
    public string Name => "Minimal";
    public string Description => "Clean, high-contrast monochrome theme with cyan accents";

    public ColorScheme WindowScheme { get; private set; } = null!;
    public ColorScheme DefaultScheme { get; private set; } = null!;
    public ColorScheme AccentScheme { get; private set; } = null!;
    public ColorScheme SuccessScheme { get; private set; } = null!;
    public ColorScheme WarningScheme { get; private set; } = null!;
    public ColorScheme ErrorScheme { get; private set; } = null!;
    public ColorScheme MutedScheme { get; private set; } = null!;
    public ColorScheme TitleScheme { get; private set; } = null!;
    public ColorScheme ButtonScheme { get; private set; } = null!;
    public ColorScheme PrimaryButtonScheme { get; private set; } = null!;
    public ColorScheme DangerButtonScheme { get; private set; } = null!;

    public MinimalTheme()
    {
        InitializeColorSchemes();
    }

    private void InitializeColorSchemes()
    {
        // Window base - slightly brighter for frames
        WindowScheme = new ColorScheme
        {
            Normal = new Terminal.Gui.Attribute(Color.Gray, Color.Black),
            Focus = new Terminal.Gui.Attribute(Color.BrightCyan, Color.Black),
            HotNormal = new Terminal.Gui.Attribute(Color.BrightCyan, Color.Black),
            HotFocus = new Terminal.Gui.Attribute(Color.Black, Color.BrightCyan),
            Disabled = new Terminal.Gui.Attribute(Color.DarkGray, Color.Black)
        };

        // Default text and controls
        DefaultScheme = new ColorScheme
        {
            Normal = new Terminal.Gui.Attribute(Color.Gray, Color.Black),
            Focus = new Terminal.Gui.Attribute(Color.Black, Color.Gray),
            HotNormal = new Terminal.Gui.Attribute(Color.BrightCyan, Color.Black),
            HotFocus = new Terminal.Gui.Attribute(Color.Black, Color.BrightCyan),
            Disabled = new Terminal.Gui.Attribute(Color.DarkGray, Color.Black)
        };

        // Bright cyan accent for highlights and important elements
        AccentScheme = new ColorScheme
        {
            Normal = new Terminal.Gui.Attribute(Color.BrightCyan, Color.Black),
            Focus = new Terminal.Gui.Attribute(Color.Black, Color.BrightCyan),
            HotNormal = new Terminal.Gui.Attribute(Color.Gray, Color.Black),
            HotFocus = new Terminal.Gui.Attribute(Color.Black, Color.Gray),
            Disabled = new Terminal.Gui.Attribute(Color.DarkGray, Color.Black)
        };

        // Bright green for success messages
        SuccessScheme = new ColorScheme
        {
            Normal = new Terminal.Gui.Attribute(Color.BrightGreen, Color.Black),
            Focus = new Terminal.Gui.Attribute(Color.Black, Color.BrightGreen),
            HotNormal = new Terminal.Gui.Attribute(Color.BrightGreen, Color.Black),
            HotFocus = new Terminal.Gui.Attribute(Color.Black, Color.BrightGreen),
            Disabled = new Terminal.Gui.Attribute(Color.DarkGray, Color.Black)
        };

        // Bright yellow for warnings
        WarningScheme = new ColorScheme
        {
            Normal = new Terminal.Gui.Attribute(Color.BrightYellow, Color.Black),
            Focus = new Terminal.Gui.Attribute(Color.Black, Color.BrightYellow),
            HotNormal = new Terminal.Gui.Attribute(Color.BrightYellow, Color.Black),
            HotFocus = new Terminal.Gui.Attribute(Color.Black, Color.BrightYellow),
            Disabled = new Terminal.Gui.Attribute(Color.DarkGray, Color.Black)
        };

        // Bright red for errors
        ErrorScheme = new ColorScheme
        {
            Normal = new Terminal.Gui.Attribute(Color.BrightRed, Color.Black),
            Focus = new Terminal.Gui.Attribute(Color.Black, Color.BrightRed),
            HotNormal = new Terminal.Gui.Attribute(Color.BrightRed, Color.Black),
            HotFocus = new Terminal.Gui.Attribute(Color.Black, Color.BrightRed),
            Disabled = new Terminal.Gui.Attribute(Color.DarkGray, Color.Black)
        };

        // Muted for secondary information
        MutedScheme = new ColorScheme
        {
            Normal = new Terminal.Gui.Attribute(Color.DarkGray, Color.Black),
            Focus = new Terminal.Gui.Attribute(Color.Black, Color.DarkGray),
            HotNormal = new Terminal.Gui.Attribute(Color.Gray, Color.Black),
            HotFocus = new Terminal.Gui.Attribute(Color.Black, Color.Gray),
            Disabled = new Terminal.Gui.Attribute(Color.DarkGray, Color.Black)
        };

        // Title - bright cyan for headers
        TitleScheme = new ColorScheme
        {
            Normal = new Terminal.Gui.Attribute(Color.BrightCyan, Color.Black),
            Focus = new Terminal.Gui.Attribute(Color.Black, Color.BrightCyan),
            HotNormal = new Terminal.Gui.Attribute(Color.BrightCyan, Color.Black),
            HotFocus = new Terminal.Gui.Attribute(Color.Black, Color.BrightCyan),
            Disabled = new Terminal.Gui.Attribute(Color.DarkGray, Color.Black)
        };

        // Standard button
        ButtonScheme = new ColorScheme
        {
            Normal = new Terminal.Gui.Attribute(Color.Gray, Color.Black),
            Focus = new Terminal.Gui.Attribute(Color.Black, Color.Gray),
            HotNormal = new Terminal.Gui.Attribute(Color.BrightCyan, Color.Black),
            HotFocus = new Terminal.Gui.Attribute(Color.Black, Color.BrightCyan),
            Disabled = new Terminal.Gui.Attribute(Color.DarkGray, Color.Black)
        };

        // Primary action button
        PrimaryButtonScheme = new ColorScheme
        {
            Normal = new Terminal.Gui.Attribute(Color.BrightCyan, Color.Black),
            Focus = new Terminal.Gui.Attribute(Color.Black, Color.BrightCyan),
            HotNormal = new Terminal.Gui.Attribute(Color.Gray, Color.Black),
            HotFocus = new Terminal.Gui.Attribute(Color.Black, Color.Gray),
            Disabled = new Terminal.Gui.Attribute(Color.DarkGray, Color.Black)
        };

        // Danger/destructive button
        DangerButtonScheme = new ColorScheme
        {
            Normal = new Terminal.Gui.Attribute(Color.BrightRed, Color.Black),
            Focus = new Terminal.Gui.Attribute(Color.Black, Color.BrightRed),
            HotNormal = new Terminal.Gui.Attribute(Color.Gray, Color.Black),
            HotFocus = new Terminal.Gui.Attribute(Color.Black, Color.Gray),
            Disabled = new Terminal.Gui.Attribute(Color.DarkGray, Color.Black)
        };
    }

    public void Apply()
    {
        // Set as default application theme
        Colors.Base = DefaultScheme;
    }
}
