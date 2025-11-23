# ?? Logging & Diagnostics Guide

Complete guide to logging, debugging, and troubleshooting Solo Adventure System.

---

## ?? Quick Start

### View Logs While Running

The application outputs logs to the console with timestamps and severity levels:

```
10:45:23 ? Using cached model at C:\Users\...\phi-3-mini-q4.gguf (2048.5 MB)
10:45:24 ?? Loading GGUF model from C:\Users\...
10:45:26 ? Model loaded successfully in 2.3s
```

### Log Levels

| Level | When to Use | Example |
|-------|-------------|---------|
| **Trace** | Extremely detailed debugging | Variable values, loop iterations |
| **Debug** | Development debugging | Function entry/exit, intermediate values |
| **Information** | Important events | Model loaded, generation started |
| **Warning** | Recoverable issues | Fallback to Stub adapter |
| **Error** | Failures | Model download failed |
| **Critical** | System failures | Out of memory, corruption |

---

## ?? Logging Configuration

### Default Configuration (`appsettings.json`)

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "System": "Warning",
      "SoloAdventureSystem.ContentGenerator.EmbeddedModel": "Debug",
      "SoloAdventureSystem.ContentGenerator.Adapters": "Debug"
    }
  }
}
```

### Enable Verbose Logging

For maximum detail during troubleshooting:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "SoloAdventureSystem": "Trace"
    }
  }
}
```

### Production Logging

Minimal logging for production:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "SoloAdventureSystem": "Information"
    }
  }
}
```

---

## ?? Understanding Log Messages

### Model Download Logs

```
?? Checking model availability: phi-3-mini-q4
?? Model path: C:\Users\...\phi-3-mini-q4.gguf
? Using cached model (2048.5 MB) - No download needed!
```

**Meaning**: Model is already downloaded and valid.

```
?? Model not cached. Will download from HuggingFace.
?? Downloading model phi-3-mini-q4 from https://...
?? Download progress: 25% (512.0/2048.0 MB) at 5.2 MB/s
? Model downloaded and cached successfully
```

**Meaning**: Model is being downloaded for the first time.

### Model Loading Logs

```
?? Loading GGUF model from C:\Users\...
?? Model file size: 2048.5 MB
?? Model parameters: ContextSize=2048, GpuLayers=0, Threads=4
?? Loading model weights...
? Model weights loaded
?? Creating model context...
? Model context created
? Model loaded successfully in 2.3s
```

**Meaning**: Model successfully loaded into RAM.

### Generation Logs

```
?? Generating text - MaxTokens: 200, Temp: 0.7
?? Generation in progress: 50/200 tokens (10.5 tok/s)
? Generated 200 tokens in 19.0s (10.5 tok/s)
```

**Meaning**: Text generation in progress with performance metrics.

---

## ??? Diagnostic Tools

### Run Model Diagnostics

Add this to your `Program.cs` for diagnostic mode:

```csharp
using SoloAdventureSystem.ContentGenerator.Utils;
using Microsoft.Extensions.Logging;

// Run diagnostics
var factory = LoggerFactory.Create(builder => builder.AddConsole());
var logger = factory.CreateLogger("Diagnostics");

Console.WriteLine(ModelDiagnostics.RunDiagnostics(logger));
```

**Output**:
```
?????????????????????????????????????????????????????????
?         MODEL CACHE DIAGNOSTIC REPORT                 ?
?????????????????????????????????????????????????????????

?? Cache Directory: C:\Users\...\SoloAdventureSystem\models
? Cache directory exists
? Directory is writable

?? Files in cache: 2

Files found:
   • phi-3-mini-q4.gguf
     Size: 2048.5 MB | Age: 2d 5h
   • tinyllama-q4.gguf
     Size: 600.2 MB | Age: 1d 3h

?? Cached Models: 2

   • phi-3-mini-q4 - ? Valid
     Size: 2048.5 MB
     Path: C:\Users\...\phi-3-mini-q4.gguf
     Last Modified: 2025-01-20 15:30:00

   • tinyllama-q4 - ? Valid
     Size: 600.2 MB
     Path: C:\Users\...\tinyllama-q4.gguf
     Last Modified: 2025-01-21 10:15:00

?? Available Disk Space: 45.2 GB
? Sufficient disk space available

???????????????????????????????????????????????????????
SUMMARY:
? No issues detected - model cache is healthy
???????????????????????????????????????????????????????
```

### Cleanup Cache

Remove temporary and corrupted files:

```csharp
var cleanedCount = ModelDiagnostics.CleanupCache(logger);
Console.WriteLine($"Cleaned {cleanedCount} file(s)");
```

---

## ?? Troubleshooting Common Issues

### Issue: "Model not persisting / Re-downloads every time"

**Symptoms**:
- Model downloads every session
- Logs show: "?? Model not cached. Will download..."

**Diagnosis**:
```csharp
// Run diagnostics to check cache
Console.WriteLine(ModelDiagnostics.RunDiagnostics());
```

**Common Causes**:

1. **Cache directory permissions**
   ```
   ? ISSUE: Directory is not writable!
   ?? Check file permissions on the cache directory
   ```
   
   **Fix**: Grant write permissions to `%APPDATA%\SoloAdventureSystem\models`

2. **File size validation failing**
   ```
   ?? Existing model appears corrupted (size: 1024000 bytes, expected ~2100000000 bytes)
   ```
   
   **Fix**: 
   - Delete corrupted file: `ModelDiagnostics.CleanupCache()`
   - Re-download model

3. **Temp file not moved to final location**
   ```
   ? Download complete. Moving to final location...
   ? Verification failed: File does not exist at final location!
   ```
   
   **Fix**: Check if antivirus is blocking file operations

4. **Download interrupted**
   ```
   ??  Found 1 temporary file(s) - may indicate interrupted downloads:
      • phi-3-mini-q4.gguf.tmp (512.5 MB)
   ```
   
   **Fix**: Delete `.tmp` files and retry download

### Issue: "Model loads slowly / High memory usage"

**Symptoms**:
- Model loading takes >10 seconds
- RAM usage >4GB

**Diagnosis**:
Check logs for model loading time:
```
?? Loading GGUF model from ...
?? [15 seconds later]
? Model loaded successfully in 15.3s
```

**Solutions**:

1. **Use smaller model**:
   ```json
   {
     "AI": {
       "LLamaModelKey": "tinyllama-q4"  // 600MB instead of 2GB
     }
   }
   ```

2. **Reduce context size**:
   ```json
   {
     "AI": {
       "ContextSize": 1024  // Instead of 2048
     }
   }
   ```

3. **Increase threads**:
   ```json
   {
     "AI": {
       "MaxInferenceThreads": 8  // Use more CPU cores
     }
   }
   ```

### Issue: "Generation is very slow"

**Symptoms**:
- Tokens/second < 5
- Generation takes >1 minute

**Diagnosis**:
Check logs for generation speed:
```
? Generated 200 tokens in 45.0s (4.4 tok/s)  // ?? Too slow!
```

**Solutions**:

1. **Reduce max tokens**:
   ```json
   {
     "AI": {
       "MaxTokens": 150  // Instead of 200
     }
   }
   ```

2. **Increase CPU threads**:
   ```json
   {
     "AI": {
       "MaxInferenceThreads": 8
     }
   }
   ```

3. **Use smaller model**:
   - TinyLlama: ~15 tok/s
   - Phi-3: ~10 tok/s
   - Llama-3.2: ~12 tok/s

---

## ?? Logging Best Practices

### For Development

Enable trace logging:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Trace"
    }
  }
}
```

### For Testing

Enable debug logging with timestamps:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug"
    },
    "Console": {
      "TimestampFormat": "yyyy-MM-dd HH:mm:ss.fff "
    }
  }
}
```

### For Production

Minimal logging:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "SoloAdventureSystem": "Information"
    }
  }
}
```

---

## ?? Log Message Emojis

| Emoji | Meaning | Example |
|-------|---------|---------|
| ? | Success | Model loaded successfully |
| ? | Error/Failure | Failed to load model |
| ?? | Warning | Model appears corrupted |
| ?? | Package/Model | Checking model availability |
| ?? | Download | Downloading model |
| ?? | Storage/Save | Model cached |
| ?? | Start/Launch | Loading model |
| ?? | Time/Progress | Generation in progress |
| ?? | Search/Check | Model path check |
| ?? | Stats/Metrics | Model file size |
| ?? | Configuration | Model parameters |
| ?? | AI/Model | Creating model context |
| ??? | Delete/Cleanup | Deleted corrupted model |
| ?? | Room | Generating room description |
| ?? | NPC | Generating NPC bio |
| ??? | Faction | Generating faction flavor |
| ?? | Lore | Generating lore entries |
| ?? | Tip/Help | Troubleshooting suggestions |
| ?? | Target/Goal | Cached models count |
| ?? | Tool/Utility | Diagnostic tools |
| ?? | Network/Web | Download URL |
| ?? | Directory | Cache directory path |

---

## ?? Advanced Debugging

### Enable LLamaSharp Native Logging

For deep debugging of model loading issues:

```csharp
// In LLamaInferenceEngine.cs
using LLama.Native;

NativeLibraryConfig.All.WithLogCallback((level, message) =>
{
    _logger?.LogDebug("[LLamaSharp Native] {Level}: {Message}", level, message);
});
```

### Memory Profiling

Monitor memory usage during generation:

```csharp
var before = GC.GetTotalMemory(true);
// ... generation code ...
var after = GC.GetTotalMemory(true);
var usedMB = (after - before) / 1024.0 / 1024.0;
_logger?.LogInformation("Memory used: {UsedMB:F1} MB", usedMB);
```

### Performance Profiling

Track generation performance:

```csharp
var stopwatch = System.Diagnostics.Stopwatch.StartNew();
var result = generator.Generate(options);
stopwatch.Stop();
_logger?.LogInformation("World generation took {Elapsed:F1}s", stopwatch.Elapsed.TotalSeconds);
```

---

## ?? Getting Help

If logs show persistent issues:

1. **Run diagnostics**:
   ```csharp
   Console.WriteLine(ModelDiagnostics.RunDiagnostics());
   ```

2. **Cleanup cache**:
   ```csharp
   ModelDiagnostics.CleanupCache();
   ```

3. **Enable verbose logging** and try again

4. **Copy relevant logs** and file an issue on GitHub

---

**Last Updated**: 2025-01-22  
**Version**: 1.0.0
