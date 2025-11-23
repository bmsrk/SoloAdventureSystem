# ?? Troubleshooting Guide

Common issues and solutions for Solo Adventure System.

---

## Table of Contents
- [Installation Issues](#installation-issues)
- [Model Download Problems](#model-download-problems)
- [Generation Issues](#generation-issues)
- [Performance Problems](#performance-problems)
- [UI Issues](#ui-issues)
- [Advanced Debugging](#advanced-debugging)

---

## Installation Issues

### ? ".NET 10 SDK not found"

**Problem**: Application won't build or run.

**Solution**:
1. Download and install [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
2. Verify installation: `dotnet --version` (should show 10.x.x)
3. Restart your terminal/IDE

### ? "Package restore failed"

**Problem**: NuGet packages won't download.

**Solution**:
```bash
# Clear NuGet cache
dotnet nuget locals all --clear

# Restore packages
dotnet restore

# If still failing, check internet connection and NuGet sources
dotnet nuget list source
```

---

## Model Download Problems

### ? "Failed to download model: Connection timeout"

**Problem**: Model download fails or hangs.

**Solutions**:

1. **Check internet connection**
   ```bash
   ping huggingface.co
   ```

2. **Verify HuggingFace is accessible**
   - Try opening https://huggingface.co in browser
   - Check if your firewall/antivirus is blocking downloads

3. **Use a smaller model first**
   - Try TinyLlama (600MB) instead of Phi-3 (2GB)
   - Smaller downloads are more reliable on slow connections

4. **Resume interrupted downloads**
   - Models are cached in `%APPDATA%\SoloAdventureSystem\models`
   - Delete incomplete `.tmp` files and retry

5. **Manual download** (if automatic fails):
   ```bash
   # Download manually from HuggingFace
   # Phi-3: https://huggingface.co/microsoft/Phi-3-mini-4k-instruct-gguf
   # TinyLlama: https://huggingface.co/TheBloke/TinyLlama-1.1B-Chat-v1.0-GGUF
   
   # Place in: %APPDATA%\SoloAdventureSystem\models\
   # Rename to: phi-3-mini-q4.gguf or tinyllama-q4.gguf
   ```

### ? "Model file is corrupted"

**Problem**: Downloaded model won't load.

**Solution**:
```bash
# Delete corrupted model and re-download
# Windows:
del "%APPDATA%\SoloAdventureSystem\models\*.gguf"

# Linux/Mac:
rm ~/Library/Application\ Support/SoloAdventureSystem/models/*.gguf
```

### ? "Out of disk space during download"

**Problem**: Not enough space for model files.

**Solution**:
1. **Check required space**:
   - Phi-3: ~2.5 GB
   - TinyLlama: ~700 MB
   - Llama-3.2: ~900 MB

2. **Free up space**:
   ```bash
   # Check cache size
   # Windows: %APPDATA%\SoloAdventureSystem\models
   # Linux/Mac: ~/.local/share/SoloAdventureSystem/models
   ```

3. **Use smaller model** or clear old models you don't use

---

## Generation Issues

### ? "World generation hangs/freezes"

**Problem**: Generation appears stuck with no progress.

**Solutions**:

1. **Check CPU usage**
   - Open Task Manager (Windows) or Activity Monitor (Mac)
   - Look for process at ~100% CPU on one core
   - If CPU usage is high, generation is working - just slow (2-5 min is normal)

2. **Wait longer**
   - First generation: 2-5 minutes (normal)
   - Subsequent generations: 1-3 minutes (model is cached)
   - Stub provider: ~2 seconds

3. **Check logs**
   - UI shows progress in the log window
   - Look for "Generating room X/5" messages
   - If no new logs for >5 minutes, something is wrong

4. **Try Stub provider first**
   - Select "Stub (fast)" in provider dropdown
   - Generates in ~2 seconds (for testing)
   - If this works, problem is with AI model

### ? "Generation fails with 'Out of Memory'"

**Problem**: System runs out of RAM during generation.

**Solutions**:

1. **Close other applications**
   - Browser tabs, games, video editors
   - Free up at least 4GB RAM

2. **Use smaller model**:
   - TinyLlama: ~1.5 GB RAM
   - Phi-3: ~3 GB RAM
   - Llama-3.2: ~2.5 GB RAM

3. **Increase page file** (Windows):
   - System Properties ? Advanced ? Performance Settings
   - Advanced ? Virtual Memory ? Change
   - Set custom size: min 4096 MB, max 8192 MB

4. **Monitor memory usage**:
   ```bash
   # Windows: Task Manager ? Performance ? Memory
   # Linux: free -h
   # Mac: Activity Monitor ? Memory
   ```

### ? "LLamaSharp initialization failed"

**Problem**: AI adapter won't load.

**Solutions**:

1. **Check model file**
   ```bash
   # Verify model exists and is complete
   # Windows:
   dir "%APPDATA%\SoloAdventureSystem\models"
   
   # Should show phi-3-mini-q4.gguf (~2GB)
   ```

2. **Verify CPU compatibility**
   - LLamaSharp requires modern CPU (SSE4.1 or newer)
   - Check CPU specs online

3. **Try different model**
   - Switch from Phi-3 to TinyLlama
   - Or use Stub provider

4. **Check logs for details**
   - Look in log window for specific error
   - Search for "LLamaSharp" in logs

5. **Fall back to Stub**
   - Application auto-falls back to Stub on LLamaSharp failure
   - Check if "Falling back to Stub adapter" appears in logs

---

## Performance Problems

### ? "Generation is very slow (>10 minutes)"

**Problem**: World generation takes too long.

**Solutions**:

1. **Use fewer regions**
   - Current default: 5 regions
   - Try reducing to 3 for faster generation

2. **Use faster model**:
   - TinyLlama: Fastest (~30 sec per room)
   - Phi-3: Medium (~45 sec per room)
   - Llama-3.2: Medium (~40 sec per room)

3. **Increase CPU threads** (if you have multi-core CPU):
   ```json
   // In appsettings.json
   {
     "AI": {
       "MaxInferenceThreads": 8  // Increase from 4
     }
   }
   ```

4. **Use Stub for testing**
   - Instant generation (~2 sec)
   - Good for testing game mechanics

### ? "High memory usage (>8GB)"

**Problem**: Application uses too much RAM.

**Solution**:
```csharp
// This is now fixed in latest version
// WorldGeneratorUI implements IDisposable
// Memory is properly released after generation
```

If still seeing high memory:
1. Restart application between generations
2. Use Task Manager to kill orphaned processes
3. Check for memory leaks: File an issue on GitHub

---

## UI Issues

### ? "Terminal UI is garbled/unreadable"

**Problem**: Text overlaps or displays incorrectly.

**Solutions**:

1. **Resize terminal window**
   - Minimum: 100 columns × 30 rows
   - Recommended: 120 columns × 40 rows

2. **Check terminal compatibility**
   - Windows: Use Windows Terminal (not cmd.exe)
   - Linux: Modern terminal (gnome-terminal, konsole, etc.)
   - Mac: Terminal.app works fine

3. **Force redraw**: Resize window

### ? "Can't click buttons / UI not responding"

**Problem**: Mouse/keyboard doesn't work.

**Solutions**:

1. **Use keyboard navigation**:
   - Tab: Move between controls
   - Arrow keys: Navigate lists/radio buttons
   - Enter: Activate button
   - Spacebar: Toggle selection

2. **Check terminal focus**
   - Click in terminal window
   - Ensure it's the active window

3. **Restart application**

### ? "Progress bar doesn't update"

**Problem**: UI appears frozen during generation.

**Solutions**:

1. **Check logs window** - Should show progress messages
2. **Wait longer** - Updates may be slow on some systems
3. **Check CPU usage** - If high, generation is working

---

## Advanced Debugging

### Enable Detailed Logging

```json
// appsettings.Development.json
{
  "Logging": {
    "LogLevel": {
      "Default": "Trace",
      "SoloAdventureSystem": "Trace"
    }
  }
}
```

### Check Generated World Files

```bash
# Windows
%USERPROFILE%\Documents\SoloAdventureSystem\worlds

# Linux
~/Documents/SoloAdventureSystem/worlds

# Mac
~/Documents/SoloAdventureSystem/worlds
```

### Inspect ZIP Contents

```bash
# Extract world ZIP manually
unzip World_MyWorld_12345.zip -d extracted/

# Check structure:
# - world.json (world metadata)
# - rooms/*.json (room definitions)
# - npcs/*.json (NPC data)
# - factions/*.json (faction info)
# - story/*.yaml (story nodes)
```

### Common Error Codes

| Error | Meaning | Solution |
|-------|---------|----------|
| `FileNotFoundException` | Model or world file missing | Re-download model or regenerate world |
| `OutOfMemoryException` | Insufficient RAM | Close apps, use smaller model |
| `InvalidOperationException` | Invalid state | Check logs for context |
| `OperationCanceledException` | User cancelled | Normal - user clicked Cancel |
| `HttpRequestException` | Network error | Check internet, firewall |

### Reset to Defaults

```bash
# Delete all cached data and start fresh

# Windows:
rmdir /s /q "%APPDATA%\SoloAdventureSystem"
rmdir /s /q "%LOCALAPPDATA%\SoloAdventureSystem"
rmdir /s /q "%USERPROFILE%\Documents\SoloAdventureSystem"

# Linux/Mac:
rm -rf ~/.local/share/SoloAdventureSystem
rm -rf ~/.cache/SoloAdventureSystem
rm -rf ~/Documents/SoloAdventureSystem
```

### Report a Bug

If none of the above helps, please file an issue:

1. Go to: https://github.com/bmsrk/SoloAdventureSystem/issues
2. Click "New Issue"
3. Include:
   - Operating System (Windows/Linux/Mac + version)
   - .NET version (`dotnet --version`)
   - What you were doing
   - Error message (exact text)
   - Logs from UI window
   - Steps to reproduce

---

## Performance Tips

### For Best Experience

? **DO**:
- Use Windows Terminal (not cmd.exe) on Windows
- Close unnecessary applications before generating
- Start with Stub provider to test, then try AI
- Use TinyLlama for quick testing
- Use Phi-3 for best quality
- Generate worlds with seed=12345 first (reproducible)

? **DON'T**:
- Use cmd.exe on Windows (Terminal.Gui issues)
- Run multiple generations simultaneously
- Resize window during generation
- Cancel generation repeatedly (wait for completion)

### Hardware Recommendations

| Use Case | RAM | CPU | Disk |
|----------|-----|-----|------|
| **Testing (Stub)** | 4 GB | Any | 500 MB |
| **Light (TinyLlama)** | 8 GB | 2+ cores | 2 GB |
| **Recommended (Phi-3)** | 16 GB | 4+ cores | 5 GB |
| **Best (All models)** | 32 GB | 8+ cores | 10 GB |

---

## Still Need Help?

- ?? Check the [README](README.md) for general info
- ?? Ask in [GitHub Discussions](https://github.com/bmsrk/SoloAdventureSystem/discussions)
- ?? File a [Bug Report](https://github.com/bmsrk/SoloAdventureSystem/issues)
- ?? Contact maintainers via GitHub

---

**Last Updated**: 2025-01-22  
**Version**: 1.0.0
