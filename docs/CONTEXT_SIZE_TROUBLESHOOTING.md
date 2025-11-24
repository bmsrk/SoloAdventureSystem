# Context Size & Resource Troubleshooting Guide

## Understanding Context Size

The "context size" is the maximum number of tokens (pieces of text) the AI model can process at once. Think of it as the model's "working memory".

### What Causes Context Size Issues?

1. **Long prompts** - Your system prompts + user prompts are too large
2. **KV Cache Full** - The model's key-value cache runs out of slots
3. **Insufficient BatchSize** - The batch processing buffer is too small

### The `NoKvSlot` Error

```
llama_decode failed: 'NoKvSlot'
```

This error means the model's KV (Key-Value) cache is full and there are no available slots for new tokens.

## Current Configuration

### Default Settings (Updated)

```json
{
  "AI": {
    "ContextSize": 4096,      // Increased from 2048
    "BatchSize": 512,          // Added
    "UBatchSize": 512          // Added
  }
}
```

### What Each Setting Does

**ContextSize** (default: 4096 tokens)
- Total working memory for the model
- Includes prompt + response
- Larger = more memory usage but handles longer interactions
- Recommended: 2048-8192 depending on RAM

**BatchSize** (default: 512)
- Size of batches when processing the prompt
- Larger = faster prompt processing but more memory
- Must be ? ContextSize

**UBatchSize** (default: 512)  
- Microbatch size for generation
- Affects generation speed and memory
- Must be ? BatchSize

## How to Fix Context Issues

### Option 1: Increase Context Size

Edit `appsettings.json`:

```json
{
  "AI": {
    "ContextSize": 8192    // Double the context
  }
}
```

**Pros:**
- ? Handles longer prompts
- ? Fewer KV cache errors

**Cons:**
- ? Uses more RAM
- ? Slower loading

### Option 2: Reduce Prompt Size

The system automatically optimizes prompts, but you can manually reduce them:

**In `PromptOptimizer.cs`:**
```csharp
public string OptimizeSystemPrompt(string prompt)
{
    // Reduce max length from 1500 to 800
    if (prompt.Length > 800)
    {
        prompt = prompt.Substring(0, 800) + "...";
    }
    return prompt;
}
```

### Option 3: Clear KV Cache More Frequently

The system already clears the cache before each generation, but you can add additional clearing:

**In `LLamaInferenceEngine.cs`:**
```csharp
// Already implemented:
_context.NativeHandle.KvCacheClear();
```

### Option 4: Increase Batch Sizes

For systems with more RAM:

```json
{
  "AI": {
    "BatchSize": 1024,
    "UBatchSize": 1024
  }
}
```

## Memory Requirements

### Context Size ? RAM Usage

| ContextSize | Model Size | Approx. RAM Needed |
|-------------|------------|-------------------|
| 2048        | 700MB (TinyLlama) | ~2GB |
| 4096        | 700MB (TinyLlama) | ~3GB |
| 8192        | 700MB (TinyLlama) | ~5GB |
| 2048        | 2.3GB (Phi-3) | ~5GB |
| 4096        | 2.3GB (Phi-3) | ~7GB |
| 8192        | 2.3GB (Phi-3) | ~11GB |

**Formula:** `RAM = ModelSize + (ContextSize × 4 bytes × layers)`

### Checking Your Available RAM

**Windows:**
```powershell
Get-CimInstance Win32_OperatingSystem | Select FreePhysicalMemory
```

**Result:** Available RAM in KB. Divide by 1,024,000 for GB.

## Troubleshooting Steps

### 1. Start Small

Use the smallest viable configuration:

```json
{
  "AI": {
    "Model": "tinyllama-q4",
    "ContextSize": 2048
  }
}
```

### 2. Monitor Generation

Watch the logs for context-related warnings:

```
?? Context usage: 1800/2048 tokens (88%)
?? KV cache near full, clearing...
? NoKvSlot error - context exhausted
```

### 3. Gradually Increase

If you see errors, increase incrementally:

```
2048 ? 3072 ? 4096 ? 6144 ? 8192
```

### 4. Check RAM Usage

While generating, monitor your system:

**Task Manager ? Performance ? Memory**

If RAM usage is > 90%, you need:
- Smaller context size
- Smaller model
- More RAM

## Error Messages & Solutions

### "NoKvSlot"
```
llama_decode failed: 'NoKvSlot'
```

**Cause:** KV cache full
**Solution:**
1. Increase ContextSize to 4096+
2. Increase BatchSize to 1024
3. Reduce prompt complexity

### "Out of Memory"
```
System.OutOfMemoryException
```

**Cause:** Not enough RAM
**Solution:**
1. Use smaller model (TinyLlama)
2. Reduce ContextSize to 2048
3. Close other applications
4. Upgrade RAM

### "Generation Timeout"
```
TimeoutException: Text generation timed out
```

**Cause:** Generation taking too long
**Solution:**
1. Check if context is too large
2. Reduce maxTokens
3. Increase timeout in appsettings.json

## Model-Specific Recommendations

### TinyLlama (700MB)
```json
{
  "Model": "tinyllama-q4",
  "ContextSize": 4096,
  "BatchSize": 512
}
```
- ? Works well with 4GB RAM
- ? Fast generation
- ?? Lower quality output

### Phi-3 Mini (2.3GB)
```json
{
  "Model": "phi-3-mini-q4",
  "ContextSize": 4096,
  "BatchSize": 512
}
```
- ?? Requires 8GB+ RAM
- ? Best quality
- ?? Slower generation

### Llama 3.2 (2GB)
```json
{
  "Model": "llama-3.2-1b-q4",
  "ContextSize": 4096,
  "BatchSize": 512
}
```
- ? Good balance
- ? Works with 6GB RAM
- ? Good quality

## Advanced Configuration

### For High-End Systems (16GB+ RAM)

```json
{
  "AI": {
    "ContextSize": 8192,
    "BatchSize": 1024,
    "UBatchSize": 1024,
    "UseGPU": true,
    "MaxInferenceThreads": 8
  }
}
```

### For Low-End Systems (4GB RAM)

```json
{
  "AI": {
    "Model": "tinyllama-q4",
    "ContextSize": 2048,
    "BatchSize": 256,
    "UBatchSize": 256,
    "UseGPU": false,
    "MaxInferenceThreads": 2
  }
}
```

## Monitoring Context Usage

### In the Logs

Look for these indicators:

```
? Context usage: 512/4096 tokens (12%) - Healthy
?? Context usage: 3500/4096 tokens (85%) - Near limit
? Context usage: 4096/4096 tokens (100%) - Full!
```

### What to Do When Context is Full

1. **Immediately:** KV cache is cleared automatically
2. **Short-term:** Generation continues but may produce errors
3. **Long-term:** Increase ContextSize in configuration

## Performance Tuning

### Optimize for Speed

```json
{
  "ContextSize": 2048,    // Smaller
  "BatchSize": 1024,      // Larger  
  "UBatchSize": 1024      // Larger
}
```

### Optimize for Quality

```json
{
  "ContextSize": 8192,    // Larger
  "BatchSize": 512,       // Moderate
  "Temperature": 0.5      // Lower randomness
}
```

### Optimize for Memory

```json
{
  "ContextSize": 2048,    // Smaller
  "BatchSize": 256,       // Smaller
  "UBatchSize": 256       // Smaller
}
```

## Testing Your Configuration

### 1. Generate a Test World

```bash
dotnet run --project SoloAdventureSystem.Web.UI
```

Navigate to `/generate` and create a small world (3 regions).

### 2. Watch the Logs

Monitor for:
- KV cache warnings
- Memory pressure
- Generation speed
- Output quality

### 3. Adjust and Repeat

Fine-tune based on results:
- Errors? ? Increase ContextSize
- Slow? ? Increase BatchSize
- OOM? ? Decrease ContextSize
- Poor quality? ? Try different model

## Summary

### Quick Reference

| Issue | Solution |
|-------|----------|
| NoKvSlot error | Increase ContextSize |
| Out of memory | Use smaller model or reduce ContextSize |
| Slow generation | Increase BatchSize/UBatchSize |
| Poor quality | Use Phi-3, increase ContextSize |
| Timeout | Increase TimeoutSeconds, check RAM |

### Recommended Defaults

**Most systems:**
```json
{
  "Model": "llama-3.2-1b-q4",
  "ContextSize": 4096,
  "BatchSize": 512,
  "UseGPU": true
}
```

**Low-end systems:**
```json
{
  "Model": "tinyllama-q4",
  "ContextSize": 2048,
  "BatchSize": 256,
  "UseGPU": false
}
```

**High-end systems:**
```json
{
  "Model": "phi-3-mini-q4",
  "ContextSize": 8192,
  "BatchSize": 1024,
  "UseGPU": true
}
```

## Getting Help

If you're still experiencing issues after trying these solutions:

1. **Check logs** for specific error messages
2. **Verify RAM** is sufficient for your configuration
3. **Try TinyLlama** with minimal settings first
4. **Delete and re-download** model files
5. **Report issue** with logs and system specs

---

**Updated:** 2025-01-XX
**Version:** 2.0 with BatchSize configuration
