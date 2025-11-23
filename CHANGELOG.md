# Changelog

All notable changes to Solo Adventure System will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- Cancellation token support for world generation - users can now cancel long-running operations
- Enhanced `DownloadProgress` with convenience properties (`DownloadedMB`, `SpeedMBPerSecond`, etc.)
- `ModelCacheInfo` utility class for managing and inspecting cached AI models
- Path sanitization in `PathHelper` to prevent path traversal attacks and handle invalid characters
- Null safety checks throughout `GameUI` for more robust error handling
- Thread-safe UI updates with proper exception handling
- `appsettings.Development.json` for development-specific configuration
- Comprehensive inline documentation and XML comments

### Changed
- **CRITICAL FIX**: Fixed model selection bug where Llama-3.2 model used display text instead of model key
- **CRITICAL FIX**: Implemented `IDisposable` pattern in `WorldGeneratorUI` to prevent 2GB+ memory leaks
- Improved `PromptOptimizer` to preserve quality - increased limit from 500 to 1500 chars with smart truncation
- Enhanced exception handling in `SeededWorldGenerator` - better logging while preserving stack traces
- Updated `Program.cs` to properly dispose `WorldGeneratorUI` using `using` statement
- `GameUI` now handles null values gracefully with sensible defaults

### Fixed
- Memory leak from cached `LLamaSharpAdapter` not being disposed
- Thread safety issues in UI update methods (`Log` and `UpdateStatus`)
- Model selection dropdown using wrong key for Llama-3.2 model
- Over-aggressive prompt truncation destroying instruction quality
- Missing null checks in NPC interactions and location descriptions
- Path validation not checking for invalid characters or path traversal

### Security
- Added path sanitization to prevent directory traversal attacks
- Added validation for world names to prevent malicious file paths
- Improved error handling to prevent information leakage in exceptions

## [1.0.0] - 2025-01-XX (Planned)

### Added
- AI-powered world generation using local LLM models (Phi-3, TinyLlama, Llama-3.2)
- Terminal-based UI using Terminal.Gui framework
- Procedural content generation for rooms, NPCs, factions, and storylines
- World persistence as compressed ZIP archives
- Model auto-download with progress tracking
- Seeded generation for reproducible worlds
- Stub provider for fast testing without AI
- Comprehensive test suite with 56+ passing tests
- Clean architecture with dependency injection
- Cross-platform support (Windows, Linux, macOS)

### Documentation
- Comprehensive README with quick start guide
- In-depth architecture documentation
- AI integration guide
- Developer setup instructions
- Troubleshooting guide

---

## Version History Legend

- ?? **CRITICAL** - Security or data loss issue requiring immediate fix
- ?? **HIGH** - Important bug or performance issue
- ?? **MEDIUM** - Moderate issue or improvement
- ?? **LOW** - Minor enhancement or cosmetic fix

---

## Upgrade Guide

### From Pre-Release to 1.0.0

1. **Memory Management**: The `WorldGeneratorUI` now implements `IDisposable`. If you're using it programmatically, wrap it in a `using` statement:
   ```csharp
   using var worldGeneratorUI = serviceProvider.GetRequiredService<WorldGeneratorUI>();
   ```

2. **Model Selection**: The Llama-3.2 model key has been corrected. If you were using it, no action needed - it will now work correctly.

3. **Path Handling**: World names are now automatically sanitized. Previously invalid characters are replaced with underscores.

4. **Cancellation Support**: You can now pass a `CancellationToken` to `GenerateWorldAsync()` for better control over long-running operations.

---

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md) for details on our code of conduct and the process for submitting pull requests.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
