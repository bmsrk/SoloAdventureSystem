# Solo Adventure System - AI World Generator

Generate AI-powered text adventure worlds with embedded offline AI.

## Quick Start

```bash
# Interactive UI (recommended)
dotnet run

# CLI mode
dotnet run --name="MyWorld" --seed=12345 --regions=5
```

## Features

- ?? **Stub Provider** - Instant testing, no setup
- ?? **LLamaSharp** - 100% offline embedded AI
- ?? **Minimalistic UI** - Clean Terminal.Gui interface
- ?? **Cached models** - Download once, use forever
- ?? **Deterministic** - Same seed = same world
- ?? **Shared storage** - Worlds saved to shared `content/worlds/` folder

## Providers

### Stub (Default)
- No setup required
- Instant generation
- Perfect for testing

### LLamaSharp (Embedded AI)
- 100% offline after model download
- Models: Phi-3-mini (2GB), TinyLlama (600MB), Llama-3.2-1B (800MB)
- First run downloads model automatically
- Subsequent runs use cached model

## Output

**Worlds are saved to:** `{solution-root}/content/worlds/World_{Name}_{Seed}.zip`

This shared directory ensures generated worlds are automatically visible to:
- ? The AI World Generator (this project)
- ? The Terminal GUI game (`SoloAdventureSystem.TerminalGUI.UI`)
- ? Any future projects that need access to worlds

### World Archive Contents
Each `.zip` file contains:
- `world.json` - World metadata and configuration
- `rooms/*.json` - Room definitions with descriptions
- `npcs/*.json` - NPC characters with stats and inventory
- `factions/*.json` - Faction information and relationships
- `story/*.yaml` - Story nodes and narrative branches

## Workflow

1. **Generate** worlds using this project (UI or CLI)
2. **Play** worlds using `SoloAdventureSystem.TerminalGUI.UI`
3. All worlds are stored in the shared `content/worlds/` directory

No manual file copying needed!

## Settings

Edit `appsettings.json`:

```json
{
  "AI": {
    "Provider": "Stub",
    "EnableCaching": true
  }
}
```

See [docs/EMBEDDED_SLM_GUIDE.md](../docs/EMBEDDED_SLM_GUIDE.md) for LLamaSharp setup.

