using Terminal.Gui;

namespace SoloAdventureSystem.ContentGenerator.UI;

/// <summary>
/// Minimalistic monochrome theme
/// </summary>
public static class MinimalTheme
{
    public static ColorScheme Default { get; } = new ColorScheme
    {
        Normal = new Terminal.Gui.Attribute(Color.White, Color.Black),
        Focus = new Terminal.Gui.Attribute(Color.Black, Color.White),
        HotNormal = new Terminal.Gui.Attribute(Color.BrightCyan, Color.Black),
        HotFocus = new Terminal.Gui.Attribute(Color.Black, Color.BrightCyan),
        Disabled = new Terminal.Gui.Attribute(Color.DarkGray, Color.Black)
    };

    public static ColorScheme Accent { get; } = new ColorScheme
    {
        Normal = new Terminal.Gui.Attribute(Color.BrightCyan, Color.Black),
        Focus = new Terminal.Gui.Attribute(Color.Black, Color.BrightCyan),
        HotNormal = new Terminal.Gui.Attribute(Color.Cyan, Color.Black),
        HotFocus = new Terminal.Gui.Attribute(Color.Black, Color.Cyan),
        Disabled = new Terminal.Gui.Attribute(Color.DarkGray, Color.Black)
    };

    public static void Apply()
    {
        Colors.Base = Default;
    }
}
