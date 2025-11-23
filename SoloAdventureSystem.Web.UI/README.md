# ?? SoloAdventureSystem.Web.UI

A Blazor Server web application for testing and generating AI-powered game worlds with CUDA acceleration support.

## ? Features

- **Interactive Web Interface** - User-friendly Blazor UI for world generation
- **Real-time Progress Tracking** - Watch model downloads and generation in real-time
- **CUDA GPU Acceleration** - Leverages NVIDIA GPUs for faster inference
- **Multiple AI Models** - Choose between Phi-3, TinyLlama, or Llama-3.2
- **Live Preview** - See generated content immediately
- **Export Functionality** - Download worlds as ZIP files

## ?? Quick Start

### Prerequisites

- .NET 10 SDK
- NVIDIA GPU with CUDA 12.x (optional, but recommended for performance)
- 8GB RAM minimum (16GB recommended)
- 2-3GB free disk space for models

### Running the Application

```bash
# Navigate to the Web UI project
cd SoloAdventureSystem.Web.UI

# Run the application
dotnet run
```

The application will start at `https://localhost:5001` (or the port shown in console).

## ?? Usage

### First-Time Setup

1. **Navigate to AI World Generator** from the menu
2. **Click "Initialize AI"** to download and load the model
   - First run will download the model (~700MB - 2.3GB)
   - Subsequent runs use cached model
3. **Wait for initialization** - Progress shown in real-time

### Generating Worlds

1. **Configure Parameters:**
   - **Name** - Your world name
   - **Seed** - Random seed (same seed = same world)
   - **Theme** - Genre (Cyberpunk, Fantasy, Sci-fi, etc.)
   - **Regions** - Number of rooms/locations (3-20)
   - **Flavor** - Mood/atmosphere
   - **Description** - Setting overview
   - **Main Plot** - Central conflict
   - **Time Period** - Era/timeframe
   - **Power Structure** - Factions/organizations

2. **Click "Generate World"**
3. **Wait for generation** - Takes 1-5 minutes depending on model and GPU
4. **View results** - Preview generated content
5. **Export** - World saved automatically to `content/worlds/`

## ?? Configuration

### appsettings.json

```json
{
  "AI": {
    "Provider": "LLamaSharp",
    "Model": "phi-3-mini-q4",
    "LLamaModelKey": "phi-3-mini-q4",
    "ContextSize": 2048,
    "UseGPU": true,              // Set to false for CPU-only
    "MaxInferenceThreads": 4,    // CPU threads
    "Temperature": 0.7,
    "EnableCaching": true,
    "CacheDirectory": ".aicache",
    "MaxRetries": 3,
    "TimeoutSeconds": 120
  }
}
```

### Available Models

| Model | Size | Speed | Quality | Best For |
|-------|------|-------|---------|----------|
| **phi-3-mini-q4** | ~2.3GB | Medium | Excellent | Best quality output |
| **tinyllama-q4** | ~700MB | Fast | Good | Quick testing, limited hardware |
| **llama-3.2-1b-q4** | ~2GB | Medium | Excellent | Balanced option |

### GPU vs CPU

**With CUDA GPU (Recommended):**
- 5-10x faster generation
- Can handle larger models
- Better for production use
- Requires NVIDIA GPU with CUDA 12.x

**CPU-Only Mode:**
- Works on any hardware
- Slower generation (2-5 min per world)
- Use smaller models (TinyLlama recommended)
- Set `UseGPU: false` in config

## ??? Architecture

### Project Structure

```
SoloAdventureSystem.Web.UI/
??? Components/
?   ??? Layout/
?   ?   ??? NavMenu.razor           # Navigation menu
?   ??? Pages/
?       ??? Home.razor              # Landing page
?       ??? WorldGenerator.razor    # Main generator UI
??? Services/
?   ??? WorldGenerationService.cs   # AI service wrapper
??? Program.cs                      # App configuration
??? appsettings.json               # Configuration
??? SoloAdventureSystem.Web.UI.csproj
```

### Dependencies

- **SoloAdventureSystem.AIWorldGenerator** - AI world generation engine
- **SoloAdventureSystem.Engine** - Game engine models
- **LLamaSharp** - LLM inference
- **LLamaSharp.Backend.Cpu** - CPU backend
- **LLamaSharp.Backend.Cuda12** - CUDA GPU backend

## ?? Performance

### Generation Times (Approximate)

| Model | GPU (CUDA) | CPU Only |
|-------|------------|----------|
| TinyLlama | 30-45 sec | 1-2 min |
| Phi-3 Mini | 1-2 min | 3-5 min |
| Llama-3.2 | 1-2 min | 3-5 min |

*Times for 5-region world. Scales linearly with region count.*

### Memory Usage

| Component | RAM | VRAM (GPU) |
|-----------|-----|------------|
| TinyLlama | ~2GB | ~1GB |
| Phi-3 Mini | ~4GB | ~2.5GB |
| Llama-3.2 | ~4GB | ~2.5GB |

## ?? Troubleshooting

### Model Download Fails

**Issue:** Download progress stalls or errors

**Solutions:**
1. Check internet connection
2. Verify HuggingFace is accessible
3. Try smaller model (TinyLlama)
4. Delete partial downloads from cache directory

### CUDA Not Working

**Issue:** GPU not being used even with `UseGPU: true`

**Solutions:**
1. Verify NVIDIA GPU is present: `nvidia-smi`
2. Check CUDA 12.x is installed
3. Ensure LLamaSharp.Backend.Cuda12 package is installed
4. Check driver version (minimum 525.60 for CUDA 12)

### Out of Memory

**Issue:** Application crashes during generation

**Solutions:**
1. Use smaller model (TinyLlama)
2. Reduce region count
3. Close other applications
4. Switch to CPU mode if GPU VRAM limited

### Slow Generation

**Issue:** Takes too long to generate worlds

**Solutions:**
1. Enable GPU mode if available
2. Use faster model (TinyLlama)
3. Reduce region count (3-5 instead of 10+)
4. Increase CPU threads in config

## ?? Development

### Building

```bash
dotnet build
```

### Running in Development

```bash
dotnet run
```

### Publishing

```bash
# Windows with CUDA
dotnet publish -c Release -r win-x64 --self-contained

# Linux with CUDA
dotnet publish -c Release -r linux-x64 --self-contained

# macOS (CPU only)
dotnet publish -c Release -r osx-x64 --self-contained
```

## ?? Notes

### Model Caching

- Models are downloaded once and cached in `%AppData%\Roaming\SoloAdventureSystem\models\`
- Delete cache to re-download corrupted models
- Cache can be 2-5GB depending on models used

### World Storage

- Generated worlds saved to `content/worlds/` by default
- Each world is a ZIP file with YAML content
- Can be loaded in the main game engine

### Thread Safety

- Service uses `SemaphoreSlim` for thread-safe initialization
- Multiple users can generate worlds simultaneously
- Each generation uses separate random seed

## ?? Future Enhancements

- [ ] Model selection in UI (currently config-based)
- [ ] Batch generation (multiple worlds at once)
- [ ] World comparison/merging
- [ ] Custom templates
- [ ] Save/load generation presets
- [ ] Quality metrics and scoring
- [ ] Live editing of generated content

## ?? Related Documentation

- [Main README](../README.md) - Project overview
- [AI Guide](../docs/AI_GUIDE.md) - AI model details
- [CLI Documentation](../SoloAdventureSystem.CLI/README.md) - Command-line interface

## ?? License

MIT License - See [LICENSE](../LICENSE) for details

## ?? Contributing

Contributions welcome! Please:
1. Fork the repository
2. Create a feature branch
3. Test your changes
4. Submit a pull request

---

**Built with .NET 10, Blazor, and LLamaSharp** ??
