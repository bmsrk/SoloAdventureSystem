# ?? Quick Logging Reference

**Quick reference for using the new logging system**

---

## ?? Log Emoji Guide

| Emoji | Meaning |
|-------|---------|
| ? | Success |
| ? | Error/Failure |
| ?? | Warning |
| ?? | Model/Package |
| ?? | Download |
| ?? | Save/Cache |
| ?? | Start/Initialize |
| ?? | Progress/Time |
| ?? | Check/Search |
| ?? | Stats/Metrics |
| ?? | Target/Goal |
| ?? | Room generation |
| ?? | NPC generation |
| ??? | Faction generation |
| ?? | Tip/Suggestion |

---

## ?? Quick Configuration

### Enable Debug Logging
```json
// appsettings.json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug"
    }
  }
}
```

### Model-Specific Logging
```json
{
  "Logging": {
    "LogLevel": {
      "SoloAdventureSystem.ContentGenerator.EmbeddedModel": "Trace"
    }
  }
}
```

---

## ?? Quick Diagnostics

### Check Model Cache
```csharp
using SoloAdventureSystem.ContentGenerator.Utils;

// Show diagnostic report
Console.WriteLine(ModelDiagnostics.RunDiagnostics());
```

### Cleanup Cache
```csharp
// Remove corrupted/temporary files
var count = ModelDiagnostics.CleanupCache();
Console.WriteLine($"Cleaned {count} file(s)");
```

### Check Cached Models
```csharp
using SoloAdventureSystem.ContentGenerator.EmbeddedModel;

var models = ModelCacheInfo.GetAllCachedModels();
foreach (var model in models)
{
    Console.WriteLine($"{model.ModelKey}: {model.SizeMB:F1} MB - {(model.IsValid ? "?" : "?")}");
}
```

---

## ?? Troubleshooting

### Model Not Persisting?
```bash
1. Run diagnostics: ModelDiagnostics.RunDiagnostics()
2. Check report for issues
3. Cleanup if needed: ModelDiagnostics.CleanupCache()
4. Try again
```

### Download Failing?
```bash
1. Check logs for: "? Failed to download"
2. Look for error details
3. Check internet connection
4. Try smaller model (tinyllama-q4)
```

### Slow Generation?
```bash
1. Check logs for: "? Generated X tokens in Ys (Z tok/s)"
2. If <5 tok/s, try:
   - Smaller model
   - More CPU threads
   - Reduce context size
```

---

## ?? Common Log Patterns

### Successful Cache Hit
```
?? Checking model availability: phi-3-mini-q4
? Using cached model (2048.5 MB) - No download needed!
```

### First Download
```
?? Model not cached. Will download from HuggingFace
?? Downloading model...
?? Download progress: 50% (1024.0/2048.0 MB) at 10.2 MB/s
? Model downloaded and cached successfully
```

### Generation Success
```
?? Generating room description (seed: 12345)
?? Generating text - MaxTokens: 200, Temp: 0.7
? Generated 200 tokens in 20.0s (10.0 tok/s)
```

---

**Full Documentation**: See `LOGGING_GUIDE.md`
