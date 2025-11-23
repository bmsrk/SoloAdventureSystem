# ?? CLI Quick Start Guide

## Generate Your First World in 30 Seconds!

### Option 1: Fast Test (No AI - Instant)
```bash
cd SoloAdventureSystem.CLI
dotnet run -- generate --provider Stub --name "MyFirstWorld"
```

**Result:** Instant world generation with deterministic content.

---

### Option 2: AI-Powered (High Quality)
```bash
cd SoloAdventureSystem.CLI
dotnet run -- generate --name "CyberWorld" --model phi-3-mini-q4
```

**Result:** AI-generated world with rich descriptions (2-5 min first time, faster after model caches).

---

### Option 3: Quick AI (Small Model)
```bash
cd SoloAdventureSystem.CLI
dotnet run -- generate --name "QuickWorld" --model tinyllama-q4 --regions 5
```

**Result:** Fast AI generation with TinyLlama (600MB model).

---

## Common Commands

### List All Generated Worlds
```bash
dotnet run -- list
```

### Show System Info
```bash
dotnet run -- info
```

### Custom World
```bash
dotnet run -- generate \
  --name "NeonCity" \
  --seed 42069 \
  --regions 10 \
  --theme "Cyberpunk" \
  --model phi-3-mini-q4
```

### Verbose Output (See Everything)
```bash
dotnet run -- generate --verbose
```

---

## Output Location

Worlds are saved to:
```
content/worlds/World_<NAME>_<SEED>.zip
```

Example:
```
content/worlds/World_CyberWorld_42069.zip
```

---

## Help

```bash
dotnet run -- generate --help
dotnet run -- list --help
dotnet run -- info --help
```

---

## Troubleshooting

**Model download slow?**
- Use `--model tinyllama-q4` (smaller, faster)

**Out of memory?**
- Use `--model tinyllama-q4`
- Reduce `--regions 3`

**Want faster results?**
- Use `--provider Stub` (instant, no AI)

---

## Full Documentation

See [CLI_IMPLEMENTATION.md](./CLI_IMPLEMENTATION.md) for complete documentation.
