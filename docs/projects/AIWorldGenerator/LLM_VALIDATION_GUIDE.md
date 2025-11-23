# LLM-Based World Validation

## Overview

The `WorldValidator` has been enhanced with **LLM-based quality validation** using your existing LLamaSharp infrastructure. This feature validates that generated worlds are not just structurally valid, but also **high-quality, coherent, and engaging**.

## Features

### 1. **Structural Validation** (Always Runs)
Basic checks that ensure the world is well-formed:
- ? Verifies world.json exists
- ? Ensures minimum 3 rooms
- ? Ensures at least 1 faction
- ? Ensures at least 1 story node

### 2. **Quality Validation** (LLM-Powered)
Advanced checks using LLM prompts to score content quality:

#### Room Description Quality (0-100)
- **Vividness & Immersion** (30 points): Are descriptions engaging and atmospheric?
- **Theme Consistency** (25 points): Does it match the Cyberpunk theme?
- **Sensory Details** (25 points): Are specific sights, sounds, smells mentioned?
- **Writing Quality** (20 points): Is it well-written and compelling?

#### NPC Biography Quality (0-100)
- **Personality** (30 points): Clear, distinct personality?
- **Theme Consistency** (25 points): Fits the Cyberpunk theme?
- **Backstory** (25 points): Interesting history and motivation?
- **Memorability** (20 points): Is the character memorable?

#### Faction Lore Quality (0-100)
- **Goals & Ideology** (30 points): Clear faction purpose?
- **Theme Consistency** (25 points): Matches the world theme?
- **Conflicts** (25 points): Interesting conflicts and tensions?
- **Coherence** (20 points): Well-written and logical?

#### Consistency Checks (0-100)
- **Room Connectivity**: All rooms reachable from start?
- **Faction References**: All NPCs reference valid factions?
- **NPC References**: All room NPCs exist in the world?

### 3. **Validation Results**
The validator returns a `ValidationResult` with:
- `IsValid`: Overall pass/fail
- `Errors`: Critical issues that should block saving
- `Warnings`: Quality concerns that may be acceptable
- `Metrics`: Detailed scores for each category

## How to Use

### In Code

```csharp
// Basic structural validation (always use this)
var validator = new WorldValidator();
validator.Validate(worldResult); // Throws on structural issues

// Quality validation with LLM (requires SLM adapter)
var llamaAdapter = new LLamaSharpAdapter(settings, logger);
await llamaAdapter.InitializeAsync();

var qualityValidator = new WorldValidator(llamaAdapter, logger);
var qualityResult = qualityValidator.ValidateQuality(worldResult, "Cyberpunk");

if (!qualityResult.IsValid)
{
    Console.WriteLine($"Quality Score: {qualityResult.Metrics.OverallScore}/100");
    foreach (var warning in qualityResult.Warnings)
    {
        Console.WriteLine($"?? {warning}");
    }
}
```

### In the UI (WorldGeneratorUI)

The validation runs **automatically** when using the LLamaSharp provider:

1. **Generate a world** with LLamaSharp provider selected
2. After generation completes, **quality validation runs automatically**
3. You'll see quality scores in the log:
   ```
   Quality Scores: Rooms=85, NPCs=78, Factions=92, Overall=83/100
   ```
4. If quality issues are detected, you'll get a **confirmation dialog**:
   ```
   Quality Issues Detected
   
   Overall Score: 58/100
   Warnings: 3
   
   Continue saving anyway?
   [Save Anyway] [Cancel]
   ```

### Validation Thresholds

- **Overall Score < 50**: Considered low quality, triggers warning dialog
- **Score 50-70**: Acceptable but may have issues (warnings logged)
- **Score 70-85**: Good quality
- **Score 85+**: Excellent quality

## Implementation Details

### Validation Prompts

The validator uses specialized prompts to evaluate content quality:

```csharp
// Example: Room validation prompt
"Rate this room description on a scale of 0-100 based on quality criteria:
- Is it vivid and immersive? (30 points)
- Does it match the Cyberpunk theme? (25 points)
- Does it include specific sensory details? (25 points)
- Is it well-written and engaging? (20 points)

Room Name: Data Vault
Description: Humming servers fill the room...

Respond with ONLY a number 0-100, nothing else:"
```

The LLM evaluates the content and returns a score, which is parsed and aggregated.

### Performance Considerations

- **Only validates first 3 items** of each category (rooms, NPCs) to save time
- **Reuses the cached LLM adapter** (no model reloading)
- **Quality validation is optional** - only runs with LLamaSharp provider
- **Stub provider skips quality checks** (instant validation)

### Fallback Behavior

- If LLM evaluation fails, defaults to score of 70 (acceptable)
- If no SLM adapter provided, structural validation still runs
- Warnings are logged but don't block generation

## Example Output

### Good Quality World
```
?? Starting world validation...
? Basic structural validation passed
?? Starting LLM-based quality validation...
?? Validating room descriptions...
?? Validating NPC biographies...
??? Validating faction lore...
?? Validating world consistency...
?? Quality Scores: Rooms=88, NPCs=82, Factions=91, Consistency=100, Overall=90
? World quality validation PASSED
```

### Low Quality World
```
?? Starting world validation...
? Basic structural validation passed
?? Starting LLM-based quality validation...
?? Validating room descriptions...
?? Room 'Data Room' quality issue: 45/100
?? Validating NPC biographies...
?? NPC 'John Smith' quality issue: 38/100
?? Validating world consistency...
?? Found 2 disconnected rooms
?? Quality Scores: Rooms=52, NPCs=48, Factions=75, Consistency=80, Overall=63
? World quality validation PASSED (with warnings)

Quality Warnings (3):
  ?? Room 'Data Room' has low quality score: 45/100
  ?? NPC 'John Smith' has low quality score: 38/100
  ?? Found 2 disconnected rooms
```

## Testing

Run the validator tests:
```bash
dotnet test --filter WorldValidatorTests
```

Tests cover:
- ? Basic structural validation
- ? Quality validation with/without SLM
- ? Consistency checks (connectivity, references)
- ? Score calculation and thresholds

## Future Enhancements

Potential improvements:
- [ ] Add validation for lore entries
- [ ] Check for contradictions between content
- [ ] Validate story node coherence
- [ ] Theme-specific validation rules
- [ ] Customizable scoring weights
- [ ] Batch validation for entire world libraries
