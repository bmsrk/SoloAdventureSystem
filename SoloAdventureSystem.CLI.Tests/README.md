# SoloAdventureSystem CLI Tests

This test project contains integration tests for the CLI-based world generation functionality.

## Test Suite

### WorldGenerationIntegrationTests

Integration tests that verify the complete world generation workflow using LLamaSharp AI.

#### Thread Safety & Mutex Protection

The tests include **mutex-based locking** to prevent concurrent access to model files:

- **Download Protection**: Only one process can download a model at a time
- **File Access Protection**: Prevents file access conflicts during model loading
- **Cross-Process Safety**: Works across multiple test runners and processes
- **Timeout Handling**: Automatically releases locks after timeout (5 minutes for download, 2 minutes for loading)

This prevents the following errors:
- ? `IOException: The process cannot access the file because it is being used by another process`
- ? File corruption during concurrent downloads
- ? Model loading conflicts when multiple tests run in parallel

#### Tests

1. **GenerateWorld_WithLLamaSharp_CreatesValidWorld** ?? *Long-running (2-5 minutes)*
   - **Purpose**: Full end-to-end test of world generation with LLamaSharp
   - **What it tests**:
     - Model download and initialization
     - World generation with AI
     - Structural validation
     - Quality validation (LLM-based)
     - Export to directory structure
     - ZIP archive creation
     - File integrity verification
   - **Note**: Skipped by default due to long runtime
   - **Run explicitly with**: `dotnet test --filter "FullyQualifiedName~GenerateWorld_WithLLamaSharp_CreatesValidWorld"`

2. **GenerateWorld_WithLLamaSharp_CreatesReproducibleWorld** ? *Enabled*
   - **Purpose**: Verifies deterministic world generation
   - **What it tests**:
     - Same seed produces identical world structure
     - Room names and IDs are consistent
     - NPC names and IDs are consistent
     - Generation is reproducible across runs
   - **Runtime**: ~1-3 minutes (includes model initialization)

## Running Tests

### Run All Tests (Excluding Skipped)
```bash
dotnet test
```

### Run Only CLI Tests
```bash
dotnet test SoloAdventureSystem.CLI.Tests
```

### Run Long Tests (Including Skipped)
```bash
dotnet test --filter "FullyQualifiedName~WorldGenerationIntegrationTests"
```

### Run With Verbose Output
```bash
dotnet test --logger "console;verbosity=detailed"
```

## Test Configuration

Tests use the following AI configuration:
- **Provider**: LLamaSharp
- **Model**: phi-3-mini-q4 (2GB)
- **Context Size**: 2048 tokens
- **GPU**: Disabled (CPU-only)
- **Threads**: 4

## Test Output

Tests create temporary directories for:
- Generated world files (JSON, YAML)
- ZIP archives
- Intermediate files

All temporary files are cleaned up automatically after each test.

## What Gets Validated

### Structural Validation
- ? World has minimum required rooms
- ? All rooms have valid IDs and titles
- ? Room connections are valid
- ? NPCs are properly assigned to rooms
- ? Factions are created and referenced
- ? Lore entries are generated

### Quality Validation (LLM-based)
- ? Room descriptions are thematic and descriptive
- ? NPC bios are coherent and relevant
- ? Faction descriptions fit the theme
- ? Overall quality score meets minimum threshold
- ? Content consistency across the world

### Export Validation
- ? All expected files are created
- ? JSON files are valid
- ? ZIP archive is well-formed
- ? File count matches entity count
- ? Directory structure is correct

## Expected Output

When tests run successfully, you'll see:
```
? Model initialized successfully
? World generated in X.Xs
   Rooms:    5
   NPCs:     5
   Factions: 1
   Lore:     3 entries
? Structural validation passed
Quality Metrics:
   Room Quality:    XX/100
   NPC Quality:     XX/100
   Faction Quality: XX/100
   Consistency:     XX/100
   Overall Score:   XX/100
? Exported X room files
? Created ZIP file: X.XX MB
? ZIP contains XX files
? All tests passed!
```

## Troubleshooting

### Model Download Issues
If tests fail during model initialization:
1. Check internet connection
2. Verify sufficient disk space (~2GB for phi-3-mini-q4)
3. Check model cache directory: `~/.gguf_models/`
4. Delete corrupted models and retry

### Memory Issues
If tests fail with out-of-memory errors:
1. Close other applications
2. Use a smaller model (tinyllama-q4)
3. Reduce context size in test configuration
4. Run tests individually instead of all at once

### Test Timeout
If tests timeout:
1. Increase test timeout in your test runner
2. Check CPU performance (model runs on CPU by default)
3. Verify model is already cached (first run takes longer)

## CI/CD Integration

For continuous integration:
- Skip long-running tests by default
- Run full validation tests nightly
- Cache downloaded models between runs
- Set appropriate timeouts (5-10 minutes)

Example GitHub Actions configuration:
```yaml
- name: Run Tests
  run: dotnet test --filter "FullyQualifiedName!~GenerateWorld_WithLLamaSharp_CreatesValidWorld"
  
- name: Run Full Integration Tests (Nightly)
  if: github.event_name == 'schedule'
  run: dotnet test --filter "FullyQualifiedName~WorldGenerationIntegrationTests"
  timeout-minutes: 10
```

## Contributing

When adding new tests:
1. Use descriptive test names following the pattern: `MethodName_Scenario_ExpectedResult`
2. Add XML documentation explaining what the test validates
3. Use `[Fact(Skip = "reason")]` for long-running tests
4. Output progress to `ITestOutputHelper` for debugging
5. Clean up resources in `Dispose()`
6. Keep test data small (3-5 regions max for speed)

## Related Documentation

- [CLI Implementation](../docs/CLI_IMPLEMENTATION.md)
- [CLI Quick Start](../SoloAdventureSystem.CLI/QUICKSTART.md)
- [LLamaSharp Fixes](../docs/LLAMASHARP_FIXES.md)
