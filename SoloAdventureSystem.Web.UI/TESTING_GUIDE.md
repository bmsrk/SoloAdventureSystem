# ?? SoloAdventureSystem Web UI - Testing Guide

## Overview

The Web UI provides an interactive interface for testing the AI world generation system with your CUDA-enabled GPU. This guide will help you get started and make the most of the testing capabilities.

## ?? Getting Started

### 1. Launch the Application

```bash
cd SoloAdventureSystem.Web.UI
dotnet run
```

The application will start and display URLs like:
```
Now listening on: https://localhost:5001
Now listening on: http://localhost:5000
```

Open your browser and navigate to `https://localhost:5001`

### 2. First-Time Setup

1. Click **"AI World Generator"** in the navigation menu
2. Click the **"?? Initialize AI"** button
3. Wait for model download (first time only)
   - Progress shown with MB/s and ETA
   - Model cached for future use
4. Once complete, you'll see **"? AI Model Ready!"**

## ?? Generating Your First World

### Basic Parameters

| Parameter | Description | Example |
|-----------|-------------|---------|
| **World Name** | Unique identifier | "NeonCity2089" |
| **Random Seed** | Deterministic generation | 42069 |
| **Theme** | Genre/style | "Cyberpunk" |
| **Regions** | Number of locations | 5 (3-20 recommended) |

### Advanced Parameters

| Parameter | Description | Example |
|-----------|-------------|---------|
| **Flavor/Mood** | Atmosphere | "Dark and gritty with neon highlights" |
| **Description** | Setting overview | "A sprawling megacity..." |
| **Main Plot** | Central conflict | "Uncover conspiracy..." |
| **Time Period** | Era/timeframe | "2089" |
| **Power Structure** | Dominant factions | "Megacorps vs hackers" |

### Example Generation

1. Fill in parameters:
   ```
   Name: CyberWorld
   Seed: 12345
   Theme: Cyberpunk
   Regions: 5
   Flavor: Dark and gritty
   Description: Neon-lit megacity ruled by corporations
   Main Plot: Stop the rogue AI
   Time Period: 2089
   Power Structure: Megacorps, hackers, street gangs
   ```

2. Click **"?? Generate World"**

3. Watch progress:
   - Starting generation... (10%)
   - Generating world structure... (40%)
   - Validating world... (80%)
   - World generation complete! (100%)

4. View results:
   - **Rooms**: 5
   - **NPCs**: 5
   - **Factions**: 1
   - **Lore**: 3 entries

5. Preview generated content
6. Check export path

## ?? Configuration Testing

### Testing Different Models

Edit `appsettings.json` to test different models:

#### Phi-3 Mini (Best Quality)
```json
"AI": {
  "LLamaModelKey": "phi-3-mini-q4",
  "ContextSize": 2048,
  "UseGPU": true
}
```

#### TinyLlama (Fastest)
```json
"AI": {
  "LLamaModelKey": "tinyllama-q4",
  "ContextSize": 2048,
  "UseGPU": true
}
```

#### Llama-3.2 (Balanced)
```json
"AI": {
  "LLamaModelKey": "llama-3.2-1b-q4",
  "ContextSize": 2048,
  "UseGPU": true
}
```

**Note:** Restart the application after changing models.

### Testing GPU vs CPU

#### With CUDA GPU (Recommended)
```json
"AI": {
  "UseGPU": true,
  "MaxInferenceThreads": 4
}
```

#### CPU-Only Mode
```json
"AI": {
  "UseGPU": false,
  "MaxInferenceThreads": 8  // More threads for CPU
}
```

### Performance Testing

Monitor generation times for different configurations:

| Model | GPU | Regions | Expected Time |
|-------|-----|---------|---------------|
| TinyLlama | ? | 5 | 30-45 sec |
| TinyLlama | ? | 5 | 1-2 min |
| Phi-3 | ? | 5 | 1-2 min |
| Phi-3 | ? | 5 | 3-5 min |
| Llama-3.2 | ? | 5 | 1-2 min |
| Llama-3.2 | ? | 5 | 3-5 min |

## ?? Testing Scenarios

### Scenario 1: Quality Comparison

Generate the same world with different models:

```
Seed: 99999 (same for all)
Name: QualityTest
Theme: Cyberpunk
Regions: 5
```

Test with:
1. TinyLlama
2. Phi-3 Mini
3. Llama-3.2

Compare:
- Room description richness
- NPC personality depth
- Faction coherence
- Lore quality

### Scenario 2: Performance Benchmarking

Test generation speed:

```bash
# Test 1: GPU Enabled, 5 regions
UseGPU: true, Regions: 5
Record time: _____ seconds

# Test 2: GPU Enabled, 10 regions
UseGPU: true, Regions: 10
Record time: _____ seconds

# Test 3: CPU Only, 5 regions
UseGPU: false, Regions: 5
Record time: _____ seconds
```

### Scenario 3: Theme Variety

Test different themes with same model:

1. **Cyberpunk**
   ```
   Theme: Cyberpunk
   Flavor: Dark and gritty
   Description: Neon megacity
   ```

2. **Fantasy**
   ```
   Theme: Fantasy
   Flavor: Mystical and ancient
   Description: Medieval kingdom
   ```

3. **Sci-Fi**
   ```
   Theme: Sci-Fi
   Flavor: Sterile and technological
   Description: Space station
   ```

### Scenario 4: Scaling Test

Test with increasing complexity:

| Test | Regions | Expected Rooms | Expected NPCs |
|------|---------|----------------|---------------|
| 1 | 3 | 3 | 3 |
| 2 | 5 | 5 | 5 |
| 3 | 10 | 10 | 10 |
| 4 | 15 | 15 | 15 |
| 5 | 20 | 20 | 20 |

## ?? Monitoring & Debugging

### Browser Developer Tools

Press F12 to open developer tools:

1. **Console Tab**: View JavaScript errors
2. **Network Tab**: Monitor SignalR connections
3. **Performance Tab**: Check rendering performance

### Application Logs

Check the console output for detailed logs:

```
info: SoloAdventureSystem.Web.UI.Services.WorldGenerationService[0]
      Initializing AI adapter with model: phi-3-mini-q4
info: SoloAdventureSystem.ContentGenerator.Adapters.LLamaSharpAdapter[0]
      ?? Initializing LLamaSharp adapter...
```

### Common Issues

#### Model Download Stuck

**Symptoms:**
- Progress stops at X%
- No error message

**Solutions:**
1. Check internet connection
2. Wait 30 seconds
3. Refresh page and retry
4. Check firewall/antivirus

#### Generation Takes Too Long

**Symptoms:**
- Progress stuck at "Generating world structure..."
- More than 5 minutes for 5 regions

**Solutions:**
1. Check GPU utilization (`nvidia-smi`)
2. Verify `UseGPU: true` in config
3. Reduce region count
4. Try smaller model (TinyLlama)

#### CUDA Not Working

**Verify CUDA:**
```bash
nvidia-smi
```

Should show:
- Driver version
- CUDA version (12.x)
- GPU name
- Memory usage

**Check Configuration:**
```json
{
  "AI": {
    "UseGPU": true  // Must be true
  }
}
```

**Check NuGet Packages:**
```xml
<PackageReference Include="LLamaSharp.Backend.Cuda12" Version="0.15.0" />
```

## ?? Testing Checklist

### Initial Setup
- [ ] Application starts successfully
- [ ] Home page loads
- [ ] Navigation menu works
- [ ] AI Generator page accessible

### Model Initialization
- [ ] Initialize button appears
- [ ] Download progress shows
- [ ] Model loads successfully
- [ ] Success message displays

### World Generation
- [ ] Form accepts input
- [ ] Generate button works
- [ ] Progress updates shown
- [ ] World generates successfully
- [ ] Results display correctly
- [ ] Preview content appears

### GPU Acceleration
- [ ] CUDA detected (check logs)
- [ ] GPU memory allocated
- [ ] Generation faster than CPU
- [ ] No CUDA errors

### Content Quality
- [ ] Room descriptions coherent
- [ ] NPC bios interesting
- [ ] Faction descriptions make sense
- [ ] Lore entries relevant

### Export
- [ ] World exports to ZIP
- [ ] File size reasonable
- [ ] ZIP contains expected files
- [ ] Can load in main engine

## ?? Performance Metrics

### Collect These Metrics

1. **Initialization Time**
   - First download: _____ seconds
   - Subsequent loads: _____ seconds

2. **Generation Time** (5 regions)
   - TinyLlama + GPU: _____ seconds
   - TinyLlama + CPU: _____ seconds
   - Phi-3 + GPU: _____ seconds
   - Phi-3 + CPU: _____ seconds

3. **Memory Usage**
   - TinyLlama: _____ GB RAM, _____ GB VRAM
   - Phi-3: _____ GB RAM, _____ GB VRAM

4. **Quality Scores** (1-10)
   - Room descriptions: _____
   - NPC personalities: _____
   - Faction coherence: _____
   - Overall quality: _____

## ?? Advanced Testing

### Custom Prompts

Modify `PromptTemplates.cs` to test different prompt strategies:

```csharp
public static string BuildRoomPrompt(...)
{
    // Test custom prompt here
    return $"Your custom prompt...";
}
```

### Parallel Generation

Open multiple browser tabs to test concurrent generation:

1. Tab 1: Generate with Seed 1
2. Tab 2: Generate with Seed 2
3. Tab 3: Generate with Seed 3

Verify:
- No conflicts
- All complete successfully
- Different outputs (different seeds)

### Stress Testing

Generate many worlds rapidly:

1. Generate world
2. Immediately start next
3. Repeat 10 times
4. Check for memory leaks
5. Verify all exports

## ?? Test Results Template

```markdown
## Test Session: [Date]

### Configuration
- Model: [phi-3-mini-q4 / tinyllama-q4 / llama-3.2-1b-q4]
- GPU: [Enabled / Disabled]
- CUDA Version: [12.x]
- GPU Model: [RTX 4090 / etc]

### Test 1: Basic Generation
- Seed: 12345
- Regions: 5
- Time: _____ seconds
- Quality: _____ / 10
- Notes: _____

### Test 2: Large World
- Seed: 67890
- Regions: 15
- Time: _____ seconds
- Quality: _____ / 10
- Notes: _____

### Issues Found
1. _____
2. _____

### Recommendations
1. _____
2. _____
```

## ?? Learning Tips

1. **Start Small**: Begin with 3-5 regions to understand the system
2. **Same Seed Testing**: Use same seed to compare model quality
3. **Monitor GPU**: Keep `nvidia-smi` open to watch GPU usage
4. **Compare Exports**: Extract and compare generated YAML files
5. **Document Findings**: Keep notes on what works best

## ?? Additional Resources

- [LLamaSharp Documentation](https://github.com/SciSharp/LLamaSharp)
- [CUDA Toolkit Guide](https://docs.nvidia.com/cuda/)
- [Blazor Documentation](https://learn.microsoft.com/en-us/aspnet/core/blazor/)

---

**Happy Testing! ??**

If you encounter issues or have suggestions, please document them and share with the team.
