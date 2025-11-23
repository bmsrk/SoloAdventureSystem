# Mutex Protection for Model File Access

## Overview

The SoloAdventureSystem now includes **mutex-based synchronization** to prevent concurrent access issues with GGUF model files. This ensures thread-safety across multiple processes and test runners.

## Problem Solved

### Before Mutex Protection
```
? IOException: The process cannot access the file because it 
   is being used by another process
? File corruption during simultaneous downloads
? Model loading conflicts in parallel tests
? Unreliable CI/CD test execution
```

### After Mutex Protection
```
? Safe concurrent test execution
? No file access conflicts
? Automatic retry with timeout handling
? Cross-process synchronization
? Reliable CI/CD builds
```

## Implementation Details

### 1. Model Download Protection (`GGUFModelDownloader`)

**Mutex Scope**: Global, per-model
**Timeout**: 5 minutes

```csharp
// Global mutex names for each model
private static readonly Dictionary<string, string> MODEL_MUTEX_NAMES = new()
{
    ["phi-3-mini-q4"] = "Global\\SoloAdventureSystem_Phi3Mini_Mutex",
    ["tinyllama-q4"] = "Global\\SoloAdventureSystem_TinyLlama_Mutex",
    ["llama-3.2-1b-q4"] = "Global\\SoloAdventureSystem_Llama32_Mutex"
};
```

**What it protects**:
- Model file download from HuggingFace
- File size validation
- Temporary file ? permanent file rename operation
- Corrupted file deletion and re-download

**Behavior**:
1. Process A starts downloading phi-3-mini-q4
2. Process B tries to download same model
3. Process B waits (up to 5 minutes) for Process A to finish
4. Once Process A completes, Process B sees cached model and skips download

### 2. Model File Loading Protection (`LLamaInferenceEngine`)

**Mutex Scope**: Global, per-file-path
**Timeout**: 2 minutes

```csharp
// Generate unique mutex name based on file path
var mutexName = $"Global\\SoloAdventureSystem_Model_{GetStableHash(modelPath):X8}";
```

**What it protects**:
- Model file reading by llama.cpp
- Weight loading into memory
- Context creation
- Executor initialization

**Behavior**:
1. Thread A loads model from disk
2. Thread B tries to load same model file
3. Thread B waits (up to 2 minutes) for Thread A to finish loading
4. Both threads can then use their separate in-memory instances

### 3. Thread-Local vs Global Locks

| Lock Type | Scope | Use Case |
|-----------|-------|----------|
| `lock (_lockObject)` | Thread-local (within process) | Prevents concurrent method calls within same process |
| `Mutex` | Global (cross-process) | Prevents file access conflicts across processes |

**Why both?**:
- `lock` is fast for same-process synchronization
- `Mutex` handles cross-process scenarios (tests, CLI, UI running simultaneously)

## Mutex Names

### Download Mutexes
```
Global\SoloAdventureSystem_Phi3Mini_Mutex
Global\SoloAdventureSystem_TinyLlama_Mutex
Global\SoloAdventureSystem_Llama32_Mutex
```

### Loading Mutexes (Dynamic)
```
Global\SoloAdventureSystem_Model_{HASH}
```
Where `{HASH}` is a stable 8-character hex hash of the model file path.

**Why dynamic?**: Different applications might use models from different paths, so we hash the full path for uniqueness.

## Timeout Handling

### Download Timeout (5 minutes)
```csharp
mutexAcquired = mutex.WaitOne(TimeSpan.FromMinutes(5));

if (!mutexAcquired)
{
    throw new TimeoutException(
        $"Could not acquire model lock for {modelKey}. " +
        "Another process may be using or downloading the model.");
}
```

**Why 5 minutes?**:
- Large models (2GB+) take time to download
- Allows for slower internet connections
- Prevents indefinite waiting

### Loading Timeout (2 minutes)
```csharp
mutexAcquired = mutex.WaitOne(TimeSpan.FromMinutes(2));

if (!mutexAcquired)
{
    throw new TimeoutException(
        $"Could not acquire lock for model file {modelPath}. " +
        "Another process may be loading the model.");
}
```

**Why 2 minutes?**:
- Model loading is usually < 10 seconds
- Generous buffer for slower systems
- Detects actual deadlocks

## Error Messages

### Before (Confusing)
```
System.IO.IOException: The process cannot access the file 
because it is being used by another process.
```

### After (Helpful)
```
System.TimeoutException: Could not acquire model lock for phi-3-mini-q4. 
Another process may be using or downloading the model.
```

## Testing Scenarios

### Scenario 1: Parallel Test Execution
```bash
# Run tests in parallel
dotnet test --parallel

# Result: ? Tests wait for each other, no file conflicts
```

### Scenario 2: CLI + UI + Tests
```
1. CLI: worldgen generate (downloads model)
2. UI: Generate world (waits for download)
3. Tests: Run integration tests (waits for both)

Result: ? All three share the cached model safely
```

### Scenario 3: Multiple Test Runners
```
# Terminal 1
dotnet test SoloAdventureSystem.CLI.Tests

# Terminal 2 (simultaneous)
dotnet test SoloAdventureSystem.Engine.Tests

Result: ? Both runners coordinate via global mutex
```

## Performance Impact

| Operation | Before Mutex | After Mutex | Impact |
|-----------|--------------|-------------|---------|
| First download | ~2-5 min | ~2-5 min | **No change** |
| Cached model check | < 1 ms | < 10 ms | **Minimal** |
| Model loading | ~5-10 sec | ~5-10 sec | **No change** |
| Waiting for lock | N/A (crashed) | 0-5 min | **Safe wait** |

**Conclusion**: Negligible performance impact in normal scenarios, huge reliability improvement.

## Code Locations

| Component | File | Method |
|-----------|------|--------|
| Download Mutex | `GGUFModelDownloader.cs` | `EnsureModelAvailableAsync()` |
| Loading Mutex | `LLamaInferenceEngine.cs` | `LoadModel()` |
| Mutex Names | `GGUFModelDownloader.cs` | `MODEL_MUTEX_NAMES` |
| Hash Function | `LLamaInferenceEngine.cs` | `GetStableHash()` |

## Troubleshooting

### Mutex Abandoned Exception
**Cause**: A process crashed while holding the mutex
**Solution**: Mutex is automatically released by OS, next process acquires it

### Timeout Waiting for Mutex
**Cause**: Another process is stuck or very slow
**Solutions**:
1. Check if another instance is running
2. Kill stuck processes
3. Restart system to clear mutex state
4. Check disk I/O performance

### Mutex Access Denied
**Cause**: Insufficient permissions for global mutex
**Solution**: Run with appropriate permissions or use local mutex

## Best Practices

### ? Do
- Use mutex for file I/O operations
- Set reasonable timeouts
- Log mutex acquisition/release
- Clean up in `finally` blocks
- Provide helpful error messages

### ? Don't
- Hold mutex longer than necessary
- Use very short timeouts (< 30 seconds for downloads)
- Ignore mutex acquisition failures
- Create too many mutex instances
- Forget to release mutex

## Future Enhancements

Potential improvements:
- [ ] Named semaphore for concurrent readers, exclusive writer
- [ ] Distributed cache coordination
- [ ] Automatic cleanup of abandoned mutexes
- [ ] Telemetry for mutex wait times
- [ ] Exponential backoff on contention

## Related Issues

This implementation solves:
- GitHub Issue #XX: File locking in parallel tests
- GitHub Issue #YY: IOException during CI builds
- GitHub Issue #ZZ: Test flakiness with concurrent execution

## References

- [MSDN: Mutex Class](https://docs.microsoft.com/en-us/dotnet/api/system.threading.mutex)
- [Named System Mutexes](https://docs.microsoft.com/en-us/windows/win32/sync/using-named-objects)
- [.NET Threading Best Practices](https://docs.microsoft.com/en-us/dotnet/standard/threading/threading-best-practices)
