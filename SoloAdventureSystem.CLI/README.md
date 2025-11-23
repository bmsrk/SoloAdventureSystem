# ?? SoloAdventureSystem - CLI Tool

Command-line interface for generating AI-powered game worlds.

## ?? Quick Start

### Install
```bash
cd SoloAdventureSystem.CLI
dotnet build
```

### Generate a World
```bash
# Simple generation (uses defaults)
dotnet run generate

# Custom world
dotnet run generate --name "NeonCity" --seed 42069 --regions 10

# Use specific model
dotnet run generate --model tinyllama-q4

# Verbose output
dotnet run generate -v
```

## ?? Commands

### `generate` - Generate a new world

**Options:**

| Option | Alias | Description | Default |
|--------|-------|-------------|---------|
| `--name` | `-n` | World name | `CLIWorld` |
| `--seed` | `-s` | Random seed | Random |
| `--theme` | `-t` | World theme | `Cyberpunk` |
| `--regions` | `-r` | Number of rooms | `5` |
| `--provider` | `-p` | AI provider | `LLamaSharp` |
| `--model` | `-m` | Model name | `phi-3-mini-q4` |
| `--output` | `-o` | Output directory | `content/worlds` |
| `--verbose` | `-v` | Verbose logging | `false` |

**Examples:**

```bash
# Fast generation with Stub (no AI)
dotnet run generate --provider Stub --name TestWorld

# Small model (600MB)
dotnet run generate --model tinyllama-q4 --name TinyWorld

# Large world with many rooms
dotnet run generate --name MegaCity --regions 20 --seed 12345

# Custom output location
dotnet run generate --name MyWorld --output ./my-worlds
```

### `list` - List generated worlds

```bash
dotnet run list
```

Shows all generated worlds with file sizes and creation dates.

### `info` - Show system information

```bash
dotnet run info
```

Displays:
- .NET version
- OS information
- CPU core count
- Directories (worlds, models)
- Cached models and sizes

## ?? AI Models

| Model | Size | Speed | Quality | Command |
|-------|------|-------|---------|---------|
| **Phi-3-mini** | 2GB | ?? | ???? | `--model phi-3-mini-q4` |
| **TinyLlama** | 600MB | ??? | ?? | `--model tinyllama-q4` |
| **Llama-3.2** | 800MB | ?? | ???? | `--model llama-3.2-1b-q4` |
| **Stub** | 0MB | ??? | ? | `--provider Stub` |

**First Use:** Models are downloaded automatically (~2GB). Subsequent uses are instant (cached).

## ?? Output

Generated worlds are saved as `.zip` files containing:
- `world.json` - World metadata
- `rooms/*.json` - Room descriptions
- `npcs/*.json` - NPC data
- `factions/*.json` - Faction information
- `story/*.yaml` - Story nodes
- `system/seed.txt` - Generation seed

## ? Performance

**Typical Generation Times** (5 regions, Phi-3-mini, CPU):

| Hardware | Time |
|----------|------|
| Modern CPU (8+ cores) | ~2-4 min |
| Mid-range CPU (4 cores) | ~5-8 min |
| Older CPU (2 cores) | ~10-15 min |

**First generation** takes longer (model download + load). Subsequent generations are faster (model cached in RAM).

## ?? Configuration

Edit `appsettings.json`:

```json
{
  "AI": {
    "Provider": "LLamaSharp",
    "LLamaModelKey": "phi-3-mini-q4",
    "ContextSize": 2048,
    "UseGPU": false,
    "MaxInferenceThreads": 4
  }
}
```

Or use environment variables:
```bash
export AI__Provider=LLamaSharp
export AI__LLamaModelKey=tinyllama-q4
```

## ?? Troubleshooting

**Model download fails**
```bash
# Check internet connection
# Verify access to huggingface.co
curl -I https://huggingface.co
```

**Out of memory**
```bash
# Use smaller model
dotnet run generate --model tinyllama-q4
```

**Generation timeout**
```bash
# Reduce regions
dotnet run generate --regions 3
```

**Verbose logging**
```bash
# See detailed logs
dotnet run generate -v
```

## ?? File Locations

**Windows:**
- Worlds: `C:\Users\<user>\source\repos\SoloAdventureSystem\content\worlds`
- Models: `C:\Users\<user>\AppData\Roaming\SoloAdventureSystem\models`

**Linux/Mac:**
- Worlds: `~/source/repos/SoloAdventureSystem/content/worlds`
- Models: `~/.config/SoloAdventureSystem/models`

## ?? Examples

### Development/Testing
```bash
# Fast iteration with Stub
dotnet run generate --provider Stub --regions 3
```

### Production Quality
```bash
# High-quality world with Phi-3
dotnet run generate \
  --name "NeonNights" \
  --seed 42069 \
  --regions 15 \
  --model phi-3-mini-q4 \
  --verbose
```

### Batch Generation
```bash
# Generate multiple worlds
for i in {1..5}; do
  dotnet run generate --name "World$i" --seed $RANDOM
done
```

## ?? Output Example

```
??????????????????????????????????????????????????????????????????
?          SoloAdventureSystem - World Generator CLI            ?
??????????????????????????????????????????????????????????????????

?? Configuration:
   Name:     TestWorld
   Seed:     12345
   Theme:    Cyberpunk
   Regions:  5
   Provider: LLamaSharp
   Model:    phi-3-mini-q4

??  Initializing AI adapter...
? Model loaded successfully!

?? Generating world...
? World generated in 142.3s
   Rooms:    5
   NPCs:     5
   Factions: 1
   Lore:     3 entries

?? Validating world structure...
? Validation passed!

?? Exporting world...
? World exported successfully!

?? Output:
   Path: content/worlds/World_TestWorld_12345.zip
   Size: 14.2 KB

?? Sample Content:

   ?? First Room: Neural Nexus
      The Neural Nexus hums with cooling fans, bathed in flickering blue 
      server lights. Rows of black terminals stretch into shadows...

   ?? First NPC: Marcus Chen
      Marcus Chen clawed from street runner to corporate executive through
      cunning and ruthlessness. He champions corporate efficiency...

?? World generation complete!
```

## ?? Next Steps

1. **Generate your first world**: `dotnet run generate`
2. **List worlds**: `dotnet run list`
3. **Play in game**: Load the generated `.zip` in the main UI
4. **Batch generate**: Create multiple worlds with different seeds

## ?? License

MIT License - Same as main project
