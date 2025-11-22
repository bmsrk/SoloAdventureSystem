# Shared Content Directory

This directory contains shared game content used across the Solo Adventure System projects.

## Structure

```
content/
??? worlds/          # Generated world files (.zip)
    ??? World_*.zip  # World archives created by AIWorldGenerator
```

## Worlds Directory

All generated worlds are saved to `{solution-root}/content/worlds/` with the naming convention:
- `World_{WorldName}_{Seed}.zip`

### Example
- `World_MyWorld_12345.zip`
- `World_Cyberpunk_42.zip`

## Usage

### Generating Worlds
Run the AI World Generator:
```bash
cd SoloAdventureSystem.AIWorldGenerator
dotnet run
```

Worlds will be automatically saved to this shared directory.

### Playing Worlds
Run the Terminal GUI:
```bash
cd SoloAdventureSystem.TerminalGUI.UI
dotnet run
```

The game will automatically find and list all worlds in this shared directory.

## Benefits of Shared Directory

1. **Single source of truth** - All projects look in the same location
2. **No duplication** - Worlds are stored once, accessible to all
3. **Easy discovery** - Both generator and game find worlds automatically
4. **Git-friendly** - Can be added to `.gitignore` to avoid committing large world files

## Technical Details

Both projects use `PathHelper.FindSolutionRoot()` to locate the solution directory and then access `content/worlds/`.

If the solution root cannot be found, projects fall back to a local `content/worlds/` directory relative to their current working directory.
