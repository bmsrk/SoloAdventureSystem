# Quick Theme Reference

## Available Themes

### ?? Minimal Theme (Default)
**Colors**: Bright Cyan on Black  
**Mood**: Cyberpunk, Matrix, Futuristic  
**Best For**: Night mode, terminal purists, retro-futuristic aesthetic

**Preview**:
```
Background: ???????? (Black)
Text:       ???????? (Gray)
Accent:     ???????? (Bright Cyan)
Success:    ???????? (Bright Green)
Warning:    ???????? (Bright Yellow)
Error:      ???????? (Bright Red)
```

---

### ?? VS Dark Theme
**Colors**: Purple/Magenta on Black  
**Mood**: Modern, Professional, Visual Studio  
**Best For**: Developers, dark mode enthusiasts, modern UI

**Preview**:
```
Background: ???????? (Black)
Text:       ???????? (Gray)
Accent:     ???????? (Magenta/Purple)
Success:    ???????? (Bright Green)
Warning:    ???????? (Bright Yellow)
Error:      ???????? (Bright Red)
```

---

### ?? VS Light Theme
**Colors**: Blue on Gray/White  
**Mood**: Classic, Professional, Traditional  
**Best For**: Daytime use, accessibility, traditional preferences

**Preview**:
```
Background: ???????? (Gray - simulates white)
Text:       ???????? (Black)
Accent:     ???????? (Blue)
Success:    ???????? (Green)
Warning:    ???????? (Brown)
Error:      ???????? (Red)
```

---

## Theme Comparison

| Feature | Minimal | VS Dark | VS Light |
|---------|---------|---------|----------|
| **Background** | Black | Black | Gray |
| **Text Color** | Gray | Gray | Black |
| **Accent** | Cyan | Purple | Blue |
| **Contrast** | High | High | Medium-High |
| **Eye Strain** | Low (dark) | Low (dark) | Medium (light) |
| **Best Time** | Night | Night | Day |
| **Vibe** | Futuristic | Modern | Classic |

## How to Switch Themes

### During Startup
1. App launches with Title Screen
2. See "?? Theme" section
3. Select desired theme with arrow keys
4. Press Enter on "Apply Theme" button
5. Theme changes immediately
6. Press Enter on "Continue to Main Menu"

### During Use
1. From Main Menu, select "?? Settings"
2. Choose your theme
3. Click "Apply Theme"
4. Click "Continue to Main Menu"

### Via Code
```csharp
using SoloAdventureSystem.UI.Themes;

// Switch to a specific theme
ThemeManager.SetTheme(new MinimalTheme());
ThemeManager.SetTheme(new VSDarkTheme());
ThemeManager.SetTheme(new VSLightTheme());

// Get current theme
var current = ThemeManager.Current;
Console.WriteLine($"Current: {current.Name}");
```

## Theme Design Philosophy

### Minimal Theme
- **Inspiration**: Classic terminal, The Matrix, cyberpunk
- **Goal**: Maximum contrast for readability
- **Key Feature**: Bright cyan creates strong visual hierarchy
- **Use Case**: Long sessions in dark environments

### VS Dark Theme
- **Inspiration**: Visual Studio 2022 dark mode
- **Goal**: Modern development environment feel
- **Key Feature**: Purple accents are easy on eyes
- **Use Case**: Developers who love VS dark theme

### VS Light Theme
- **Inspiration**: Visual Studio 2022 light mode
- **Goal**: Traditional, professional appearance
- **Key Feature**: Blue is professional and calming
- **Use Case**: Well-lit environments, accessibility needs

## Accessibility Notes

### For Low Vision Users
- **Recommended**: Minimal Theme or VS Dark Theme
- **Why**: Highest contrast ratios
- **Tip**: Increase terminal font size for better readability

### For Bright Environments
- **Recommended**: VS Light Theme
- **Why**: Better visibility in sunlight/bright rooms
- **Tip**: Adjust monitor brightness for comfort

### For Long Sessions
- **Recommended**: VS Dark Theme
- **Why**: Purple is gentler than cyan on eyes
- **Tip**: Take regular breaks

## Creating Your Own Theme

Want to create a custom theme? Here's a template:

```csharp
using Terminal.Gui;
using SoloAdventureSystem.UI.Themes;

public class MyCustomTheme : ITheme
{
    public string Name => "My Theme Name";
    public string Description => "Description of my theme";

    // Define all required properties
    public ColorScheme WindowScheme { get; private set; }
    public ColorScheme DefaultScheme { get; private set; }
    public ColorScheme AccentScheme { get; private set; }
    public ColorScheme SuccessScheme { get; private set; }
    public ColorScheme WarningScheme { get; private set; }
    public ColorScheme ErrorScheme { get; private set; }
    public ColorScheme MutedScheme { get; private set; }
    public ColorScheme TitleScheme { get; private set; }
    public ColorScheme ButtonScheme { get; private set; }
    public ColorScheme PrimaryButtonScheme { get; private set; }
    public ColorScheme DangerButtonScheme { get; private set; }

    public MyCustomTheme()
    {
        InitializeColorSchemes();
    }

    private void InitializeColorSchemes()
    {
        // Example: Create a red theme
        DefaultScheme = new ColorScheme
        {
            Normal = new Attribute(Color.Gray, Color.Black),
            Focus = new Attribute(Color.Black, Color.Gray),
            // ... etc
        };
        
        AccentScheme = new ColorScheme
        {
            Normal = new Attribute(Color.BrightRed, Color.Black),
            Focus = new Attribute(Color.Black, Color.BrightRed),
            // ... etc
        };
        
        // Define all other schemes...
    }

    public void Apply()
    {
        Colors.Base = DefaultScheme;
    }
}
```

Then add it to `TitleScreenUI.cs`:
```csharp
_availableThemes.Add(new MyCustomTheme());
```

## Popular Theme Ideas

Here are some theme ideas you could implement:

1. **Solarized Dark** - Popular programmer theme
2. **Dracula** - Purple and pink dark theme
3. **Nord** - Blue-ish nordic theme
4. **Gruvbox** - Retro warm colors
5. **Monokai** - Classic Sublime Text theme
6. **One Dark** - Atom editor theme
7. **Material** - Google Material Design
8. **Cyberpunk Neon** - Bright neons on dark
9. **Matrix Green** - Green on black
10. **High Contrast** - Black/white only for accessibility

## FAQ

**Q: Will my theme choice be saved?**  
A: Not yet, but theme persistence is planned for a future update.

**Q: Can I have different themes for different screens?**  
A: Currently, themes apply globally. Per-screen theming could be added.

**Q: My terminal doesn't show all colors correctly. What should I do?**  
A: Make sure you're using a terminal that supports 16 colors. Most modern terminals do.

**Q: Can I switch themes without restarting?**  
A: Yes! Use the Settings menu to switch themes at any time.

**Q: Which theme is best?**  
A: It's personal preference! Try each one and see which you like best.

---

**Happy Theming! ??**
