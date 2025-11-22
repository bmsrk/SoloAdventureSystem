# ? Solo Adventure System

<div align="center">

**A cyberpunk-themed AI-powered text adventure game with Terminal.GUI interface**

[![.NET](https://img.shields.io/badge/.NET-10.0-purple)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![Build](https://img.shields.io/badge/build-passing-brightgreen)](https://github.com)
[![Tests](https://img.shields.io/badge/tests-30%2F30-success)](https://github.com)

[Quick Start](#-quick-start) • [Features](#-features) • [Documentation](#-documentation) • [Roadmap](#-roadmap)

</div>

---

## ?? What is This?

Solo Adventure System is a **complete text adventure game platform** that combines:

- ?? **AI-Powered World Generation** - Create unique worlds using AI or deterministic algorithms
- ?? **Cyberpunk Terminal UI** - Retro-futuristic interface with Terminal.Gui
- ?? **Deterministic Worlds** - Same seed = same world, every time
- ?? **Infinite Possibilities** - Generate endless unique adventures
- ?? **Portable Worlds** - Share worlds as ZIP files

---

## ? Features

### World Generator
- ?? **5 AI Providers**: STUB (offline), GROQ (free!), OpenAI, GitHub Models, Azure
- ?? **Deterministic Generation**: Reproducible worlds from seeds
- ?? **Smart Caching**: Save AI API costs
- ?? **Configurable**: Zones, NPC density, themes, and more
- ?? **Export to ZIP**: Self-contained world packages
- ?? **Terminal.GUI Interface**: Beautiful cyberpunk UI

### Game Engine
- ??? **Room Navigation**: Explore interconnected locations
- ?? **NPC Interactions**: Talk to characters, learn about factions
- ?? **Character Stats**: HP, Level, XP, Attributes (STR, DEX, INT, etc.)
- ?? **Inventory System**: Collect and manage items (framework ready)
- ?? **Game Log**: Track your adventure
- ?? **Simple Commands**: `look`, `go north`, `talk`, `inventory`, `stats`

### Developer Experience
- ?? **30+ Tests**: Comprehensive test coverage
- ?? **Complete Documentation**: Architecture, guides, and examples
- ?? **Clean Architecture**: Modular, testable, maintainable
- ?? **Auto-Discovery**: Worlds automatically found and copied
- ?? **Well-Documented**: Code comments, XML docs, and guides

---

## ?? Quick Start

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Terminal with Unicode support
- (Optional) [GROQ API Key](https://groq.com/) for free AI generation

### 1. Clone the Repository
```sh
git clone https://github.com/yourusername/SoloAdventureSystem.git
cd SoloAdventureSystem
```

### 2. Generate Your First World
```sh
cd SoloAdventureSystem.AIWorldGenerator
dotnet run
```

**In the UI:**
1. Select **STUB** provider (for instant generation)
2. Name: `MyFirstWorld`
3. Seed: `12345`
4. Zones: `10`
5. Click **? GENERATE WORLD ?**

**Output:**
```
? GENERATED 10 ZONES
? SPAWNED 10 ENTITIES
? CREATED 1 FACTIONS
? FILE: content/worlds\WORLD_MyFirstWorld_12345.zip
```

### 3. Play the Game
```sh
cd ../SoloAdventureSystem.TerminalGUI.UI
dotnet run
```

**In the UI:**
1. Select your world from the list
2. Press Enter to start
3. Type `help` for commands

**Example Gameplay:**
```
> look
You are at: Central Hub
A bustling central location with holographic displays...
Exits: north, east, south, west

> go north
You move north to Data Vault.

> talk guard
You talk to Security Guard.
Security Guard: "Access restricted to authorized personnel only."
```

---

## ?? Documentation

### For Players
- **[Game Guide](./SoloAdventureSystem.TerminalGUI.UI/README.md)** - How to play
- **[Quick Start](./docs/MVP_COMPLETE.md#-quick-start-guide)** - Get started in 5 minutes

### For Developers
- **[?? Documentation Index](./docs/INDEX.md)** - Complete documentation map
- **[?? Agent Instructions](./docs/AGENT_INSTRUCTIONS.md)** - Architecture and patterns
- **[?? Roadmap](./docs/ROADMAP.md)** - Future features and priorities
- **[?? Worlds Setup](./docs/WORLDS_SETUP.md)** - Technical details

### Key Documents
| Document | Description |
|----------|-------------|
| [MVP Complete](./docs/MVP_COMPLETE.md) | What's working now |
| [Agent Instructions](./docs/AGENT_INSTRUCTIONS.md) | Developer guide |
| [Roadmap](./docs/ROADMAP.md) | Future plans |
| [Worlds Setup](./docs/WORLDS_SETUP.md) | Auto-copy system |

---

## ??? Project Structure

```
SoloAdventureSystem/
??? SoloAdventureSystem.Engine/              # Core engine
?   ??? Models/                              # World data models
?   ??? WorldLoader/                         # ZIP world loading
?
??? SoloAdventureSystem.AIWorldGenerator/    # World generator
?   ??? Adapters/                            # AI providers
?   ??? Generation/                          # World generation
?   ??? UI/                                  # Generator UI
?
??? SoloAdventureSystem.TerminalGUI.UI/      # Game player
?   ??? GameEngine/                          # Game logic
?   ??? Program.cs                           # Entry point
?
??? SoloAdventureSystem.Engine.Tests/        # Tests (30+)
?
??? content/worlds/                          # Shared worlds ?
?
??? docs/                                     # Documentation ??
    ??? INDEX.md                             # Documentation map
    ??? AGENT_INSTRUCTIONS.md                # Developer guide
    ??? ROADMAP.md                           # Future features
    ??? ...
```

---

## ?? Current Status: MVP 1.0 ?

### What Works
- ? AI World Generation (5 providers)
- ? Deterministic world creation
- ? Terminal.GUI game interface
- ? Room navigation
- ? NPC interactions
- ? Character stats
- ? Automatic world discovery
- ? 30 passing tests
- ? Complete documentation

### Known Limitations
- ? No combat system (Phase 2)
- ? No item pickup/usage (Phase 2)
- ? No quests (Phase 2)
- ? No save/load (Phase 3)

See [Roadmap](./docs/ROADMAP.md) for upcoming features!

---

## ?? Roadmap

### Phase 2: Enhanced Gameplay (v1.1)
- [ ] Combat system
- [ ] Inventory & items
- [ ] Quest system
- [ ] Save/load

### Phase 3: Polish (v1.2)
- [ ] Better UI
- [ ] Configuration
- [ ] More world themes

### Phase 4+: Advanced Features
- [ ] Character progression
- [ ] Advanced mechanics
- [ ] World editor
- [ ] Multiplayer (experimental)

**[View Full Roadmap ?](./docs/ROADMAP.md)**

---

## ?? Testing

```sh
# Run all tests
dotnet test

# Run specific tests
dotnet test --filter "FullyQualifiedName~WorldGeneratorTests"

# With coverage
dotnet test /p:CollectCoverage=true
```

**Test Results:** 30/30 passing ?

---

## ?? Building

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

## ?? Screenshots

### World Generator UI
```
??????????????????????????????????????????????????????????????????
?         ? ? ?  DAEMON NEURAL NET PROTOCOL  ? ? ?            ?
??????????????????????????????????????????????????????????????????
? ? WORLD NAME                                                   ?
?  Neon Nexus                                                    ?
? ? THEME                                                        ?
?  DYSTOPIAN MEGACITY                                            ?
? ? ZONES: 13                                                    ?
? ? NPC DENSITY: MODERATE                                        ?
??????????????????????????????????????????????????????????????????
```

### Game UI
```
?????????????????????????????????????????????????????????????????
?       ? SOLO ADVENTURE ? NEON_NEXUS                         ?
?????????????????????????????????????????????????????????????????
? >>> HP: 100/100 | LVL: 1 | XP: 0                             ?
? >>> LOCATION: Central Hub                                     ?
?????????????????????????????????????????????????????????????????
? ? LOCATION ?           ? ? EXITS ?                        ?
?                          ?  ? NORTH                           ?
? A bustling plaza...      ?  ? EAST                            ?
?                          ??????????????????????????????????????
?                          ? ? NPCS ?                         ?
?                          ?  • Street Vendor                   ?
?????????????????????????????????????????????????????????????????
? ? GAME LOG ?                                                ?
? > look                                                        ?
? You are at: Central Hub                                       ?
?????????????????????????????????????????????????????????????????
```

---

## ?? Contributing

Contributions are welcome! Please:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Read [Agent Instructions](./docs/AGENT_INSTRUCTIONS.md) for coding standards
4. Add tests for new features
5. Update documentation
6. Commit changes (`git commit -m 'feat: Add amazing feature'`)
7. Push to branch (`git push origin feature/amazing-feature`)
8. Open a Pull Request

---

## ?? License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

## ?? Acknowledgments

- **Terminal.Gui** - Excellent terminal UI framework
- **GROQ** - Free AI API access
- **YamlDotNet** - YAML serialization
- **xUnit** - Testing framework

---

## ?? Contact

- GitHub Issues: [Bug reports and features](https://github.com/yourusername/SoloAdventureSystem/issues)
- GitHub Discussions: [Questions and ideas](https://github.com/yourusername/SoloAdventureSystem/discussions)

---

## ? Star History

If you find this project useful, please give it a star! ?

---

<div align="center">

**Made with ? and .NET 10**

[Documentation](./docs/INDEX.md) • [Quick Start](#-quick-start) • [Roadmap](./docs/ROADMAP.md)

</div>
