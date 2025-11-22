# ?? Solo Adventure System

[![.NET 10](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/download)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![Tests](https://img.shields.io/badge/Tests-56%20Passed-green.svg)]()

**A text-based adventure game engine with AI-powered world generation**

Solo Adventure System is a modern .NET framework for creating and playing procedurally generated text-based adventure games. Features embedded AI models for dynamic world creation, a clean terminal UI, and an extensible game engine.

---

## ? Features

### ?? Core Capabilities
- **AI-Powered World Generation** - Create unique game worlds using local LLM models (Phi-3, TinyLlama, Llama-3.2)
- **Terminal-Based UI** - Clean, intuitive interface using Terminal.Gui
- **Procedural Content** - Generate rooms, NPCs, factions, and storylines dynamically
- **Rule Engine** - Flexible game logic system for custom mechanics
- **World Persistence** - Save and load generated worlds as compressed archives
- **Extensible Architecture** - Clean separation between engine, generator, and UI

### ?? AI Features
- **Embedded Models** - Run LLMs locally with LLamaSharp
- **Model Auto-Download** - Automatic GGUF model downloading with progress tracking
- **Multiple Providers** - Support for Phi-3, TinyLlama, Llama-3.2, and more
- **Optimized Prompts** - Specially crafted prompts for world generation
- **CPU Backend** - No GPU required, runs on any modern PC

---

## ?? Quick Start

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) or later
- Windows, Linux, or macOS
- 8GB RAM minimum (16GB recommended for larger models)

### Installation

```bash
# Clone the repository
git clone https://github.com/bmsrk/SoloAdventureSystem.git
cd SoloAdventureSystem

# Build the solution
dotnet build

# Run the game
cd SoloAdventureSystem.Terminal.UI
dotnet run
```

### First-Time Setup

1. **Generate Your First World**
   - Select "Generate New World" from the main menu
   - Choose an AI provider (TinyLlama recommended for first-time users)
   - Enter a world name and seed
   - Wait for model download (one-time only) and generation

2. **Play the Game**
   - Select "Play Game" from the main menu
   - Choose a generated world
   - Start exploring!

---

## ?? Project Structure

```
SoloAdventureSystem/
??? SoloAdventureSystem.Engine/          # Core game engine
?   ??? Game/                            # Game state management
?   ??? WorldLoader/                     # World loading/saving
?   ??? Models/                          # Data models
?   ??? Rules/                           # Rule engine
??? SoloAdventureSystem.AIWorldGenerator/ # AI world generation
?   ??? Adapters/                        # LLM provider adapters
?   ??? EmbeddedModel/                   # Model management
?   ??? Generation/                      # Generation logic
?   ??? Models/                          # Generation models
??? SoloAdventureSystem.Terminal.UI/     # Terminal UI application
?   ??? Game/                            # Game UI components
?   ??? WorldGenerator/                  # Generator UI
?   ??? Themes/                          # UI themes
??? SoloAdventureSystem.Engine.Tests/    # Unit tests
??? content/                             # Shared content
?   ??? worlds/                          # Generated world files
??? docs/                                # Documentation
```

---

## ?? How to Play

### Main Menu Options

**1. Generate New World**
- Create a procedurally generated game world
- Choose from multiple AI models
- Customize world name and random seed
- Worlds are saved to `content/worlds/`

**2. Play Game**
- Browse available worlds
- Load and explore generated content
- (Game loop coming in v1.1)

**3. Exit**
- Close the application

### AI Model Selection

| Model | Size | Speed | Quality | Recommended For |
|-------|------|-------|---------|-----------------|
| **TinyLlama** | ~700MB | Fast | Good | Quick testing, low-end PCs |
| **Phi-3 Mini** | ~2.3GB | Medium | Excellent | Best balance |
| **Llama-3.2** | ~2.0GB | Medium | Excellent | Advanced users |

---

## ??? Architecture

### Technology Stack

- **Runtime**: .NET 10.0
- **Language**: C# 14.0
- **UI Framework**: Terminal.Gui 1.19.0
- **AI Engine**: LLamaSharp 0.15.0
- **DI Container**: Microsoft.Extensions.DependencyInjection 10.0.0
- **Configuration**: Microsoft.Extensions.Configuration 10.0.0
- **Serialization**: YamlDotNet 16.3.0

### Design Patterns

- **Dependency Injection** - Services configured via DI container
- **Adapter Pattern** - Multiple AI provider implementations
- **Factory Pattern** - World generator and adapter creation
- **Repository Pattern** - World loading and persistence
- **Strategy Pattern** - Rule engine for game mechanics

### Key Components

#### Game Engine (`SoloAdventureSystem.Engine`)
```csharp
// Core game state
public class GameState
{
    public WorldModel World { get; set; }
    public Location CurrentLocation { get; set; }
    public List<string> Inventory { get; set; }
    public Dictionary<string, bool> Flags { get; set; }
    public int TurnCount { get; set; }
}
```

#### World Generator (`SoloAdventureSystem.AIWorldGenerator`)
```csharp
// Generate worlds with AI
public interface IWorldGenerator
{
    Task<WorldGenerationResult> GenerateWorldAsync(
        WorldGenerationOptions options,
        IProgress<string>? progress = null,
        CancellationToken cancellationToken = default
    );
}
```

#### Terminal UI (`SoloAdventureSystem.Terminal.UI`)
- Main menu system with navigation
- World generator UI with progress tracking
- World selector with preview
- Minimal theme for clean aesthetics

---

## ?? Configuration

### `appsettings.json`

```json
{
  "AI": {
    "Provider": "LLamaSharp",
    "ModelPath": "./models",
    "DefaultModel": "phi-3-mini",
    "MaxTokens": 2048,
    "Temperature": 0.7,
    "CacheModels": true
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning"
    }
  }
}
```

### Environment Variables

```bash
# Override AI settings
AI__Provider=LLamaSharp
AI__DefaultModel=tinyllama

# Override logging
Logging__LogLevel__Default=Debug
```

---

## ?? Testing

### Run All Tests
```bash
dotnet test
```

### Test Coverage
- **56 tests** passing
- **1 test** skipped
- Coverage: Engine core, world loading, validation

### Test Projects
- `SoloAdventureSystem.Engine.Tests` - Unit tests for engine components

---

## ??? Roadmap

### Phase 1: MVP ? (Current)
- ? AI world generation
- ? Terminal UI
- ? Model management
- ? World persistence

### Phase 2: Enhanced Gameplay (v1.1)
- ? Turn-based combat system
- ? Inventory and items
- ? Quest system
- ? NPC interactions

### Phase 3: Persistence & Polish (v1.2)
- ?? Save/load game state
- ?? Enhanced UI
- ?? Configuration settings

### Phase 4: Advanced Features (v2.0)
- ?? Character progression
- ?? Advanced mechanics (stealth, persuasion)
- ?? Dynamic world events

See [docs/ROADMAP.md](docs/ROADMAP.md) for complete feature list.

---

## ?? Contributing

Contributions are welcome! Here's how to get started:

### Development Setup

```bash
# Clone and build
git clone https://github.com/bmsrk/SoloAdventureSystem.git
cd SoloAdventureSystem
dotnet restore
dotnet build

# Run tests
dotnet test

# Run the UI
cd SoloAdventureSystem.Terminal.UI
dotnet run
```

### Contribution Guidelines

1. **Fork** the repository
2. **Create** a feature branch (`git checkout -b feature/amazing-feature`)
3. **Make** your changes with clear commits
4. **Add** tests for new functionality
5. **Ensure** all tests pass (`dotnet test`)
6. **Submit** a pull request

### Code Style
- Follow C# coding conventions
- Use nullable reference types
- Add XML documentation for public APIs
- Keep methods focused and testable

---

## ?? Documentation

- **[Quick Start Guide](docs/QUICK_START.md)** - Get up and running
- **[Developer Guide](docs/DEVELOPER_GUIDE.md)** - Architecture and development
- **[AI Guide](docs/AI_GUIDE.md)** - Working with LLM models
- **[API Reference](docs/API_REFERENCE.md)** - Code documentation
- **[Roadmap](docs/ROADMAP.md)** - Future features and plans

---

## ?? Troubleshooting

### Common Issues

**Model Download Fails**
- Check internet connection
- Verify HuggingFace is accessible
- Try a smaller model (TinyLlama)

**Out of Memory**
- Close other applications
- Use a smaller model
- Increase system page file

**World Generation Hangs**
- Check CPU usage (should be 100% on one core)
- Be patient - first generation takes 2-5 minutes
- Try reducing world complexity

For more help, see [docs/TROUBLESHOOTING.md](docs/TROUBLESHOOTING.md)

---

## ?? License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

```
MIT License - Copyright (c) 2025 Solo Adventure System Contributors
```

---

## ?? Acknowledgments

- **[LLamaSharp](https://github.com/SciSharp/LLamaSharp)** - LLM inference engine
- **[Terminal.Gui](https://github.com/gui-cs/Terminal.Gui)** - Terminal UI framework
- **[YamlDotNet](https://github.com/aaubry/YamlDotNet)** - YAML serialization
- **HuggingFace** - Model hosting and distribution

---

## ?? Contact

- **GitHub**: [github.com/bmsrk/SoloAdventureSystem](https://github.com/bmsrk/SoloAdventureSystem)
- **Issues**: [github.com/bmsrk/SoloAdventureSystem/issues](https://github.com/bmsrk/SoloAdventureSystem/issues)

---

<div align="center">

**Built with ?? using .NET 10 and AI**

[? Star this repo](https://github.com/bmsrk/SoloAdventureSystem) if you find it useful!

</div>
