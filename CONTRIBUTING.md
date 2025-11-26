# Contributing Guidelines and Project Distribution Standards

This file defines the team/project conventions for building, packaging and distributing the SoloAdventureSystem desktop UI.

## Purpose
These guidelines exist to:
- Ensure consistent builds and packaging for standalone distributions
- Define expected developer workflow for adding UI features
- Describe model handling policy for distributions
- Define CI/CD expectations and release process

## Project Layout and New UI Project
- A new MAUI Blazor Hybrid project MUST be added for the desktop UI. Project name suggestions:
  - `SoloAdventureSystem.Desktop` (preferred)
  - Or `SoloAdventureSystem.Aspire.Web.Web` if integrating under existing Aspire solution structure.
- The MAUI Blazor project MUST host existing Blazor components using `BlazorWebView`.
- Existing server-only Blazor Server pages may be reused with minimal changes; prefer components that do not require server-specific services.

## Distribution Requirements
- Deliverables: single-file, self-contained published artifacts per platform (Windows x64, Windows x86 optional, macOS x64/arm, Linux x64).
- Models MUST NOT be bundled into the installer/artifact. Models are downloaded on first run into a configurable cache directory (default: `%AppData%/SoloAdventureSystem/models` on Windows, `~/.local/share/SoloAdventureSystem/models` on Linux/macOS).
- Provide a clear first-run experience showing model download progress and an option to select CPU-only mode.
- Provide a configuration file `appsettings.json` and allow environment overrides.

## Build & Publish
- Use .NET 10 SDK for builds and `dotnet publish` for release artifacts.
- Example publish command (Windows x64 self-contained single-file):

```
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:PublishTrimmed=false -o ./publish
```

- For MAUI packaging and platform-native installers, use Visual Studio `Publish` workflows or the `dotnet publish` MAUI targets as needed.
- CI: GitHub Actions should build artifacts for each target and attach them to GitHub Releases. Provide workflow templates under `.github/workflows/publish.yml`.

## Model & Runtime Policy
- Default: Use GPU (CUDA) if available. Provide `UseGPU` toggle in `appsettings.json`.
- If GPU is not present, fall back to CPU with configurable `MaxInferenceThreads`.
- Implement model download with resume, integrity check (sha256 or similar), and retry logic.
- Cache eviction policy: LRU with configurable size limit. Provide a CLI or UI tool to clear cache.

## Packaging & Installer Policy
- For Windows: MSIX or ZIP attachments uploaded to GitHub Releases. For broader reach, upload the same zip to itch.io (free).
- For macOS: provide .pkg or zipped app bundle.
- For Linux: provide tar.gz or AppImage (preferred for desktop users).

## CI/CD & Releases
- CI must build on every tag and on `main`/`master` branch merges.
- Tagged release flow: tag ? CI builds platform artifacts ? create GitHub Release and attach artifacts ? optionally upload to itch.io.
- Provide a 'Check for updates' feature that queries GitHub Releases API. Updater should be opt-in and use HTTPS.

## Code Style & EditorConfig
- Project will include an `.editorconfig` file covering indentation (4 spaces), C# naming conventions, file encoding UTF-8, and trimming trailing whitespace.
- Follow existing solution formatting rules (C# 14.0, .NET 10).

## Testing
- Unit tests for services (WorldGenerationService, ModelDownloadService, WorldExporter) are required.
- Integration tests for first-run model download must run with network stubs in CI.

## Security & Privacy
- Do not upload or bundle private models or keys with releases.
- Use secure TLS when downloading models; validate certificates.
- Log only essential diagnostic information; avoid sensitive data in logs.

## Contribution Process
1. Fork the repository and create a feature branch named `feat/<short-descr>`.
2. Follow the `.editorconfig` and run `dotnet format` before committing.
3. Open pull request against `master`. Include a clear changelog for UI changes and packaging changes.
4. CI will run builds and unit tests. Fix any failures before merge.

## Rollback & Backup
- Always create a backup branch before destructive history operations: `git switch -c backup-before-<action>`.

## Contacts
- Maintainer: Project owner on GitHub (open an issue for questions)

---
This file is authoritative for project distribution and build behaviors. Update only via PR with CI passing.
