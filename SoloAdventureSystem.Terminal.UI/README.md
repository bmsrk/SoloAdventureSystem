# Solo Adventure System - Terminal UI

## Overview

A beautiful terminal-based adventure game system with AI-generated worlds, featuring a polished Terminal.GUI interface.

## Features

### ?? Beautiful Main Menu
- Elegant ASCII art title screen
- Clean, minimalist theme with cyan accents
- Easy keyboard navigation
- Smooth user experience

### ?? Interactive Gameplay
- Split-panel interface showing:
  - **Location Description**: Rich text view with room details, NPCs, and items
  - **Action Panel**: Context-aware list of available actions
  - **Status Bar**: Turn counter and inventory count
  - **Command Buttons**: Quick access to inventory, look around, and quit

### ?? World Features
- **Movement**: Navigate through connected locations
- **NPCs**: Talk to characters you encounter
- **Items**: Pick up and manage inventory
- **Dynamic Actions**: Available actions update based on your location

### ?? Main Menu Options

1. **?? Generate New World**
   - Create AI-generated adventure worlds
   - Customizable world parameters
   - Automatic validation and export

2. **?? Play Adventure**
   - Select from available worlds
   - Immersive text-based gameplay
   - Turn-based adventure mechanics

3. **?? Exit**
   - Clean application shutdown

## Controls

### Main Menu
- **?/? Arrow Keys**: Navigate menu options
- **Enter**: Select option
- **Ctrl+C**: Force quit

### In-Game
- **?/? Arrow Keys**: Select actions
- **Enter**: Execute selected action
- **Tab**: Cycle between UI elements
- **Click Buttons**: Use mouse for quick commands

## Gameplay

### Starting a Game
1. Launch the application
2. Select "Play Adventure" from the main menu
3. Choose a world from the list
4. Begin your adventure!

### Actions
The game automatically presents available actions based on your current location:

- **Movement**: Travel to connected locations (North, South, East, West, etc.)
- **Interactions**: Talk to NPCs present in the room
- **Items**: Take items you find in locations
- **Inventory**: View your collected items
- **Look Around**: Re-read the current location description

### Tips
- Talk to NPCs to learn more about the world
- Explore every location to find hidden items
- Pay attention to location connections
- Your progress is not saved, so complete your adventure in one session!

## Visual Design

The UI uses the **Minimal Theme** with:
- **Primary Color**: White on Black (clean, readable)
- **Accent Color**: Cyan (highlights and interactive elements)
- **Focus Color**: Black on White (selected elements)
- **Warning Color**: Yellow (important actions like quit)
- **Success Color**: Green (positive messages)
- **Error Color**: Red (error messages)

## Technical Details

### Built With
- **.NET 10.0**
- **Terminal.Gui 1.19.0** - Cross-platform terminal UI framework
- **Microsoft.Extensions.DependencyInjection** - Service management
- **Microsoft.Extensions.Configuration** - Configuration management

### Architecture
- **Program.cs**: Application entry point and service configuration
- **MainMenuUI.cs**: Main menu presentation
- **GameUI.cs**: Gameplay interface and mechanics
- **GameState.cs**: Game state management
- **WorldSelectorUI.cs**: World selection dialog
- **MinimalTheme.cs**: UI theming and color schemes

### Data Flow
1. Application starts ? Service container configured
2. Main menu displayed ? User selects option
3. World selected ? WorldLoader loads world data
4. Game state initialized ? GameUI renders interface
5. Player actions ? State updates ? UI refreshes

## Future Enhancements

Planned features:
- ?? Save/Load game progress
- ??? Mini-map visualization
- ?? Combat system
- ?? Quest tracking
- ?? Dice roll mechanics
- ?? Achievement system
- ?? Custom themes
- ?? Statistics tracking

## Troubleshooting

### No Worlds Available
- Generate a world first using the "Generate New World" option
- Worlds are stored in the shared worlds directory

### Display Issues
- Ensure your terminal supports Unicode characters
- Try resizing the terminal window
- Use a modern terminal emulator (Windows Terminal, iTerm2, etc.)

### Performance
- The UI is optimized for responsive interaction
- Large world files may take a moment to load
- Consider the terminal window size for best display

## Credits

Created with ?? using Terminal.Gui and .NET 10
