# ?? Development Guide

**Audience**: Contributors, Developers  
**Level**: Intermediate to Advanced

---

## Welcome, Developer! ?????

Thank you for your interest in contributing to Solo Adventure System! This section contains everything you need to set up your development environment, understand our standards, and start contributing.

---

## ?? Documentation in This Section

### ?? [Contributing Guide](./CONTRIBUTING.md)
**Essential reading for all contributors**

**Topics covered**:
- Code of conduct
- How to contribute (issues, PRs, documentation)
- Contribution workflow
- Review process
- Recognition and credits

**Start here if**: You want to contribute to the project.

---

### ?? [Development Setup](./DEV_SETUP.md)
**Setting up your development environment**

**Topics covered**:
- Required tools and SDKs
- Cloning and building the project
- Running tests locally
- IDE configuration (VS Code, Visual Studio, Rider)
- Debugging setup

**Start here if**: You're ready to start coding.

---

### ?? [Coding Standards](./CODING_STANDARDS.md)
**Code style and conventions**

**Topics covered**:
- C# style guide
- Naming conventions
- File organization
- Documentation standards (XML comments)
- Best practices

**Start here if**: You're writing code for this project.

---

### ?? [Testing Guide](./TESTING.md)
**Writing and running tests**

**Topics covered**:
- Test structure and organization
- Unit testing best practices
- Integration testing approach
- Mocking and fixtures
- Running tests (local, CI/CD)
- Test coverage

**Start here if**: You're adding features or fixing bugs.

---

### ??? [Build & Deploy](./BUILD_DEPLOY.md)
**Building and publishing the application**

**Topics covered**:
- Build configurations
- Publishing for different platforms
- CI/CD pipeline
- Release process
- Versioning strategy

**Start here if**: You're preparing a release or deployment.

---

## ?? Quick Start for Contributors

### First-Time Setup (30 minutes)

```bash
# 1. Clone the repository
git clone https://github.com/yourusername/SoloAdventureSystem.git
cd SoloAdventureSystem

# 2. Install .NET 10 SDK
# Download from https://dotnet.microsoft.com/download

# 3. Restore packages
dotnet restore

# 4. Build the solution
dotnet build

# 5. Run tests
dotnet test

# 6. Run the application
cd SoloAdventureSystem.Terminal.UI
dotnet run
```

**? If all steps succeed, you're ready to contribute!**

See [Development Setup](./DEV_SETUP.md) for detailed instructions.

---

## ?? Contribution Workflow

```
1. Find/Create Issue ? 2. Fork ? 3. Branch ? 4. Code ? 5. Test ? 6. PR ? 7. Review ? 8. Merge
```

### Detailed Steps

1. **Pick an issue** or create one
   - Check [GitHub Issues](https://github.com/yourusername/SoloAdventureSystem/issues)
   - Comment to claim the issue

2. **Fork and clone**
   ```bash
   # Fork on GitHub, then:
   git clone https://github.com/YOUR_USERNAME/SoloAdventureSystem.git
   ```

3. **Create a branch**
   ```bash
   git checkout -b feature/your-feature-name
   # or
   git checkout -b fix/issue-123
   ```

4. **Make changes**
   - Follow [Coding Standards](./CODING_STANDARDS.md)
   - Write tests (see [Testing Guide](./TESTING.md))
   - Update documentation

5. **Test thoroughly**
   ```bash
   dotnet test
   dotnet build --configuration Release
   ```

6. **Commit and push**
   ```bash
   git add .
   git commit -m "feat: Add world theme customization"
   git push origin feature/your-feature-name
   ```

7. **Create Pull Request**
   - Describe changes clearly
   - Link related issues
   - Await review

8. **Address feedback**
   - Make requested changes
   - Push updates to same branch

9. **Merge!** ??
   - Maintainer merges when approved

---

## ?? Project Structure

```
SoloAdventureSystem/
??? SoloAdventureSystem.Engine/           # Core game engine
??? SoloAdventureSystem.AIWorldGenerator/ # AI integration
??? SoloAdventureSystem.Terminal.UI/      # Terminal interface
??? SoloAdventureSystem.Web.UI/           # Blazor web app
??? SoloAdventureSystem.CLI.Tests/        # CLI tests
??? SoloAdventureSystem.Engine.Tests/     # Engine tests
??? SoloAdventureSystem.ValidationTool/   # Testing utilities
??? docs/                                 # Documentation (wiki)
??? content/                              # Generated content
```

See [Project Structure](../architecture/PROJECT_STRUCTURE.md) for details.

---

## ??? Development Tools

### Required
- **.NET 10 SDK** - [Download](https://dotnet.microsoft.com/download)
- **Git** - Version control

### Recommended
- **Visual Studio 2022** or **VS Code** or **Rider**
- **Git Extensions** or **GitHub Desktop**
- **Windows Terminal** (Windows users)

### Optional
- **NVIDIA GPU with CUDA 12.x** - For testing GPU features
- **Docker** - For containerized testing

---

## ?? Testing Standards

All contributions must include tests:

- ? **Unit tests** for new features
- ? **Integration tests** for API changes
- ? **Update existing tests** if behavior changes
- ? **All tests pass** before PR

```bash
# Run all tests
dotnet test

# Run specific project tests
dotnet test SoloAdventureSystem.Engine.Tests

# Run with coverage (if configured)
dotnet test /p:CollectCoverage=true
```

See [Testing Guide](./TESTING.md) for detailed practices.

---

## ?? Documentation Standards

Code changes often require documentation updates:

| Change Type | Documentation Required |
|-------------|------------------------|
| New feature | User guide + API docs + changelog |
| Bug fix | Changelog + troubleshooting (if notable) |
| API change | API reference + migration guide |
| Configuration option | Configuration reference |
| Performance improvement | Benchmarks + performance guide |

Update relevant docs in `docs/` folder following the [Documentation Standards](../INDEX.md#-documentation-standards).

---

## ?? Code Quality

We maintain high code quality through:

- **Code reviews** - All PRs reviewed by maintainers
- **Automated tests** - CI/CD runs on every commit
- **Coding standards** - Consistent style across codebase
- **Static analysis** - Built-in .NET analyzers
- **Documentation** - XML comments on public APIs

---

## ?? Recognition

Contributors are recognized in:
- `CONTRIBUTORS.md` file
- Release notes
- GitHub contributors page
- Special thanks in major releases

---

## ?? Need Help?

- **Questions**: Open GitHub Discussion
- **Bug reports**: Open GitHub Issue
- **Feature ideas**: Open GitHub Issue with `enhancement` label
- **Code questions**: Ask in PR comments

---

## ?? Related Documentation

- [Architecture Overview](../architecture/ARCHITECTURE.md) - System design
- [API Reference](../technical/API_REFERENCE.md) - Public APIs
- [Troubleshooting](../troubleshooting/DEBUG_GUIDE.md) - Debug techniques

---

## ?? Areas Looking for Contributors

High-impact areas where contributions are especially welcome:

- ?? **AI prompts** - Improve generation quality
- ?? **UI/UX** - Enhance interfaces
- ?? **Documentation** - Expand guides
- ?? **Tests** - Increase coverage
- ?? **Localization** - Translate to other languages
- ? **Performance** - Optimize hot paths
- ?? **Bug fixes** - Check open issues

See [Roadmap](../project/ROADMAP.md) for planned features.

---

**Ready to contribute? Start with the [Contributing Guide](./CONTRIBUTING.md)! ??**

---

*Part of the [Solo Adventure System Documentation](../INDEX.md)*
