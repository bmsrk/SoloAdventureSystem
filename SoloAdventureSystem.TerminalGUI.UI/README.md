# Solo Adventure System - Terminal UI

Play AI-generated text adventure worlds.

## Quick Start

```bash
dotnet run
```

## Controls

- **Arrow keys** - Navigate menus
- **Enter** - Select/Confirm
- **Type commands** - `go north`, `examine door`, `talk to npc`
- **Quit** - Type `quit` or close window

## Loading Worlds

Worlds loaded from: `{solution}/content/worlds/*.zip`

Generate worlds using: `SoloAdventureSystem.AIWorldGenerator`

## Commands

```
Movement:    go [direction], north, south, east, west
Interaction: examine [object], talk to [npc], take [item]
Inventory:   inventory, use [item]
System:      help, quit
```

## Troubleshooting

**No worlds found?**
- Generate worlds first using AIWorldGenerator
- Check `content/worlds/` folder exists
- Ensure world files are `.zip` format

**Can't load world?**
- Check console for error messages
- Verify ZIP file isn't corrupted
- Re-generate the world
