# SoloAdventureSystem.ContentGenerator - AI World Generator

AI-powered world generation tool for SoloAdventureSystem using GitHub Models (or Azure OpenAI).

## Recent Updates (v1.2 - 2025)

### ?? **NEW: FREE AI Provider - Groq!**
- **100% FREE tier** with generous limits (no credit card required!)
- **Lightning fast** inference (faster than OpenAI)
- **Easy signup** at https://console.groq.com
- **Multiple models**: Llama 3.3 70B, Mixtral 8x7B, Gemma 2 9B
- Perfect solution if your OpenAI quota is exhausted!

### ?? Improved Generation Process
- **Dynamic adapter recreation**: Settings changes in UI now take effect immediately
- **Better error handling**: Clear error messages that stop generation on failure
- **Automatic retry logic**: Handles rate limits and transient failures automatically
- **Progress tracking**: Detailed logging shows exactly what's being generated
- **Fixed OpenAI adapter**: Now uses correct `OpenAIClient` instead of `AzureOpenAIClient` (fixes 404 errors)

### ??? Error Handling
- **Authentication errors (401)**: Fail fast with clear "Check your API key" message
- **Rate limits (429)**: Automatic retry with exponential backoff (2s, 4s, 8s...)
- **Server errors (500+)**: Automatic retry with linear backoff (1s, 2s, 3s...)
- **Network errors**: Configurable retries (default: 3 attempts)

---

## Features

- ??? **Beautiful Terminal UI** - Interactive wizard-style interface with API key input
- ?? **Deterministic generation** from seeds
- ?? **FREE AI Provider** - **Groq** with 100% free tier and fast inference!
- ?? **Multiple AI providers**: **Groq (FREE!)**, OpenAI, GitHub Models, Azure OpenAI, or Stub (for testing)
- ?? **Smart caching** for reproducible builds and cost savings
- ?? **Automatic retry logic** - Handles rate limits and failures gracefully
- ? **Easy model switching** via configuration
- ?? **Enter API key in UI** - No configuration files needed!
- ?? **Generates complete world packages** as ZIP files
- ?? **CLI mode** for automation and scripting

## Quick Start

### Interactive Mode (Default - Recommended!)

Just run without arguments to launch the beautiful Terminal.GUI interface:

```bash
dotnet run
```

**?? First time? Use FREE Groq (Recommended!)**
1. Launch UI: `dotnet run`
2. Select **"GROQ (FREE!)"** as provider
3. Get FREE API key: https://console.groq.com (no credit card!)
4. Paste key in the **"API Key"** field (yellow)
5. Choose **"LLAMA-3.3-70B"** model (fast & powerful)
6. Click **"Generate World"**
7. Watch your AI-generated world! ??

**Or use OpenAI (if you have credits):**
1. Launch UI: `dotnet run`
2. Select **"OpenAI"** as provider
3. Get free API key: https://platform.openai.com/api-keys (gives you $5 free credit!)
4. Paste key in the **"API Key"** field (yellow)
5. Choose **"gpt-4o-mini"** model
6. Click **"Generate World"**

**or test instantly without API key:**
1. Select **"Stub (Testing)"**
2. Click **"Generate World"**
3. Instant world with placeholder text!

### CLI Mode (For Automation)

```bash
# Using stub (no AI, for testing)
dotnet run --name="TestWorld" --seed=12345

# Using GitHub Models
dotnet run --name="Cidade Cinza" --seed=12345 --theme="Urban Horror" --regions=8 --npc-density=medium
```

## Configuration

### appsettings.json

| Setting | Description | Default |
|---------|-------------|---------|
| `Provider` | AI provider: `Stub`, `GitHubModels`, `AzureOpenAI`, `OpenAI` | `Stub` |
| `Endpoint` | API endpoint | `https://models.inference.ai.azure.com` |
| `Model` | Model to use | `gpt-4o-mini` |
| `Token` | GitHub/Azure/OpenAI token | _(required for AI)_ |
| `EnableCaching` | Cache AI responses | `true` |
| `CacheDirectory` | Where to store cache | `.aicache` |
| `Temperature` | Randomness (0=deterministic) | `0.0` |
| `MaxRetries` | Retry failed requests (1-10) | `3` |
| `TimeoutSeconds` | Request timeout (1-300) | `30` |

### Retry Behavior
- **Rate Limit Errors (429)**: Retries with exponential backoff (2s, 4s, 8s...)
- **Server Errors (500+)**: Retries with linear backoff (1s, 2s, 3s...)
- **Authentication Errors (401)**: No retry - fails immediately
- **Network Errors**: Retries based on `MaxRetries` setting

### Command-line Arguments (CLI Mode)

```bash
--name=WorldName          # World name (default: TestWorld)
--seed=12345              # Random seed for generation
--theme="Urban Horror"    # Theme/setting
--regions=8               # Number of regions/rooms
--npc-density=medium      # NPC density: low, medium, high
--render-images=false     # Generate images (not yet implemented)
--ui                      # Force interactive UI mode
```

## Switching AI Providers

### In Interactive UI
Use the radio buttons in the "AI Provider" section - changes take effect immediately!

### In Configuration File

```json
// Stub (No AI - instant generation)
{ "AI": { "Provider": "Stub" } }

// Groq (100% FREE! - Recommended if OpenAI is out of quota)
{
  "AI": {
    "Provider": "Groq",
    "Model": "llama-3.3-70b-versatile",
    "Token": "gsk_YOUR_FREE_KEY"
  }
}

// OpenAI (Free $5 credit)
{
  "AI": {
    "Provider": "OpenAI",
    "Token": "sk-proj-YOUR_KEY"
  }
}

// GitHub Models (Free tier available)
{
  "AI": {
    "Provider": "GitHubModels",
    "Token": "ghp_YOUR_TOKEN"
  }
}

// Azure OpenAI
{
  "AI": {
    "Provider": "AzureOpenAI",
    "Endpoint": "https://YOUR_RESOURCE.openai.azure.com",
    "Token": "YOUR_AZURE_KEY"
  }
}
```

## Troubleshooting

### "API Key Required" Error
**Problem**: Selected AI provider but no API key entered  
**Solution**: Enter your API key in the yellow field at the top of the AI Configuration section

### "Authentication Failed" Error
**Problem**: Invalid or expired API key  
**Solution**: 
- OpenAI: Get new key at https://platform.openai.com/api-keys (starts with `sk-`)
- GitHub: Get new token at https://github.com/settings/tokens (starts with `ghp_`)
- Groq: Get new token at https://console.groq.com
- Check for typos in the key

### "Invalid Key Format" Warning
**Problem**: Key doesn't match expected format  
**Solution**:
- OpenAI keys start with `sk-`
- GitHub tokens start with `ghp_` or `github_pat_`
- Azure keys are 32+ characters
- Groq tokens are alphanumeric strings (length may vary)

### "Rate Limited" Messages
**Problem**: Too many API requests  
**Solution**: Generator automatically retries with backoff. Enable caching to reduce requests.

### Generation Fails Partway Through
**Problem**: Network error or API issue during generation  
**Solution**: 
- Check internet connection
- Check the log for specific error (which room/NPC failed)
- Increase `MaxRetries` in settings
- Enable caching to avoid re-generating successful parts

### Different Output Each Run
**Problem**: Non-deterministic generation  
**Solution**:
- Ensure `Temperature: 0.0` in settings
- Enable caching
- Use the same seed value

### Clear Cache
```bash
rm -rf .aicache
```

## Development

Project structure:
```
SoloAdventureSystem.AIWorldGenerator/
??? Adapters/              # AI provider implementations
?   ??? ILocalSLMAdapter.cs
?   ??? GitHubModelsAdapter.cs
?   ??? OpenAIAdapter.cs
?   ??? CachedSLMAdapter.cs
?   ??? StubSLMAdapter.cs
?   ??? SLMAdapterFactory.cs
??? Configuration/         # Settings
?   ??? AISettings.cs
??? Generation/            # Core logic
?   ??? SeededWorldGenerator.cs
?   ??? WorldValidator.cs
?   ??? WorldExporter.cs
??? Models/                # Data models
??? UI/                    # Terminal.Gui interface
?   ??? WorldGeneratorUI.cs
??? Program.cs             # Entry point
```

## Example Workflows

```bash
# Quick interactive test
dotnet run

# Automated generation
dotnet run --name="AutoWorld" --seed=999 --regions=10

# Generate multiple worlds
for i in {1..5}; do
  dotnet run --name="World$i" --seed=$i --regions=10
done

# Debug mode
export LOGGING__LOGLEVEL__DEFAULT=Debug
dotnet run
```

## Output Structure

Generated worlds are exported as ZIP files to `content/worlds/`:

```
SoloAdventureWorld_<name>_<seed>.zip
??? world.json              # World metadata
??? rooms/
?   ??? room1.json
?   ??? ...
??? npcs/
?   ??? npc1.json
?   ??? ...
??? factions/
?   ??? faction1.json
??? story/
?   ??? story1.yaml
??? system/
    ??? seed.txt
    ??? generatorVersion.txt
```

## Cost Estimates (GitHub Models)

Using `gpt-4o-mini`:
- **10-room world**: ~$0.01
- **50-room world**: ~$0.05
- **100-room world**: ~$0.10

Caching drastically reduces costs on subsequent runs.

## Changelog

### v1.2 (2025)
- ? **Added Groq provider** - 100% FREE AI with no quota limits!
  - Lightning fast inference (faster than OpenAI)
  - Models: Llama 3.3 70B, Mixtral 8x7B, Gemma 2 9B
  - Sign up free at https://console.groq.com
  - Perfect for when OpenAI quota is exhausted
- ? FREE alternative for users without OpenAI credits
- ? Dynamic adapter recreation for runtime settings changes
- ? Improved error handling with specific error types
- ? Automatic retry logic with exponential/linear backoff
- ? Better progress tracking and logging
- ? Clear error messages with actionable guidance
- ? Fast failure for authentication errors
- ? Graceful handling of rate limits and network issues
- ? Fixed OpenAI adapter to use `OpenAIClient` (was using wrong Azure client causing 404 errors)

### v1.1 (2025)
- ? Dynamic adapter recreation for runtime settings changes
- ? Improved error handling with specific error types
- ? Automatic retry logic with exponential/linear backoff
- ? Better progress tracking and logging
- ? Clear error messages with actionable guidance
- ? Fast failure for authentication errors
- ? Graceful handling of rate limits and network issues
- ? Fixed OpenAI adapter to use `OpenAIClient` (was using wrong Azure client causing 404 errors)

### v1.0
- Initial release with Terminal.GUI interface
- Multi-provider support (Stub, GitHub, Azure, OpenAI)
- Caching system for deterministic generation
- CLI and interactive modes

## License

Part of the SoloAdventureSystem project.

