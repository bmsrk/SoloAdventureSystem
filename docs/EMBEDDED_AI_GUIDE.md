# Embedded AI Guide - LLamaSharp

Run AI world generation 100% offline with LLamaSharp.

## Setup

### 1. Choose Provider in UI
- Launch: `dotnet run` (in AIWorldGenerator)
- Select: **LLamaSharp (AI)**
- Choose model: **Phi-3-mini Q4 (2GB)** (recommended)

### 2. First Generation
- Click **Generate**
- Model downloads automatically to: `%APPDATA%/SoloAdventureSystem/models/`
- Download: ~2GB, one-time only
- Progress bar shows download speed

### 3. Subsequent Runs
- Uses cached model (instant)
- No re-download needed
- Change models anytime

## Models

| Model | Size | Quality | Speed |
|-------|------|---------|-------|
| Phi-3-mini Q4 | 2GB | ??? | Fast |
| TinyLlama Q4 | 600MB | ?? | Very Fast |
| Llama-3.2-1B Q4 | 800MB | ??? | Fast |

## Troubleshooting

**Model download fails?**
- Check internet connection
- Check disk space (~2-3GB free)
- Restart and try again

**Generation hangs?**
- Wait for model to load (first time only)
- Check log window for progress
- Falls back to Stub if LLamaSharp fails

**Clear cached models:**
```
Delete: %APPDATA%/SoloAdventureSystem/models/
```

## Configuration

`appsettings.json`:
```json
{
  "AI": {
    "Provider": "LLamaSharp",
    "LLamaModelKey": "phi-3-mini-q4",
    "EnableCaching": true
  }
}
```

## Technical Details

- **Backend**: llama.cpp via LLamaSharp
- **Format**: GGUF quantized models
- **Source**: HuggingFace
- **GPU**: Optional (CPU works fine)
- **Privacy**: 100% offline, no telemetry
