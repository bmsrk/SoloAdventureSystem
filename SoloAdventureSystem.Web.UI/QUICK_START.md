# ?? Quick Start - SoloAdventureSystem Web UI

## One-Minute Setup

### 1. Navigate to Project
```bash
cd C:\Users\bruno\source\repos\SoloAdventureSystem\SoloAdventureSystem.Web.UI
```

### 2. Run Application
```bash
dotnet run
```

### 3. Open Browser
Navigate to: `https://localhost:5001`

## First Generation (5 Steps)

1. **Click** "AI World Generator" in menu
2. **Click** "?? Initialize AI" button (one-time, ~2-3GB download)
3. **Wait** for model to download and load
4. **Fill** in world parameters (or use defaults)
5. **Click** "?? Generate World"

That's it! ?

## Configuration (Optional)

### Enable GPU (CUDA)

Edit `appsettings.json`:
```json
{
  "AI": {
    "UseGPU": true
  }
}
```

### Change Model

Options: `phi-3-mini-q4`, `tinyllama-q4`, `llama-3.2-1b-q4`

```json
{
  "AI": {
    "LLamaModelKey": "phi-3-mini-q4"
  }
}
```

## Expected Results

### First Time
- Model download: 2-5 minutes
- Model load: 30-60 seconds
- Generation (5 regions): 1-3 minutes

### Subsequent Times
- Model load: 30 seconds (cached)
- Generation (5 regions): 30 seconds - 2 minutes

## Performance Tips

? **For Best Performance:**
- Use `phi-3-mini-q4` model
- Enable GPU (`UseGPU: true`)
- Start with 5 regions
- Keep regions under 15 for reasonable times

? **For Fastest Results:**
- Use `tinyllama-q4` model
- Enable GPU
- Use 3-5 regions

## Troubleshooting

### Application Won't Start
```bash
# Check .NET version
dotnet --version  # Should be 10.0+

# Restore packages
dotnet restore

# Build
dotnet build
```

### CUDA Not Working
```bash
# Check CUDA
nvidia-smi

# Should show CUDA 12.x
```

### Model Download Fails
- Check internet connection
- Wait and retry
- Try smaller model (`tinyllama-q4`)

## Output Location

Generated worlds saved to:
```
C:\Users\bruno\source\repos\SoloAdventureSystem\content\worlds\
```

Format: `World_{Name}_{Seed}.zip`

## Next Steps

- Read [README.md](README.md) for detailed information
- Check [TESTING_GUIDE.md](TESTING_GUIDE.md) for testing scenarios
- Experiment with different themes and parameters
- Compare model quality and performance

## Support

- GitHub Issues: [Report problems](https://github.com/bmsrk/SoloAdventureSystem/issues)
- Documentation: Check the docs folder
- Logs: View console output for errors

---

**Enjoy generating worlds! ??**
