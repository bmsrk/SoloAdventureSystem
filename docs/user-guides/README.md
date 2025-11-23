# ?? User Guides

**Audience**: End Users  
**Level**: Beginner to Intermediate

---

## Overview

This section contains guides for using Solo Adventure System's various interfaces and features. Whether you prefer the terminal, web browser, or command-line, we've got you covered.

---

## ??? Interface Guides

### [Terminal UI Guide](./TERMINAL_UI.md)
Learn to use the text-based terminal interface.

**Topics covered**:
- Navigation and controls
- World generation workflow
- Theme customization
- Keyboard shortcuts
- Tips and tricks

**Best for**: Users who prefer traditional terminal UIs, low-resource systems, or SSH access.

---

### [Web UI Guide](./WEB_UI.md)
Use the modern Blazor web interface.

**Topics covered**:
- Accessing the web app
- Real-time progress tracking
- Model initialization
- World preview and export
- Configuration options

**Best for**: Users who want a modern, visual interface with real-time feedback.

---

### [CLI Reference](./CLI_REFERENCE.md)
Command-line tools and scripts.

**Topics covered**:
- Command syntax and options
- Batch world generation
- Scripting and automation
- Configuration via command line
- CI/CD integration

**Best for**: Power users, automation, scripting, and batch operations.

---

## ?? World Management

### [World Management](./WORLD_MANAGEMENT.md)
Organize, import, export, and manage your generated worlds.

**Topics covered**:
- World file format (ZIP structure)
- Import/export workflows
- Backup and restore
- Sharing worlds
- Cloud storage integration

---

### [World Customization](./WORLD_CUSTOMIZATION.md)
Advanced world generation techniques.

**Topics covered**:
- Customization parameters explained
- Theme selection and creation
- Seeded generation for reproducibility
- Advanced prompting techniques
- Quality tuning
- Multi-world campaigns

---

## ?? Quick Navigation

### I Want To...

| Task | Guide | Page |
|------|-------|------|
| Generate my first world | [Terminal UI](./TERMINAL_UI.md) or [Web UI](./WEB_UI.md) | Any |
| Batch generate 10 worlds | [CLI Reference](./CLI_REFERENCE.md) | Batch Commands |
| Customize world themes | [World Customization](./WORLD_CUSTOMIZATION.md) | Themes |
| Export/share a world | [World Management](./WORLD_MANAGEMENT.md) | Export |
| Use same seed to reproduce | [World Customization](./WORLD_CUSTOMIZATION.md) | Seeded Generation |
| Change AI model | [Web UI](./WEB_UI.md) or [AI Models](../ai/AI_MODELS.md) | Configuration |

---

## ?? Common Questions

### Which interface should I use?

| Interface | Best For | Pros | Cons |
|-----------|----------|------|------|
| **Terminal UI** | Linux servers, SSH, minimal resources | Fast, lightweight, keyboard-driven | Text-only, no visuals |
| **Web UI** | Local use, visual feedback | Modern, real-time progress, easy to use | Requires browser |
| **CLI** | Automation, scripting, batch jobs | Scriptable, fast, non-interactive | No UI, less feedback |

**Recommendation**: 
- **New users**: Start with **Web UI**
- **Terminal fans**: Use **Terminal UI**
- **Automation**: Use **CLI**

---

### How do I choose an AI model?

See [AI Models Overview](../ai/AI_MODELS.md) for detailed comparison.

**Quick guide**:
- **TinyLlama**: Fast, low resources, good quality
- **Phi-3 Mini**: Best quality, moderate speed
- **Llama-3.2**: Balanced option

---

### Can I use multiple interfaces?

Yes! All interfaces work with the same:
- Configuration files
- Model cache
- Generated worlds
- Settings

Switch between them anytime without reconfiguration.

---

## ?? Related Documentation

- [AI Models Overview](../ai/AI_MODELS.md) - Choose the right model
- [GPU Acceleration](../ai/GPU_ACCELERATION.md) - Speed up generation
- [Configuration Reference](../technical/CONFIGURATION.md) - All settings
- [Troubleshooting](../troubleshooting/COMMON_ISSUES.md) - Fix problems

---

## ?? Tutorial Path

1. **First Time**: [Quick Start](../getting-started/QUICK_START.md) ? Choose interface guide
2. **Basic Usage**: Interface guide ? Generate worlds
3. **Customize**: [World Customization](./WORLD_CUSTOMIZATION.md)
4. **Organize**: [World Management](./WORLD_MANAGEMENT.md)
5. **Advanced**: [CLI Reference](./CLI_REFERENCE.md) for automation

---

## ?? Tips for Success

- **Start simple**: Use default settings first
- **Experiment**: Try different themes and models
- **Save seeds**: Reproduce favorite worlds
- **Use GPU**: Enable CUDA for 5-10x speedup
- **Batch generate**: Create variety, pick the best

---

**Choose your interface and start creating! ??**

---

*Part of the [Solo Adventure System Documentation](../INDEX.md)*
