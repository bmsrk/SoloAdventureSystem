# Contributing to Solo Adventure System

Thank you for your interest in contributing! ??

This document provides guidelines and instructions for contributing to Solo Adventure System.

---

## Table of Contents
- [Code of Conduct](#code-of-conduct)
- [Getting Started](#getting-started)
- [Development Setup](#development-setup)
- [Making Changes](#making-changes)
- [Coding Standards](#coding-standards)
- [Testing](#testing)
- [Submitting Changes](#submitting-changes)
- [Areas for Contribution](#areas-for-contribution)

---

## Code of Conduct

### Our Pledge
We are committed to providing a welcoming and inspiring community for all. Please be respectful and constructive.

### Expected Behavior
- Use welcoming and inclusive language
- Be respectful of differing viewpoints
- Accept constructive criticism gracefully
- Focus on what's best for the community
- Show empathy towards others

### Unacceptable Behavior
- Harassment, discrimination, or trolling
- Publishing others' private information
- Disruptive or off-topic comments
- Other conduct that would be inappropriate in a professional setting

---

## Getting Started

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Git
- IDE: Visual Studio 2022, VS Code, or Rider
- 8GB+ RAM (for testing AI features)

### First Time Setup

1. **Fork the repository**
   ```bash
   # Click 'Fork' on GitHub, then clone your fork:
   git clone https://github.com/YOUR_USERNAME/SoloAdventureSystem.git
   cd SoloAdventureSystem
   ```

2. **Add upstream remote**
   ```bash
   git remote add upstream https://github.com/bmsrk/SoloAdventureSystem.git
   ```

3. **Build the solution**
   ```bash
   dotnet restore
   dotnet build
   ```

4. **Run tests**
   ```bash
   dotnet test
   ```

5. **Run the application**
   ```bash
   cd SoloAdventureSystem.Terminal.UI
   dotnet run
   ```

---

## Development Setup

### Project Structure
```
SoloAdventureSystem/
??? SoloAdventureSystem.Engine/          # Core game engine
??? SoloAdventureSystem.AIWorldGenerator/ # AI world generation
??? SoloAdventureSystem.Terminal.UI/     # Terminal UI
??? SoloAdventureSystem.Engine.Tests/    # Unit tests
??? content/worlds/                       # Generated worlds
```

### Branch Strategy

- `master` - Stable, production-ready code
- `develop` - Integration branch for features
- `feature/*` - New features (branch from `develop`)
- `bugfix/*` - Bug fixes (branch from `develop`)
- `hotfix/*` - Critical fixes (branch from `master`)

### Recommended Extensions (VS Code)

- C# Dev Kit
- .NET Extension Pack
- GitLens
- EditorConfig for VS Code

---

## Making Changes

### Creating a Feature Branch

```bash
# Update your fork
git checkout develop
git pull upstream develop

# Create feature branch
git checkout -b feature/your-feature-name
```

### Making Commits

Use clear, descriptive commit messages:

```bash
# Good commits:
git commit -m "Add cancellation token support to world generation"
git commit -m "Fix memory leak in LLamaSharpAdapter disposal"
git commit -m "Improve prompt optimizer truncation logic"

# Bad commits:
git commit -m "fixed stuff"
git commit -m "wip"
git commit -m "Update WorldGeneratorUI.cs"
```

### Commit Message Format

```
<type>: <subject>

<body>

<footer>
```

**Types**:
- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation changes
- `style`: Code style (formatting, no logic change)
- `refactor`: Code restructuring (no behavior change)
- `perf`: Performance improvement
- `test`: Adding or updating tests
- `chore`: Maintenance tasks

**Example**:
```
feat: Add model cache management UI

- Created ModelCacheInfo utility class
- Added cache summary and validation methods
- Integrated with WorldGeneratorUI settings

Closes #42
```

---

## Coding Standards

### C# Style Guide

Follow Microsoft's [C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions).

#### Key Points

1. **Naming**:
   ```csharp
   // PascalCase for classes, methods, properties
   public class WorldGenerator { }
   public void GenerateWorld() { }
   public string WorldName { get; set; }
   
   // camelCase for local variables, parameters
   var worldName = "MyWorld";
   public void Generate(int seed) { }
   
   // _camelCase for private fields
   private readonly ILogger _logger;
   ```

2. **Formatting**:
   ```csharp
   // Use 4 spaces for indentation (not tabs)
   // Opening braces on new line
   if (condition)
   {
       DoSomething();
   }
   
   // Single-line for simple cases
   if (simple) DoThis();
   ```

3. **Modern C# Features**:
   ```csharp
   // Use nullable reference types
   public string? NullableString { get; set; }
   
   // Use expression-bodied members
   public int Double(int x) => x * 2;
   
   // Use pattern matching
   if (obj is WorldModel world) { ... }
   
   // Use 'using' declarations
   using var stream = File.OpenRead(path);
   ```

4. **XML Documentation**:
   ```csharp
   /// <summary>
   /// Generates a world using the specified options.
   /// </summary>
   /// <param name="options">Generation parameters</param>
   /// <returns>Generated world result</returns>
   public WorldResult Generate(WorldOptions options)
   {
       // Implementation
   }
   ```

### Architecture Principles

1. **Dependency Injection**: Use constructor injection, avoid service locator
2. **SOLID Principles**: Follow Single Responsibility, Open/Closed, etc.
3. **Async/Await**: Use async methods for I/O operations
4. **Disposal**: Implement `IDisposable` for resources that need cleanup
5. **Null Safety**: Handle nulls explicitly, use nullable reference types

---

## Testing

### Writing Tests

```csharp
using Xunit;

public class WorldGeneratorTests
{
    [Fact]
    public void Generate_WithValidOptions_ReturnsWorld()
    {
        // Arrange
        var options = new WorldOptions { Name = "Test", Seed = 123 };
        var generator = new SeededWorldGenerator(...);
        
        // Act
        var result = generator.Generate(options);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test", result.World.Name);
    }
    
    [Theory]
    [InlineData(1, "Room 1")]
    [InlineData(2, "Room 2")]
    public void Generate_MultipleSeeds_ProducesDifferentRooms(int seed, string expected)
    {
        // ...
    }
}
```

### Test Coverage Goals

- **Core Engine**: 80%+ coverage
- **World Generator**: 70%+ coverage
- **UI**: Manual testing + critical path coverage

### Running Tests

```bash
# Run all tests
dotnet test

# Run specific test class
dotnet test --filter "FullyQualifiedName~WorldGeneratorTests"

# Run with coverage (requires coverlet)
dotnet test /p:CollectCoverage=true
```

---

## Submitting Changes

### Before Submitting

1. **Update from upstream**:
   ```bash
   git checkout develop
   git pull upstream develop
   git checkout feature/your-feature
   git rebase develop
   ```

2. **Run tests**:
   ```bash
   dotnet test
   ```

3. **Check build**:
   ```bash
   dotnet build --configuration Release
   ```

4. **Format code** (if using VS Code):
   ```bash
   dotnet format
   ```

### Creating Pull Request

1. **Push to your fork**:
   ```bash
   git push origin feature/your-feature
   ```

2. **Open PR on GitHub**:
   - Go to your fork on GitHub
   - Click "Pull Request"
   - Base: `bmsrk/SoloAdventureSystem:develop`
   - Compare: `your-fork:feature/your-feature`

3. **Fill PR template**:
   ```markdown
   ## Description
   Brief description of changes
   
   ## Type of Change
   - [ ] Bug fix
   - [ ] New feature
   - [ ] Breaking change
   - [ ] Documentation update
   
   ## Checklist
   - [ ] Tests added/updated
   - [ ] Documentation updated
   - [ ] Code follows style guidelines
   - [ ] No breaking changes (or documented)
   
   ## Related Issues
   Fixes #123
   ```

4. **Respond to review feedback**:
   - Make requested changes
   - Push updates to same branch
   - Respond to comments

---

## Areas for Contribution

### High Priority

- ? **Save/Load Game State** - Implement game state persistence
- ? **Combat System** - Add turn-based combat mechanics
- ? **Quest System** - Story-driven quests and objectives
- ? **Character Progression** - Leveling and skill systems

### Medium Priority

- ?? **Additional Themes** - Fantasy, Sci-Fi, Horror world themes
- ?? **World Editor** - In-game world editing tools
- ?? **Multiplayer** - Client/server architecture
- ?? **Advanced AI** - Better NPC dialogue, behavior trees

### Good First Issues

- ?? **Documentation** - Improve docs, add examples
- ?? **Bug Fixes** - Fix reported issues
- ?? **Tests** - Increase test coverage
- ?? **UI Improvements** - Better prompts, error messages

### Community Requested

Check [GitHub Issues](https://github.com/bmsrk/SoloAdventureSystem/issues) labeled:
- `good first issue` - Easy for newcomers
- `help wanted` - Community contributions welcome
- `enhancement` - Feature requests

---

## Questions?

- **GitHub Discussions**: https://github.com/bmsrk/SoloAdventureSystem/discussions
- **Issues**: https://github.com/bmsrk/SoloAdventureSystem/issues
- **Email**: Check GitHub profile

---

## License

By contributing, you agree that your contributions will be licensed under the MIT License.

---

**Thank you for contributing!** ??

Your time and effort make this project better for everyone.
