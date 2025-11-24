# Model Failure Detection & Recovery System

## Overview

The SoloAdventureSystem now includes automatic detection when an AI model stops working and provides users with recovery options through an interactive dialog.

## How It Works

### 1. Failure Detection

The `LLamaSharpAdapter` tracks consecutive empty outputs from the AI model:

```csharp
private int _consecutiveEmptyOutputs = 0;
private const int MAX_EMPTY_OUTPUTS_BEFORE_FAILURE = 3;
```

When the model returns empty output 3 times in a row, it throws a `ModelStoppedException`:

```csharp
public class ModelStoppedException : Exception
{
    public string ModelKey { get; }
    public int ConsecutiveFailures { get; }
}
```

### 2. User Notification

When a `ModelStoppedException` is caught, the UI displays a modal dialog (`ModelFailureDialog.razor`) that:

- ?? Explains what happened
- ?? Lists common causes
- ?? Provides recovery options

### 3. Recovery Options

#### Option 1: Switch to TinyLlama (Recommended)
- Most stable and tested model
- Automatically reinitializes with TinyLlama
- Best for reliability

#### Option 2: Delete & Re-download
- Deletes potentially corrupted model file
- Forces fresh download on next initialization
- Solves corruption issues

#### Option 3: Try Another Model
- Switch to Phi-3 or Llama-3.2
- Automatically reinitializes
- Tests if issue is model-specific

#### Option 4: Cancel
- Returns to generator
- Allows manual troubleshooting
- Preserves current state

## Implementation Details

### In `LLamaSharpAdapter.cs`

**Before generation methods:**
```csharp
public string GenerateRoomDescription(string context, int seed)
{
    // ... existing code ...
    
    // Check for empty output
    if (string.IsNullOrWhiteSpace(output))
    {
        _consecutiveEmptyOutputs++;
        
        if (_consecutiveEmptyOutputs >= MAX_EMPTY_OUTPUTS_BEFORE_FAILURE)
        {
            throw new ModelStoppedException(
                modelKey, 
                _consecutiveEmptyOutputs,
                "The AI model has stopped generating output.");
        }
    }
    
    // Success - reset counter
    _consecutiveEmptyOutputs = 0;
}
```

### In `WorldGenerator.razor`

**Catching the exception:**
```csharp
try
{
    generatedWorld = await GenerationService.GenerateWorldAsync(options, progress);
}
catch (ModelStoppedException mse)
{
    failedModelKey = mse.ModelKey;
    consecutiveFailures = mse.ConsecutiveFailures;
    showModelFailureDialog = true;
}
```

**Handling model switch:**
```csharp
private async Task HandleModelSwitch(string? newModel)
{
    // Dispose old service
    GenerationService.Dispose();
    
    // Change model
    selectedModel = newModel;
    
    // Reinitialize
    await InitializeAI();
}
```

### In `WorldGenerationService.cs`

**Allowing reinitialization:**
```csharp
public async Task InitializeAsync(...)
{
    // Dispose existing adapter if reinitializing
    if (_isInitialized && _adapter != null)
    {
        _adapter.Dispose();
        _adapter = null;
        _isInitialized = false;
    }
    
    // Initialize with new settings
    _adapter = new LLamaSharpAdapter(...);
    await _adapter.InitializeAsync(progress);
}
```

## User Experience

### Before This Feature
1. ? Model generates empty text
2. ? Generation fails with generic error
3. ? User has no guidance on what to do
4. ? User must manually delete files and restart

### After This Feature
1. ? Model generates empty text 3 times
2. ? System detects the pattern
3. ? Interactive dialog appears
4. ? User selects recovery option
5. ? System automatically fixes the issue

## Error Messages

### Console/Log Output
```
?? Model returned empty output (consecutive failures: 1/3)
?? Model returned empty output (consecutive failures: 2/3)
? Model 'llama-3.2-1b-q4' has stopped working after 3 consecutive failures
```

### UI Dialog
```
?? AI Model Stopped Working

The AI model 'llama-3.2-1b-q4' has stopped generating output.

This typically happens when:
• The model file is corrupted
• The prompt format is incompatible with this model
• The model ran out of context space

[Recovery Options...]
```

## Testing

### Scenario 1: Corrupted Model
1. Corrupt a model file manually
2. Try to generate
3. Verify dialog appears after 3 failures
4. Choose "Delete & Re-download"
5. Verify fresh download succeeds

### Scenario 2: Incompatible Prompt
1. Use a model with wrong prompt format
2. Generate world
3. Verify dialog appears
4. Choose "Switch to TinyLlama"
5. Verify generation succeeds

### Scenario 3: Model-Specific Issue
1. Use problematic model
2. Generate world
3. Verify dialog appears
4. Choose different model
5. Verify new model works

## Configuration

No configuration needed - the feature is automatic. However, you can adjust:

```csharp
// In LLamaSharpAdapter.cs
private const int MAX_EMPTY_OUTPUTS_BEFORE_FAILURE = 3;  // Increase for more tolerance
```

## Benefits

### For Users
- ? Clear explanation of what went wrong
- ? Actionable recovery steps
- ? No need to manually delete files
- ? Automatic model switching
- ? No lost work

### For Developers
- ? Easier debugging (specific exception type)
- ? Better logging
- ? Reduced support requests
- ? Automatic error recovery

### For System
- ? Graceful degradation
- ? Self-healing capability
- ? Better reliability
- ? Improved user retention

## Future Enhancements

### Planned
- [ ] Auto-retry with exponential backoff
- [ ] Model health checks before generation
- [ ] Automatic model file verification
- [ ] Telemetry for failure patterns
- [ ] Fallback model cascade

### Possible
- [ ] Cloud model backup option
- [ ] Automatic prompt format detection
- [ ] Model performance tracking
- [ ] Smart model recommendations
- [ ] Offline model repair tools

## Troubleshooting

### Dialog doesn't appear
- Check console for exceptions
- Verify `ModelStoppedException` is being thrown
- Check if exception is being caught in UI

### Model switch fails
- Verify model file exists
- Check disk space
- Review initialization logs
- Try manual reinitialization

### Repeated failures
- Try TinyLlama (most stable)
- Delete all model files
- Check system RAM
- Review GPU drivers (if using GPU)

## Files Modified

- `SoloAdventureSystem.AIWorldGenerator\Adapters\LLamaSharpAdapter.cs` - Detection logic
- `SoloAdventureSystem.Web.UI\Components\UI\ModelFailureDialog.razor` - Dialog UI
- `SoloAdventureSystem.Web.UI\Components\UI\ModelFailureDialog.razor.css` - Dialog styling
- `SoloAdventureSystem.Web.UI\Components\Pages\WorldGenerator.razor` - Integration
- `SoloAdventureSystem.Web.UI\Services\WorldGenerationService.cs` - Reinitialization support

## Summary

This feature transforms AI model failures from frustrating dead-ends into recoverable situations with clear user guidance. Users get:

1. **Detection** - Automatic failure detection after 3 empty outputs
2. **Explanation** - Clear information about what happened
3. **Options** - Multiple recovery paths
4. **Automation** - One-click fixes
5. **Continuity** - No lost work or configuration

The result is a more robust, user-friendly system that handles AI model issues gracefully.
