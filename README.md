# Solo Adventure System

This repository has been restructured: legacy UI projects (Web.UI, Web.Wasm, Terminal.UI) have been removed. See `CONTRIBUTING.md` for the new distribution and desktop UI plan.

This repository still contains core libraries and tools:
- `SoloAdventureSystem.Engine` - Core game engine
- `SoloAdventureSystem.AIWorldGenerator` - AI world generation logic
- `SoloAdventureSystem.ValidationTool` - Tools for validating generated worlds

Quick developer steps

```bash
# Clone and build the solution
git clone https://github.com/bmsrk/SoloAdventureSystem.git
cd SoloAdventureSystem
dotnet restore
dotnet build
```

If you are looking for how to run or build a UI distribution, read `CONTRIBUTING.md` — it documents the plan to create a standalone desktop MAUI Blazor Hybrid app and the packaging strategy for self-contained single-file releases.

For tool-oriented workflows (validation, model utilities), check the individual project READMEs under their folders.
