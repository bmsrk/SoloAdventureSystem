# Contributing to Solo Adventure System

Thank you for your interest in contributing! ??

## ?? Table of Contents
- [Code of Conduct](#code-of-conduct)
- [Getting Started](#getting-started)
- [Development Process](#development-process)
- [Coding Standards](#coding-standards)
- [Pull Request Process](#pull-request-process)
- [Reporting Bugs](#reporting-bugs)
- [Suggesting Features](#suggesting-features)

---

## ?? Code of Conduct

### Our Pledge
We are committed to providing a welcoming and inspiring community for all.

### Our Standards
- ? Be respectful and inclusive
- ? Accept constructive criticism gracefully  
- ? Focus on what's best for the community
- ? Show empathy towards others

- ? No harassment or discriminatory language
- ? No trolling or personal attacks
- ? No unwelcome sexual attention
- ? No publishing others' private information

---

## ?? Getting Started

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Git
- Code editor (Visual Studio, VS Code, Rider)
- Terminal with Unicode support

### Setup
```bash
# Clone the repository
git clone https://github.com/yourusername/SoloAdventureSystem.git
cd SoloAdventureSystem

# Restore dependencies
dotnet restore

# Build the solution
dotnet build

# Run tests
dotnet test
```

### First Steps
1. Read the [README.md](README.md)
2. Review the [Agent Instructions](docs/AGENT_INSTRUCTIONS.md)
3. Check the [Roadmap](docs/ROADMAP.md)
4. Look for issues labeled `good-first-issue`

---

## ?? Development Process

### Branching Strategy
- `main` - Stable, production-ready code
- `develop` - Integration branch for features
- `feature/*` - New features
- `bugfix/*` - Bug fixes
- `docs/*` - Documentation updates

### Workflow
```bash
# Create a feature branch
git checkout -b feature/your-feature-name

# Make changes and commit
git add .
git commit -m "feat: Add your feature"

# Push to GitHub
git push origin feature/your-feature-name

# Create Pull Request on GitHub
```

---

## ?? Coding Standards

### Follow Existing Patterns
- Review [AGENT_INSTRUCTIONS.md](docs/AGENT_INSTRUCTIONS.md)
- Match existing code style
- Use meaningful variable names
- Add XML documentation for public APIs

### Naming Conventions
```csharp
// Classes, Methods, Properties: PascalCase
public class WorldGenerator
public void GenerateWorld()
public string WorldName { get; set; }

// Private fields: _camelCase
private readonly ILogger _logger;
private string _currentState;

// Local variables, parameters: camelCase
var worldOptions = new WorldGenerationOptions();
public void Process(string inputValue)
```

### Code Quality
- ? Write self-documenting code
- ? Add comments for complex logic
- ? Keep methods under 50 lines
- ? Follow SOLID principles
- ? Use dependency injection

### Testing Requirements
- ? Write tests for new features
- ? Maintain > 80% code coverage
- ? All tests must pass before PR
- ? Follow Arrange-Act-Assert pattern

```csharp
[Fact]
public void MethodName_Scenario_ExpectedResult()
{
    // Arrange
    var input = CreateTestData();
    
    // Act
    var result = sut.Method(input);
    
    // Assert
    Assert.NotNull(result);
}
```

---

## ?? Pull Request Process

### Before Submitting
- [ ] Code builds successfully
- [ ] All tests pass
- [ ] New tests added (if applicable)
- [ ] Documentation updated
- [ ] Commit messages follow convention
- [ ] No merge conflicts

### Commit Message Convention
```
type(scope): Short description

Longer description if needed

Closes #123
```

**Types:**
- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation only
- `test`: Adding tests
- `refactor`: Code restructuring
- `chore`: Build/tooling changes
- `perf`: Performance improvement

**Examples:**
```
feat(combat): Add turn-based combat system
fix(worldgen): Correct NPC placement bug
docs(readme): Update quick start guide
test(engine): Add world loading tests
```

### PR Template
When creating a PR, include:
```markdown
## Description
Brief description of changes

## Type of Change
- [ ] Bug fix
- [ ] New feature
- [ ] Documentation
- [ ] Refactoring

## Testing
How was this tested?

## Checklist
- [ ] Tests pass
- [ ] Documentation updated
- [ ] Follows code standards
```

### Review Process
1. Maintainer reviews code
2. Automated tests run
3. Feedback provided if needed
4. Once approved, PR is merged
5. Branch is deleted

---

## ?? Reporting Bugs

### Before Reporting
1. Check if bug already reported
2. Try latest version
3. Gather reproduction steps

### Bug Report Template
```markdown
**Describe the bug**
Clear description of the issue

**To Reproduce**
Steps to reproduce:
1. Go to '...'
2. Click on '...'
3. See error

**Expected behavior**
What should happen

**Actual behavior**
What actually happens

**Screenshots**
If applicable

**Environment:**
- OS: [e.g., Windows 11]
- .NET Version: [e.g., 10.0]
- Project Version: [e.g., 1.0]

**Additional context**
Any other information
```

---

## ?? Suggesting Features

### Feature Request Template
```markdown
**Is your feature request related to a problem?**
Clear description of the problem

**Describe the solution you'd like**
What you want to happen

**Describe alternatives considered**
Other solutions you've thought about

**Additional context**
Mockups, examples, etc.
```

### Feature Priorities
Check the [Roadmap](docs/ROADMAP.md) to see:
- Planned features
- Current priorities
- Future possibilities

---

## ?? Areas to Contribute

### Code
- World generation improvements
- New AI providers
- UI enhancements
- Performance optimizations
- Bug fixes

### Content
- Example worlds
- Quest templates
- NPC personalities
- Lore and stories

### Documentation
- Tutorials and guides
- Code comments
- API documentation
- Translation (future)

### Testing
- Write unit tests
- Manual playtesting
- Performance testing
- Bug reproduction

---

## ?? Resources

### Documentation
- [Agent Instructions](docs/AGENT_INSTRUCTIONS.md) - Architecture and patterns
- [Roadmap](docs/ROADMAP.md) - Future plans
- [Game Design Doc](docs/GAME_DESIGN_DOCUMENT.md) - Design vision

### Communication
- GitHub Issues - Bug reports and features
- GitHub Discussions - Questions and ideas
- Pull Requests - Code contributions

---

## ?? Recognition

Contributors will be:
- Listed in CONTRIBUTORS.md
- Credited in release notes
- Mentioned in documentation

---

## ? Questions?

- Check the [FAQ](docs/FAQ.md) (if available)
- Open a GitHub Discussion
- Review existing issues

---

**Thank you for contributing!** ??

Your efforts help make Solo Adventure System better for everyone!
