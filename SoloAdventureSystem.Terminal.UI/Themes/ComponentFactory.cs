using Terminal.Gui;
using NStack;

namespace SoloAdventureSystem.UI.Themes;

/// <summary>
/// Factory for creating consistently styled UI components using the current theme
/// </summary>
public static class ComponentFactory
{
    private static ITheme Theme => ThemeManager.Current;

    #region Windows and Containers

    /// <summary>
    /// Creates a themed window
    /// </summary>
    public static Window CreateWindow(string title)
    {
        return new Window(title)
        {
            ColorScheme = Theme.WindowScheme
        };
    }

    /// <summary>
    /// Creates a themed frame view
    /// </summary>
    public static FrameView CreateFrame(string title)
    {
        return new FrameView(title)
        {
            ColorScheme = Theme.WindowScheme
        };
    }

    #endregion

    #region Labels

    /// <summary>
    /// Creates a standard label
    /// </summary>
    public static Label CreateLabel(string text)
    {
        return new Label(text)
        {
            ColorScheme = Theme.DefaultScheme
        };
    }

    /// <summary>
    /// Creates a title/header label
    /// </summary>
    public static Label CreateTitle(string text)
    {
        return new Label(text)
        {
            ColorScheme = Theme.TitleScheme,
            TextAlignment = TextAlignment.Centered
        };
    }

    /// <summary>
    /// Creates an accent label for important information
    /// </summary>
    public static Label CreateAccentLabel(string text)
    {
        return new Label(text)
        {
            ColorScheme = Theme.AccentScheme
        };
    }

    /// <summary>
    /// Creates a muted label for secondary information
    /// </summary>
    public static Label CreateMutedLabel(string text)
    {
        return new Label(text)
        {
            ColorScheme = Theme.MutedScheme
        };
    }

    /// <summary>
    /// Creates a success message label
    /// </summary>
    public static Label CreateSuccessLabel(string text)
    {
        return new Label(text)
        {
            ColorScheme = Theme.SuccessScheme
        };
    }

    /// <summary>
    /// Creates a warning message label
    /// </summary>
    public static Label CreateWarningLabel(string text)
    {
        return new Label(text)
        {
            ColorScheme = Theme.WarningScheme
        };
    }

    /// <summary>
    /// Creates an error message label
    /// </summary>
    public static Label CreateErrorLabel(string text)
    {
        return new Label(text)
        {
            ColorScheme = Theme.ErrorScheme
        };
    }

    #endregion

    #region Buttons

    /// <summary>
    /// Creates a standard button
    /// </summary>
    public static Button CreateButton(string text)
    {
        return new Button(text)
        {
            ColorScheme = Theme.ButtonScheme
        };
    }

    /// <summary>
    /// Creates a primary action button
    /// </summary>
    public static Button CreatePrimaryButton(string text)
    {
        return new Button(text)
        {
            ColorScheme = Theme.PrimaryButtonScheme
        };
    }

    /// <summary>
    /// Creates a danger/destructive action button
    /// </summary>
    public static Button CreateDangerButton(string text)
    {
        return new Button(text)
        {
            ColorScheme = Theme.DangerButtonScheme
        };
    }

    /// <summary>
    /// Creates a success action button
    /// </summary>
    public static Button CreateSuccessButton(string text)
    {
        return new Button(text)
        {
            ColorScheme = Theme.SuccessScheme
        };
    }

    #endregion

    #region Input Controls

    /// <summary>
    /// Creates a themed text field
    /// </summary>
    public static TextField CreateTextField(string text = "")
    {
        return new TextField(text)
        {
            ColorScheme = Theme.DefaultScheme
        };
    }

    /// <summary>
    /// Creates a themed text view
    /// </summary>
    public static TextView CreateTextView()
    {
        return new TextView()
        {
            ColorScheme = Theme.DefaultScheme
        };
    }

    /// <summary>
    /// Creates a themed radio group
    /// </summary>
    public static RadioGroup CreateRadioGroup(ustring[] options)
    {
        return new RadioGroup(options)
        {
            ColorScheme = Theme.DefaultScheme
        };
    }

    /// <summary>
    /// Creates a themed list view
    /// </summary>
    public static ListView CreateListView()
    {
        return new ListView()
        {
            ColorScheme = Theme.AccentScheme
        };
    }

    /// <summary>
    /// Creates a themed list view with data
    /// </summary>
    public static ListView CreateListView(string[] items)
    {
        return new ListView(items)
        {
            ColorScheme = Theme.AccentScheme
        };
    }

    #endregion

    #region Progress and Status

    /// <summary>
    /// Creates a themed progress bar
    /// </summary>
    public static ProgressBar CreateProgressBar()
    {
        return new ProgressBar()
        {
            ColorScheme = Theme.AccentScheme
        };
    }

    #endregion

    #region Layout Helpers

    /// <summary>
    /// Creates a horizontal separator line
    /// </summary>
    public static Label CreateSeparator(int y, int width = 0)
    {
        return new Label()
        {
            X = 0,
            Y = y,
            Width = width > 0 ? width : Dim.Fill(),
            Height = 1,
            Text = new string('?', 100),
            ColorScheme = Theme.MutedScheme
        };
    }

    /// <summary>
    /// Creates a hint/help text label
    /// </summary>
    public static Label CreateHintLabel(string text)
    {
        return new Label()
        {
            Text = text,
            TextAlignment = TextAlignment.Centered,
            ColorScheme = Theme.MutedScheme
        };
    }

    #endregion
}
