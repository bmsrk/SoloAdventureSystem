using Terminal.Gui;

namespace SoloAdventureSystem.UI.Themes;

/// <summary>
/// Visual Studio inspired dark theme with purple accents
/// </summary>
public class VSDarkTheme : ITheme
{
    public string Name => "VS Dark";
    public string Description => "Visual Studio inspired dark theme with purple accents";

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

    public VSDarkTheme()
    {
        InitializeColorSchemes();
    }

    private void InitializeColorSchemes()
    {
        // VS Dark uses light text on dark background
        WindowScheme = new ColorScheme
        {
            Normal = new Terminal.Gui.Attribute(Color.Gray, Color.Black),
            Focus = new Terminal.Gui.Attribute(Color.Magenta, Color.Black),
            HotNormal = new Terminal.Gui.Attribute(Color.Magenta, Color.Black),
            HotFocus = new Terminal.Gui.Attribute(Color.Black, Color.Magenta),
            Disabled = new Terminal.Gui.Attribute(Color.DarkGray, Color.Black)
        };

        DefaultScheme = new ColorScheme
        {
            Normal = new Terminal.Gui.Attribute(Color.Gray, Color.Black),
            Focus = new Terminal.Gui.Attribute(Color.Black, Color.Gray),
            HotNormal = new Terminal.Gui.Attribute(Color.Magenta, Color.Black),
            HotFocus = new Terminal.Gui.Attribute(Color.Black, Color.Magenta),
            Disabled = new Terminal.Gui.Attribute(Color.DarkGray, Color.Black)
        };

        // Purple/Magenta accent (VS purple theme color)
        AccentScheme = new ColorScheme
        {
            Normal = new Terminal.Gui.Attribute(Color.Magenta, Color.Black),
            Focus = new Terminal.Gui.Attribute(Color.Black, Color.Magenta),
            HotNormal = new Terminal.Gui.Attribute(Color.BrightMagenta, Color.Black),
            HotFocus = new Terminal.Gui.Attribute(Color.Black, Color.BrightMagenta),
            Disabled = new Terminal.Gui.Attribute(Color.DarkGray, Color.Black)
        };

        SuccessScheme = new ColorScheme
        {
            Normal = new Terminal.Gui.Attribute(Color.BrightGreen, Color.Black),
            Focus = new Terminal.Gui.Attribute(Color.Black, Color.BrightGreen),
            HotNormal = new Terminal.Gui.Attribute(Color.BrightGreen, Color.Black),
            HotFocus = new Terminal.Gui.Attribute(Color.Black, Color.BrightGreen),
            Disabled = new Terminal.Gui.Attribute(Color.DarkGray, Color.Black)
        };

        WarningScheme = new ColorScheme
        {
            Normal = new Terminal.Gui.Attribute(Color.BrightYellow, Color.Black),
            Focus = new Terminal.Gui.Attribute(Color.Black, Color.BrightYellow),
            HotNormal = new Terminal.Gui.Attribute(Color.BrightYellow, Color.Black),
            HotFocus = new Terminal.Gui.Attribute(Color.Black, Color.BrightYellow),
            Disabled = new Terminal.Gui.Attribute(Color.DarkGray, Color.Black)
        };

        ErrorScheme = new ColorScheme
        {
            Normal = new Terminal.Gui.Attribute(Color.BrightRed, Color.Black),
            Focus = new Terminal.Gui.Attribute(Color.Black, Color.BrightRed),
            HotNormal = new Terminal.Gui.Attribute(Color.BrightRed, Color.Black),
            HotFocus = new Terminal.Gui.Attribute(Color.Black, Color.BrightRed),
            Disabled = new Terminal.Gui.Attribute(Color.DarkGray, Color.Black)
        };

        MutedScheme = new ColorScheme
        {
            Normal = new Terminal.Gui.Attribute(Color.DarkGray, Color.Black),
            Focus = new Terminal.Gui.Attribute(Color.Black, Color.DarkGray),
            HotNormal = new Terminal.Gui.Attribute(Color.Gray, Color.Black),
            HotFocus = new Terminal.Gui.Attribute(Color.Black, Color.Gray),
            Disabled = new Terminal.Gui.Attribute(Color.DarkGray, Color.Black)
        };

        TitleScheme = new ColorScheme
        {
            Normal = new Terminal.Gui.Attribute(Color.BrightMagenta, Color.Black),
            Focus = new Terminal.Gui.Attribute(Color.Black, Color.BrightMagenta),
            HotNormal = new Terminal.Gui.Attribute(Color.BrightMagenta, Color.Black),
            HotFocus = new Terminal.Gui.Attribute(Color.Black, Color.BrightMagenta),
            Disabled = new Terminal.Gui.Attribute(Color.DarkGray, Color.Black)
        };

        ButtonScheme = new ColorScheme
        {
            Normal = new Terminal.Gui.Attribute(Color.Gray, Color.Black),
            Focus = new Terminal.Gui.Attribute(Color.Black, Color.Gray),
            HotNormal = new Terminal.Gui.Attribute(Color.Magenta, Color.Black),
            HotFocus = new Terminal.Gui.Attribute(Color.Black, Color.Magenta),
            Disabled = new Terminal.Gui.Attribute(Color.DarkGray, Color.Black)
        };

        PrimaryButtonScheme = new ColorScheme
        {
            Normal = new Terminal.Gui.Attribute(Color.Magenta, Color.Black),
            Focus = new Terminal.Gui.Attribute(Color.Black, Color.Magenta),
            HotNormal = new Terminal.Gui.Attribute(Color.BrightMagenta, Color.Black),
            HotFocus = new Terminal.Gui.Attribute(Color.Black, Color.BrightMagenta),
            Disabled = new Terminal.Gui.Attribute(Color.DarkGray, Color.Black)
        };

        DangerButtonScheme = new ColorScheme
        {
            Normal = new Terminal.Gui.Attribute(Color.BrightRed, Color.Black),
            Focus = new Terminal.Gui.Attribute(Color.Black, Color.BrightRed),
            HotNormal = new Terminal.Gui.Attribute(Color.Red, Color.Black),
            HotFocus = new Terminal.Gui.Attribute(Color.Black, Color.Red),
            Disabled = new Terminal.Gui.Attribute(Color.DarkGray, Color.Black)
        };
    }

    public void Apply()
    {
        Colors.Base = DefaultScheme;
    }
}
