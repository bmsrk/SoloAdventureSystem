# Developer Guide - Solo Adventure System

## Project Overview

**Project Name**: Solo Adventure System  
**Type**: Terminal-based text adventure game with AI world generation  
**Framework**: .NET 10  
**UI**: Terminal.Gui  
**Status**: MVP 1.0 Complete

---

## Architecture

### Project Structure
```
SoloAdventureSystem/
??? SoloAdventureSystem.Engine/              # Core engine & models
?   ??? Models/                              # World data models
?   ??? WorldLoader/                         # ZIP world loading
?   ??? ...
?
??? SoloAdventureSystem.AIWorldGenerator/    # World generation tool
?   ??? Adapters/                            # AI provider adapters
?   ?   ??? StubSLMAdapter.cs               # Test/offline mode
?   ?   ??? GroqAdapter.cs                  # FREE AI (recommended)
?   ?   ??? OpenAIAdapter.cs                # OpenAI GPT
?   ?   ??? GitHubModelsAdapter.cs          # GitHub Models
?   ?   ??? AzureOpenAIAdapter.cs           # Azure OpenAI
?   ?   ??? CachedSLMAdapter.cs             # Caching wrapper
?   ??? Generation/                          # World generation logic
?   ??? UI/                                  # Terminal.GUI generator UI
?   ??? content/worlds/                      # Generated worlds (source)
?
??? SoloAdventureSystem.TerminalGUI.UI/      # Game player
?   ??? GameEngine/
?   ?   ??? GameState.cs                    # Game state management
?   ?   ??? GameUI.cs                       # Main game interface
?   ?   ??? WorldSelectorUI.cs              # World selection menu
?   ??? Program.cs                           # Entry point
?
??? SoloAdventureSystem.Engine.Tests/        # Test suite
?   ??? WorldGeneratorTests.cs              # 15 tests
?   ??? WorldLoaderServiceTests.cs          # 15 tests
?
??? content/worlds/                          # Shared worlds location
?
??? docs/                                     # Documentation
    ??? DEVELOPER_GUIDE.md (this file)
    ??? GAME_DESIGN_DOCUMENT.md
    ??? ROADMAP.md
    ??? EMBEDDED_AI_GUIDE.md
```

---

## Key Concepts

### World Generation
- **Deterministic**: Same seed = same world
- **AI Providers**: STUB (offline), GROQ (free), OpenAI, GitHub, Azure
- **Caching**: AI responses cached by (method + context + seed)
- **Output**: ZIP file with JSON/YAML content
- **Location**: Generated in `AIWorldGenerator/content/worlds/`
- **Auto-Copy**: Copied to `content/worlds/` on build

### World Format
```
WORLD_Name_Seed.zip
??? world.json          # Metadata
??? rooms/*.json        # Room definitions
??? npcs/*.json         # NPC data
??? factions/*.json     # Faction info
??? story/*.yaml        # Story nodes
??? system/             # Generation metadata
```

### World Discovery (Game)
Game searches in priority order:
1. `content/worlds/` (solution shared)
2. `AIWorldGenerator/content/worlds/` (generator source)
3. `./content/worlds/` (local)
4. `../AIWorldGenerator/content/worlds/` (parent)
5. `./worlds/` (bin output)

**Auto-Copy**: Worlds copied to `bin/Debug/net10.0/worlds/` on game build

---

## Common Tasks

### Adding a New Command
**File**: `SoloAdventureSystem.TerminalGUI.UI/GameEngine/GameUI.cs`

1. Add case to `ProcessCommand` switch:
```csharp
case "newcommand":
case "nc":
    DoNewCommand(args);
    break;
```

2. Implement method:
```csharp
private void DoNewCommand(string[] args)
{
    _gameState.AddLog("Executing new command...");
    // Your logic here
}
```

3. Add to help:
```csharp
private void ShowHelp()
{
    // Add line:
    _gameState.AddLog("? newcommand (nc) - Description      ?");
}
```

### Adding a New AI Provider
**File**: `SoloAdventureSystem.AIWorldGenerator/Adapters/`

1. Create new adapter:
```csharp
public class MyProviderAdapter : ILocalSLMAdapter
{
    public string GenerateRoomDescription(string context, int seed) { ... }
    public string GenerateNpcBio(string context, int seed) { ... }
    // Implement all interface methods
}
```

2. Add to `SLMAdapterFactory.cs`:
```csharp
case "MyProvider":
    baseAdapter = new MyProviderAdapter(settings, logger);
    break;
```

3. Update `appsettings.json` enum

### Adding World Generation Features

**File**: `SoloAdventureSystem.AIWorldGenerator/Generation/SeededWorldGenerator.cs`

Follow the deterministic pattern:
```csharp
var random = new Random(options.Seed + offset);
// Use 'random' for all generation
// Never use DateTime.Now or other non-deterministic sources!
```

### Adding Game State Properties

**File**: `SoloAdventureSystem.TerminalGUI.UI/GameEngine/GameState.cs`

```csharp
public class GameState
{
    // Add new property
    public Dictionary<string, bool> Achievements { get; set; } = new();
    
    // Add helper method if needed
    public void UnlockAchievement(string name)
    {
        if (!Achievements.ContainsKey(name))
        {
            Achievements[name] = true;
            AddLog($"[!] Achievement Unlocked: {name}");
        }
    }
}
```

---

## Code Standards

### Naming Conventions
- **Classes**: PascalCase (`GameState`, `WorldLoader`)
- **Methods**: PascalCase (`ProcessCommand`, `LoadWorld`)
- **Fields**: `_camelCase` with underscore (`_gameState`, `_logger`)
- **Properties**: PascalCase (`CurrentLocation`, `Player`)
- **Local vars**: camelCase (`worldPath`, `result`)

### File Organization
- One class per file
- File name matches class name
- Group related classes in folders
- Keep files under 500 lines

### Comments
- XML comments for public APIs
- Inline comments for complex logic
- TODO comments for future work
- HACK/FIXME for technical debt

```csharp
/// <summary>
/// Loads a world from a ZIP file.
/// </summary>
/// <param name="zipPath">Path to the world ZIP file</param>
/// <returns>Loaded world model or null if failed</returns>
public async Task<WorldModel?> LoadWorld(string zipPath)
{
    // Complex logic here
    var result = await LoadFromZipAsync(zipPath);
    
    // TODO: Add validation
    // HACK: Temporary workaround for issue #123
    
    return result;
}
```

### Error Handling
- Use try-catch for I/O operations
- Log errors with context
- Return null or throw on failure
- User-friendly error messages

```csharp
try
{
    using var fs = new FileStream(path, FileMode.Open);
    return await loader.LoadFromZipAsync(fs);
}
catch (FileNotFoundException ex)
{
    _logger.LogError(ex, "World file not found: {Path}", path);
    Console.WriteLine($"[X] Error: World file not found");
    return null;
}
```

---

## Testing Guidelines

### Test Structure
```csharp
[Fact]
public void MethodName_Scenario_ExpectedResult()
{
    // Arrange
    var input = CreateTestData();
    var sut = new SystemUnderTest();
    
    // Act
    var result = sut.Method(input);
    
    // Assert
    Assert.NotNull(result);
    Assert.Equal(expected, result);
}
```

### Test Categories
1. **Unit Tests**: Single class/method
2. **Integration Tests**: Multiple components
3. **E2E Tests**: Full workflow

### Running Tests
```sh
# All tests
dotnet test

# Specific test
dotnet test --filter "FullyQualifiedName~WorldGeneratorTests"

# With coverage
dotnet test /p:CollectCoverage=true
```

---

## UI/UX Guidelines

### Terminal.GUI Patterns
```csharp
// Color scheme
var cyberCyan = new ColorScheme
{
    Normal = new Terminal.Gui.Attribute(Color.Cyan, Color.Black),
    Focus = new Terminal.Gui.Attribute(Color.Black, Color.Cyan),
};

// Window creation
var win = new Window("Title")
{
    X = 0, Y = 0,
    Width = Dim.Fill(),
    Height = Dim.Fill(),
    ColorScheme = cyberCyan
};

// Layout
var frame = new FrameView("Frame")
{
    X = 1, Y = 1,
    Width = Dim.Percent(50),
    Height = Dim.Fill(1)
};
```

### Cyberpunk Aesthetic
- Use ASCII symbols: >, >>, >>>, *, •, -, |
- Cyan/Magenta/Green color scheme
- ASCII borders: ????? ? ? ????? 
- ALL CAPS for emphasis
- >>> prefix for system messages

---

## Debugging Tips

### Common Issues

**"Could not find worlds directory"**
- Check `content/worlds/` exists
- Run `dotnet build` to trigger auto-copy
- Check console output for search paths

**Worlds not loading**
- Validate ZIP structure with tests
- Check file permissions
- Look for JSON parsing errors in logs

**UI not rendering correctly**
- Terminal must support Unicode
- Try resizing terminal
- Check Terminal.Gui version compatibility

### Debug Commands
```sh
# Verbose logging
dotnet run --verbosity detailed

# Check build output
dotnet build /v:detailed

# Clean build
dotnet clean && dotnet build

# Run specific project
dotnet run --project SoloAdventureSystem.TerminalGUI.UI
```

---

## Building & Deployment

### Development Build
```sh
dotnet build
```

### Release Build
```sh
dotnet build -c Release
```

### Publish Standalone
```sh
# Windows
dotnet publish -c Release -r win-x64 --self-contained

# Linux
dotnet publish -c Release -r linux-x64 --self-contained

# MacOS
dotnet publish -c Release -r osx-x64 --self-contained
```

---

## Git Workflow

### Branch Strategy
- `main` - Stable releases
- `develop` - Development branch
- `feature/*` - New features
- `bugfix/*` - Bug fixes
- `release/*` - Release preparation

### Commit Messages
```
type(scope): Short description

Longer description if needed

Closes #123
```

Types: `feat`, `fix`, `docs`, `test`, `refactor`, `chore`

### Example
```
feat(combat): Add turn-based combat system

- Implemented dice rolling
- Added damage calculation
- Created combat UI

Closes #42
```

---

## Documentation

### Update When:
- Adding new features ? Update ROADMAP.md
- Completing milestones ? Update README.md
- Changing architecture ? Update this file
- Adding new files ? Update README.md

### Documentation Files
| File | Purpose | Update When |
|------|---------|-------------|
| `README.md` | User guide | User-facing changes |
| `ROADMAP.md` | Future plans | Planning |
| `AGENT_INSTRUCTIONS.md` | This file | Architecture changes |
| `GAME_DESIGN_DOCUMENT.md` | Design vision | Design changes |

---

## Quick Reference

### File Locations
| What | Where |
|------|-------|
| Game commands | `GameUI.cs` ? `ProcessCommand()` |
| Game state | `GameState.cs` |
| World loading | `Engine/WorldLoader/WorldLoaderService.cs` |
| World generation | `AIWorldGenerator/Generation/SeededWorldGenerator.cs` |
| AI providers | `AIWorldGenerator/Adapters/*.cs` |
| Configuration | `appsettings.json` |
| Tests | `Engine.Tests/*Tests.cs` |

### Important Constants
```csharp
// World Discovery Timeout
const int MAX_SEARCH_LEVELS = 5;

// Default Player Stats
const int DEFAULT_HP = 100;
const int DEFAULT_LEVEL = 1;

// Game Log Size
const int MAX_LOG_ENTRIES = 100;
```

---

## Important Notes

### DO:
- [x] Use deterministic generation (seeded Random)
- [x] Add tests for new features
- [x] Update documentation
- [x] Follow naming conventions
- [x] Handle errors gracefully
- [x] Log important actions
- [x] Use dependency injection

### DON'T:
- [ ] Use DateTime.Now for generation
- [ ] Hard-code file paths
- [ ] Ignore null references
- [ ] Skip tests
- [ ] Leave TODO comments permanently
- [ ] Break determinism
- [ ] Commit secrets/API keys

---

## World Discovery System

### Directory Structure
```
SoloAdventureSystem/
??? content/worlds/                          # Shared worlds location
??? AIWorldGenerator/content/worlds/         # Generated here first
??? TerminalGUI.UI/bin/Debug/net10.0/worlds/ # Copied for exe
```

### Search Priority
The game searches in this order:
1. `content/worlds/` (solution shared)
2. `AIWorldGenerator/content/worlds/` (generator)
3. `./content/worlds/` (local)
4. `../AIWorldGenerator/content/worlds/` (parent)
5. `./worlds/` (bin output)

### Auto-Copy System
- **Generator build**: Copies worlds to `content/worlds/`
- **Game build**: Copies worlds to `bin/Debug/net10.0/worlds/`
- **Result**: Worlds automatically discovered from any run location

---

## Getting Help

### Resources
- Project README files
- Inline code documentation
- Test examples
- Terminal.Gui docs: https://gui-cs.github.io/Terminal.Gui/
- .NET 10 docs: https://learn.microsoft.com/dotnet/

### Contact
- GitHub Issues for bugs
- GitHub Discussions for questions
- Pull Requests for contributions

---

**Keep this file updated as the project evolves!**

Last Updated: November 22, 2025  
Version: 1.0  
Status: MVP Complete
