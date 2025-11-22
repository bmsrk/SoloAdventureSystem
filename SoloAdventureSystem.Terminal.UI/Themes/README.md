# Design System

This document describes the unified design system used across all Terminal UI components in the Solo Adventure System.

## Overview

The design system provides a consistent, themeable interface across the entire application with the following key components:

- **ITheme**: Interface defining all color schemes and theme properties
- **ThemeManager**: Manages and applies themes throughout the application
- **ComponentFactory**: Creates consistently styled UI components
- **MinimalTheme**: Default high-contrast theme implementation

## Architecture

### Theme Interface (`ITheme`)

Defines the contract for all themes:

```csharp
public interface ITheme
{
    string Name { get; }
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
    
    void Apply();
}
```

### Theme Manager

Central theme management:

```csharp
// Get current theme
var theme = ThemeManager.Current;

// Switch themes
ThemeManager.SetTheme(new MinimalTheme());

// Apply current theme
ThemeManager.ApplyCurrentTheme();
```

### Component Factory

Create consistently styled components:

```csharp
// Windows and containers
var window = ComponentFactory.CreateWindow("Title");
var frame = ComponentFactory.CreateFrame("Frame Title");

// Labels
var label = ComponentFactory.CreateLabel("Text");
var title = ComponentFactory.CreateTitle("Header");
var accent = ComponentFactory.CreateAccentLabel("Important!");
var muted = ComponentFactory.CreateMutedLabel("Secondary info");
var success = ComponentFactory.CreateSuccessLabel("Success!");
var warning = ComponentFactory.CreateWarningLabel("Warning!");
var error = ComponentFactory.CreateErrorLabel("Error!");

// Buttons
var button = ComponentFactory.CreateButton("Standard");
var primary = ComponentFactory.CreatePrimaryButton("Primary Action");
var danger = ComponentFactory.CreateDangerButton("Delete");
var success = ComponentFactory.CreateSuccessButton("Confirm");

// Input controls
var textField = ComponentFactory.CreateTextField("default text");
var textView = ComponentFactory.CreateTextView();
var radio = ComponentFactory.CreateRadioGroup(new[] { "Option 1", "Option 2" });
var list = ComponentFactory.CreateListView();

// Progress
var progress = ComponentFactory.CreateProgressBar();

// Layout helpers
var separator = ComponentFactory.CreateSeparator(y: 5);
var hint = ComponentFactory.CreateHintLabel("Press Enter to continue");
```

## Color Schemes

### MinimalTheme Color Palette

| Scheme | Foreground | Use Case |
|--------|-----------|----------|
| **Window** | Gray | Window borders and frames |
| **Default** | Gray | Standard text and controls |
| **Accent** | BrightCyan | Highlights, selected items, actions |
| **Success** | BrightGreen | Success messages, confirmations |
| **Warning** | BrightYellow | Warnings, caution messages |
| **Error** | BrightRed | Errors, critical messages |
| **Muted** | DarkGray | Secondary information, hints |
| **Title** | BrightCyan | Headers, section titles |
| **Button** | Gray | Standard buttons |
| **PrimaryButton** | BrightCyan | Primary action buttons |
| **DangerButton** | BrightRed | Destructive action buttons |

All schemes use Black as the background color for maximum contrast.

## Creating a New Theme

To create a new theme:

1. Implement the `ITheme` interface:

```csharp
public class DarkBlueTheme : ITheme
{
    public string Name => "Dark Blue";
    public string Description => "Professional dark blue theme";
    
    public ColorScheme WindowScheme { get; private set; }
    public ColorScheme DefaultScheme { get; private set; }
    // ... implement all required properties
    
    public DarkBlueTheme()
    {
        InitializeColorSchemes();
    }
    
    private void InitializeColorSchemes()
    {
        // Define your color schemes
        DefaultScheme = new ColorScheme
        {
            Normal = new Attribute(Color.Cyan, Color.Blue),
            // ... etc
        };
    }
    
    public void Apply()
    {
        Colors.Base = DefaultScheme;
    }
}
```

2. Apply the theme:

```csharp
ThemeManager.SetTheme(new DarkBlueTheme());
```

## UI Guidelines

### Window Layout Pattern

Standard layout for all screens:

```csharp
Application.Init();
try
{
    ThemeManager.ApplyCurrentTheme();
    var top = Application.Top;
    
    var win = ComponentFactory.CreateWindow("Title")
    {
        X = 0, Y = 0,
        Width = Dim.Fill(),
        Height = Dim.Fill()
    };
    
    // Title at top
    var title = ComponentFactory.CreateTitle("?? Screen Title")
    {
        X = Pos.Center(),
        Y = 1
    };
    
    // Content in the middle
    // ...
    
    // Actions at bottom
    var okBtn = ComponentFactory.CreatePrimaryButton("OK")
    {
        X = 1,
        Y = Pos.AnchorEnd(1)
    };
    
    win.Add(title, /* ... content ..., */ okBtn);
    top.Add(win);
    Application.Run();
}
finally
{
    Application.Shutdown();
}
```

### Component Spacing

- Use `X = 1` for left margin (1 space from edge)
- Use `Y = 1` spacing between major sections
- Use `Width = Dim.Fill(1)` to leave 1 space margin on right
- Use `Height = Dim.Fill(3)` to leave space for buttons at bottom

### Icon Usage

Consistent emoji icons across the application:

- ?? - World generation
- ?? - Play/Adventure
- ?? - Location
- ?? - NPCs/People
- ?? - Items/Inventory
- ?? - Look/Examine
- ?? - Quit/Exit
- ?? - Configuration
- ?? - Progress/Stats
- ? - Success
- ? - Error
- ?? - Warning
- ?? - Start/Generate/Select
- ?? - Back/Cancel

## Best Practices

1. **Always use ComponentFactory**: Never create Terminal.Gui components directly
2. **Apply theme on init**: Call `ThemeManager.ApplyCurrentTheme()` in every screen
3. **Use semantic names**: Choose the right factory method for the component's purpose
4. **Consistent button placement**: Primary actions first, cancel/back last
5. **Clear visual hierarchy**: Title ? Content ? Actions
6. **Helpful hints**: Use `CreateHintLabel()` for keyboard shortcuts and instructions

## Example Screen

```csharp
public class MyScreen
{
    public void Show()
    {
        Application.Init();
        try
        {
            ThemeManager.ApplyCurrentTheme();
            var top = Application.Top;
            
            var win = ComponentFactory.CreateWindow("My Screen")
            {
                X = 0, Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill()
            };
            
            var title = ComponentFactory.CreateTitle("?? Welcome")
            {
                X = Pos.Center(),
                Y = 2
            };
            
            var description = ComponentFactory.CreateLabel("Choose an option:")
            {
                X = 1,
                Y = 5
            };
            
            var list = ComponentFactory.CreateListView(new[] { "Option 1", "Option 2" })
            {
                X = 1,
                Y = 7,
                Width = Dim.Fill(1),
                Height = Dim.Fill(5)
            };
            
            var hint = ComponentFactory.CreateHintLabel("?? Select • Enter to confirm")
            {
                X = Pos.Center(),
                Y = Pos.AnchorEnd(3)
            };
            
            var okBtn = ComponentFactory.CreatePrimaryButton("OK")
            {
                X = 1,
                Y = Pos.AnchorEnd(1)
            };
            
            var cancelBtn = ComponentFactory.CreateButton("Cancel")
            {
                X = Pos.Right(okBtn) + 2,
                Y = Pos.AnchorEnd(1)
            };
            
            win.Add(title, description, list, hint, okBtn, cancelBtn);
            top.Add(win);
            Application.Run();
        }
        finally
        {
            Application.Shutdown();
        }
    }
}
```

## Future Enhancements

Potential improvements to the design system:

1. **Theme persistence**: Save/load user's theme preference
2. **Dynamic theme switching**: Hot-reload themes without restart
3. **Additional themes**: Light theme, High-contrast theme, Retro theme
4. **Theme editor**: UI for creating custom themes
5. **Accessibility**: Color-blind friendly themes
6. **Component variants**: Additional button styles, list styles, etc.
