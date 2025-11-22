using Terminal.Gui;

namespace SoloAdventureSystem.UI.Themes;

/// <summary>
/// Visual Studio inspired light theme with blue accents
/// </summary>
public class VSLightTheme : ITheme
{
    public string Name => "VS Light";
    public string Description => "Visual Studio inspired light theme with blue accents";

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

    public VSLightTheme()
    {
        InitializeColorSchemes();
    }

    private void InitializeColorSchemes()
    {
        // VS Light uses dark text on light background (Gray background simulates white)
        WindowScheme = new ColorScheme
        {
            Normal = new Terminal.Gui.Attribute(Color.Black, Color.Gray),
            Focus = new Terminal.Gui.Attribute(Color.Blue, Color.Gray),
            HotNormal = new Terminal.Gui.Attribute(Color.Blue, Color.Gray),
            HotFocus = new Terminal.Gui.Attribute(Color.Gray, Color.Blue),
            Disabled = new Terminal.Gui.Attribute(Color.DarkGray, Color.Gray)
        };

        DefaultScheme = new ColorScheme
        {
            Normal = new Terminal.Gui.Attribute(Color.Black, Color.Gray),
            Focus = new Terminal.Gui.Attribute(Color.Gray, Color.Black),
            HotNormal = new Terminal.Gui.Attribute(Color.Blue, Color.Gray),
            HotFocus = new Terminal.Gui.Attribute(Color.Gray, Color.Blue),
            Disabled = new Terminal.Gui.Attribute(Color.DarkGray, Color.Gray)
        };

        // Blue accent (VS blue theme color)
        AccentScheme = new ColorScheme
        {
            Normal = new Terminal.Gui.Attribute(Color.Blue, Color.Gray),
            Focus = new Terminal.Gui.Attribute(Color.Gray, Color.Blue),
            HotNormal = new Terminal.Gui.Attribute(Color.BrightBlue, Color.Gray),
            HotFocus = new Terminal.Gui.Attribute(Color.Gray, Color.BrightBlue),
            Disabled = new Terminal.Gui.Attribute(Color.DarkGray, Color.Gray)
        };

        SuccessScheme = new ColorScheme
        {
            Normal = new Terminal.Gui.Attribute(Color.Green, Color.Gray),
            Focus = new Terminal.Gui.Attribute(Color.Gray, Color.Green),
            HotNormal = new Terminal.Gui.Attribute(Color.Green, Color.Gray),
            HotFocus = new Terminal.Gui.Attribute(Color.Gray, Color.Green),
            Disabled = new Terminal.Gui.Attribute(Color.DarkGray, Color.Gray)
        };

        WarningScheme = new ColorScheme
        {
            Normal = new Terminal.Gui.Attribute(Color.Brown, Color.Gray),
            Focus = new Terminal.Gui.Attribute(Color.Gray, Color.Brown),
            HotNormal = new Terminal.Gui.Attribute(Color.Brown, Color.Gray),
            HotFocus = new Terminal.Gui.Attribute(Color.Gray, Color.Brown),
            Disabled = new Terminal.Gui.Attribute(Color.DarkGray, Color.Gray)
        };

        ErrorScheme = new ColorScheme
        {
            Normal = new Terminal.Gui.Attribute(Color.Red, Color.Gray),
            Focus = new Terminal.Gui.Attribute(Color.Gray, Color.Red),
            HotNormal = new Terminal.Gui.Attribute(Color.Red, Color.Gray),
            HotFocus = new Terminal.Gui.Attribute(Color.Gray, Color.Red),
            Disabled = new Terminal.Gui.Attribute(Color.DarkGray, Color.Gray)
        };

        MutedScheme = new ColorScheme
        {
            Normal = new Terminal.Gui.Attribute(Color.DarkGray, Color.Gray),
            Focus = new Terminal.Gui.Attribute(Color.Gray, Color.DarkGray),
            HotNormal = new Terminal.Gui.Attribute(Color.Black, Color.Gray),
            HotFocus = new Terminal.Gui.Attribute(Color.Gray, Color.Black),
            Disabled = new Terminal.Gui.Attribute(Color.DarkGray, Color.Gray)
        };

        TitleScheme = new ColorScheme
        {
            Normal = new Terminal.Gui.Attribute(Color.Blue, Color.Gray),
            Focus = new Terminal.Gui.Attribute(Color.Gray, Color.Blue),
            HotNormal = new Terminal.Gui.Attribute(Color.BrightBlue, Color.Gray),
            HotFocus = new Terminal.Gui.Attribute(Color.Gray, Color.BrightBlue),
            Disabled = new Terminal.Gui.Attribute(Color.DarkGray, Color.Gray)
        };

        ButtonScheme = new ColorScheme
        {
            Normal = new Terminal.Gui.Attribute(Color.Black, Color.Gray),
            Focus = new Terminal.Gui.Attribute(Color.Gray, Color.Black),
            HotNormal = new Terminal.Gui.Attribute(Color.Blue, Color.Gray),
            HotFocus = new Terminal.Gui.Attribute(Color.Gray, Color.Blue),
            Disabled = new Terminal.Gui.Attribute(Color.DarkGray, Color.Gray)
        };

        PrimaryButtonScheme = new ColorScheme
        {
            Normal = new Terminal.Gui.Attribute(Color.Blue, Color.Gray),
            Focus = new Terminal.Gui.Attribute(Color.Gray, Color.Blue),
            HotNormal = new Terminal.Gui.Attribute(Color.BrightBlue, Color.Gray),
            HotFocus = new Terminal.Gui.Attribute(Color.Gray, Color.BrightBlue),
            Disabled = new Terminal.Gui.Attribute(Color.DarkGray, Color.Gray)
        };

        DangerButtonScheme = new ColorScheme
        {
            Normal = new Terminal.Gui.Attribute(Color.Red, Color.Gray),
            Focus = new Terminal.Gui.Attribute(Color.Gray, Color.Red),
            HotNormal = new Terminal.Gui.Attribute(Color.BrightRed, Color.Gray),
            HotFocus = new Terminal.Gui.Attribute(Color.Gray, Color.BrightRed),
            Disabled = new Terminal.Gui.Attribute(Color.DarkGray, Color.Gray)
        };
    }

    public void Apply()
    {
        Colors.Base = DefaultScheme;
    }
}
