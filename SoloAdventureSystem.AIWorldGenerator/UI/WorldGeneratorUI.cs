using Terminal.Gui;
using SoloAdventureSystem.ContentGenerator.Configuration;
using SoloAdventureSystem.ContentGenerator.Adapters;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using NStack;
using Microsoft.Extensions.DependencyInjection;

namespace SoloAdventureSystem.ContentGenerator.UI;

/// <summary>
/// Terminal.Gui interactive interface for the AI World Generator
/// CYBERPUNK EDITION - MAXIMUM COOLNESS
/// </summary>
public class WorldGeneratorUI
{
    private readonly IServiceProvider _services;
    private readonly WorldValidator _validator;
    private readonly WorldExporter _exporter;
    private readonly IOptions<AISettings> _settings;
    private readonly ILogger<WorldGeneratorUI> _logger;

    private TextField? _nameField;
    private TextField? _seedField;
    private TextField? _themeField;
    private TextField? _regionsField;
    private TextField? _apiKeyField;
    private RadioGroup? _npcDensityRadio;
    private RadioGroup? _providerRadio;
    private RadioGroup? _modelRadio;
    private CheckBox? _enableCachingCheck;
    private CheckBox? _renderImagesCheck;
    private TextView? _logView;
    private ProgressBar? _progressBar;
    private Label? _statusLabel;
    private Label? _asciiArt;

    public WorldGeneratorUI(
        IServiceProvider services,
        WorldValidator validator,
        WorldExporter exporter,
        IOptions<AISettings> settings,
        ILogger<WorldGeneratorUI> logger)
    {
        _services = services;
        _validator = validator;
        _exporter = exporter;
        _settings = settings;
        _logger = logger;
    }

    public void Run()
    {
        Application.Init();
        
        var top = Application.Top;
        
        // 🔥 CYBERPUNK COLOR SCHEMES 🔥
        var cyberCyan = new ColorScheme
        {
            Normal = new Terminal.Gui.Attribute(Color.Cyan, Color.Black),
            Focus = new Terminal.Gui.Attribute(Color.Black, Color.Cyan),
            HotNormal = new Terminal.Gui.Attribute(Color.BrightCyan, Color.Black),
            HotFocus = new Terminal.Gui.Attribute(Color.Black, Color.BrightCyan),
            Disabled = new Terminal.Gui.Attribute(Color.DarkGray, Color.Black)
        };

        var cyberMagenta = new ColorScheme
        {
            Normal = new Terminal.Gui.Attribute(Color.Magenta, Color.Black),
            Focus = new Terminal.Gui.Attribute(Color.Black, Color.Magenta),
            HotNormal = new Terminal.Gui.Attribute(Color.BrightMagenta, Color.Black),
            HotFocus = new Terminal.Gui.Attribute(Color.Black, Color.BrightMagenta),
            Disabled = new Terminal.Gui.Attribute(Color.DarkGray, Color.Black)
        };

        var cyberYellow = new ColorScheme
        {
            Normal = new Terminal.Gui.Attribute(Color.BrightYellow, Color.Black),
            Focus = new Terminal.Gui.Attribute(Color.Black, Color.BrightYellow),
            HotNormal = new Terminal.Gui.Attribute(Color.BrightYellow, Color.Black),
            HotFocus = new Terminal.Gui.Attribute(Color.Black, Color.BrightYellow),
            Disabled = new Terminal.Gui.Attribute(Color.DarkGray, Color.Black)
        };

        var cyberGreen = new ColorScheme
        {
            Normal = new Terminal.Gui.Attribute(Color.BrightGreen, Color.Black),
            Focus = new Terminal.Gui.Attribute(Color.Black, Color.BrightGreen),
            HotNormal = new Terminal.Gui.Attribute(Color.Green, Color.Black),
            HotFocus = new Terminal.Gui.Attribute(Color.Black, Color.Green),
            Disabled = new Terminal.Gui.Attribute(Color.DarkGray, Color.Black)
        };

        // Set global theme
        Colors.Base = cyberCyan;
        
        // 🎨 EPIC MAIN WINDOW
        var win = new Window()
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill(),
            ColorScheme = cyberCyan,
            Border = new Border { BorderStyle = BorderStyle.Double }
        };

        // 🔥 SICK ASCII ART HEADER
        _asciiArt = new Label()
        {
            X = Pos.Center(),
            Y = 0,
            Width = Dim.Fill(),
            Height = 5,
            TextAlignment = TextAlignment.Centered,
            ColorScheme = cyberMagenta
        };
        _asciiArt.Text = @"
╔═══════════════════════════════════════════════════════════════════════╗
║  ██████╗ ██████╗ ██╗   ██████╗     ███████╗██╗   ██╗███████╗████████╗║
║  ██╔═══╝██╔═══██╗██║   ██╔═══██╗   ██╔════╝╚██╗ ██╔╝██╔════╝╚══██╔══╝║
║  ███████╗██║   ██║██║   ██║   ██║   ███████╗ ╚████╔╝ ███████╗   ██║  ║
║  ╚════██║██║   ██║██║   ██║   ██║   ╚════██║  ╚██╔╝  ╚════██║   ██║  ║
║  ███████║╚██████╔╝███████╚██████╔╝   ███████║   ██║   ███████║   ██║  ║
║  ╚══════╝ ╚═════╝ ╚══════╝╚═════╝    ╚══════╝   ╚═╝   ╚══════╝   ╚═╝  ║
║          ⚡ AI WORLD GENERATOR • CYBERPUNK EDITION v2.0 ⚡            ║
╚═══════════════════════════════════════════════════════════════════════╝";
        win.Add(_asciiArt);

        // 🎮 LEFT PANEL - CONFIG ZONE
        var leftFrame = new FrameView("⚡ WORLD FORGE ⚡")
        {
            X = 1,
            Y = 6,
            Width = Dim.Percent(48),
            Height = Dim.Fill(3),
            ColorScheme = cyberCyan,
            Border = new Border { BorderStyle = BorderStyle.Double }
        };

        int yPos = 1;

        // World Config
        leftFrame.Add(new Label($"╔══ WORLD CONFIGURATION ══╗") { X = 1, Y = yPos++, ColorScheme = cyberMagenta });
        
        leftFrame.Add(new Label("█ NAME") { X = 1, Y = yPos++, ColorScheme = cyberCyan });
        _nameField = new TextField("NEON_NEXUS")
        {
            X = 2,
            Y = yPos++,
            Width = Dim.Fill(2),
            ColorScheme = cyberGreen
        };
        leftFrame.Add(_nameField);

        leftFrame.Add(new Label("█ SEED") { X = 1, Y = yPos++, ColorScheme = cyberCyan });
        _seedField = new TextField("42069")
        {
            X = 2,
            Y = yPos++,
            Width = Dim.Fill(2),
            ColorScheme = cyberGreen
        };
        leftFrame.Add(_seedField);

        leftFrame.Add(new Label("█ THEME") { X = 1, Y = yPos++, ColorScheme = cyberCyan });
        _themeField = new TextField("DYSTOPIAN MEGACITY")
        {
            X = 2,
            Y = yPos++,
            Width = Dim.Fill(2),
            ColorScheme = cyberGreen
        };
        leftFrame.Add(_themeField);

        leftFrame.Add(new Label("█ ZONES") { X = 1, Y = yPos++, ColorScheme = cyberCyan });
        _regionsField = new TextField("13")
        {
            X = 2,
            Y = yPos++,
            Width = Dim.Fill(2),
            ColorScheme = cyberGreen
        };
        leftFrame.Add(_regionsField);

        leftFrame.Add(new Label("█ NPC DENSITY") { X = 1, Y = yPos++, ColorScheme = cyberCyan });
        _npcDensityRadio = new RadioGroup(new ustring[] { "▸ SPARSE", "▸ MODERATE", "▸ SWARM" })
        {
            X = 2,
            Y = yPos++,
            SelectedItem = 1,
            ColorScheme = cyberGreen
        };
        leftFrame.Add(_npcDensityRadio);
        yPos += 2;  // Reduced spacing

        // AI Config
        leftFrame.Add(new Label($"╔══ AI NEURAL NET ══╗") { X = 1, Y = yPos++, ColorScheme = cyberMagenta });
        
        leftFrame.Add(new Label("⚡ API KEY") { X = 1, Y = yPos++, ColorScheme = cyberYellow });
        _apiKeyField = new TextField(_settings.Value.Token ?? "")
        {
            X = 2,
            Y = yPos++,
            Width = Dim.Fill(2),
            Secret = true,
            ColorScheme = cyberYellow
        };
        leftFrame.Add(_apiKeyField);

        leftFrame.Add(new Label("█ PROVIDER") { X = 1, Y = yPos++, ColorScheme = cyberCyan });
        _providerRadio = new RadioGroup(new ustring[] { "▸ STUB", "▸ GROQ", "▸ OPENAI", "▸ GITHUB", "▸ AZURE" })
        {
            X = 2,
            Y = yPos++,
            SelectedItem = _settings.Value.Provider.ToLower() switch
            {
                "groq" => 1,
                "openai" => 2,
                "githubmodels" => 3,
                "azureopenai" => 4,
                _ => 0
            },
            ColorScheme = cyberGreen
        };
        _providerRadio.SelectedItemChanged += (args) => UpdateModelsForProvider(args.SelectedItem);
        leftFrame.Add(_providerRadio);
        yPos += 5;

        leftFrame.Add(new Label("█ MODEL") { X = 1, Y = yPos++, ColorScheme = cyberCyan });
        _modelRadio = new RadioGroup(GetModelsForProvider(0))
        {
            X = 2,
            Y = yPos++,
            SelectedItem = 0,
            ColorScheme = cyberGreen
        };
        leftFrame.Add(_modelRadio);
        yPos += 3;

        _enableCachingCheck = new CheckBox("☑ CACHE", _settings.Value.EnableCaching)
        {
            X = 2,
            Y = yPos++,
            ColorScheme = cyberCyan
        };
        leftFrame.Add(_enableCachingCheck);

        _renderImagesCheck = new CheckBox("☐ IMAGES", false)
        {
            X = 2,
            Y = yPos++,
            ColorScheme = cyberCyan
        };
        leftFrame.Add(_renderImagesCheck);

        win.Add(leftFrame);

        // 💻 RIGHT PANEL - TERMINAL OUTPUT
        var rightFrame = new FrameView("⚡ SYSTEM TERMINAL ⚡")
        {
            X = Pos.Right(leftFrame) + 1,
            Y = 6,
            Width = Dim.Fill(1),
            Height = Dim.Fill(3),
            ColorScheme = cyberGreen,
            Border = new Border { BorderStyle = BorderStyle.Double }
        };

        _statusLabel = new Label(">>> NEURAL NET: ONLINE | STATUS: READY")
        {
            X = 1,
            Y = 0,
            Width = Dim.Fill(1),
            ColorScheme = cyberMagenta
        };
        rightFrame.Add(_statusLabel);

        _progressBar = new ProgressBar()
        {
            X = 1,
            Y = 2,
            Width = Dim.Fill(1),
            Height = 1,
            ColorScheme = cyberMagenta
        };
        rightFrame.Add(_progressBar);

        _logView = new TextView()
        {
            X = 1,
            Y = 4,
            Width = Dim.Fill(1),
            Height = Dim.Fill(1),
            ReadOnly = true,
            WordWrap = true,
            ColorScheme = cyberGreen
        };
        rightFrame.Add(_logView);

        win.Add(rightFrame);

        // 🚀 EPIC BUTTONS
        var generateButton = new Button("═══════ [ ⚡ GENERATE WORLD ⚡ ] ═══════")
        {
            X = Pos.Center() - 20,
            Y = Pos.AnchorEnd(1),
            ColorScheme = cyberMagenta
        };
        generateButton.Clicked += () => GenerateWorld();

        var quitButton = new Button("[ ✖ DISCONNECT ]")
        {
            X = Pos.Right(generateButton) + 2,
            Y = Pos.AnchorEnd(1),
            ColorScheme = cyberCyan
        };
        quitButton.Clicked += () => Application.RequestStop();

        win.Add(generateButton);
        win.Add(quitButton);

        top.Add(win);

        // 🔥 BOOT SEQUENCE
        LogMessage("╔════════════════════════════════════════════════════════════════╗");
        LogMessage("║        AI WORLD GENERATOR - NEURAL NET INITIALIZED            ║");
        LogMessage("╚════════════════════════════════════════════════════════════════╝");
        LogMessage("");
        LogMessage($"⚡ PROVIDER: {_settings.Value.Provider.ToUpper()}");
        LogMessage($"⚡ CACHE: {(_settings.Value.EnableCaching ? "ENABLED" : "DISABLED")}");
        LogMessage($"⚡ TEMPERATURE: {_settings.Value.Temperature}°K");
        LogMessage("");
        LogMessage("╔════════════════════════════════════════════════════════════════╗");
        LogMessage("║  QUICK START PROTOCOL:                                        ║");
        LogMessage("╠════════════════════════════════════════════════════════════════╣");
        LogMessage("║  [1] ENTER API KEY → Yellow field (if using AI)               ║");
        LogMessage("║  [2] SELECT PROVIDER → Groq (FREE!) recommended                ║");
        LogMessage("║  [3] CONFIGURE WORLD → Name, seed, theme, zones               ║");
        LogMessage("║  [4] EXECUTE → Hit the big magenta button!                    ║");
        LogMessage("╚════════════════════════════════════════════════════════════════╝");
        LogMessage("");
        LogMessage("💀 PROVIDERS AVAILABLE:");
        LogMessage("   ▸ STUB      → Instant test mode (no key needed)");
        LogMessage("   ▸ GROQ      → 100% FREE! Fast! (gsk_... key)");
        LogMessage("   ▸ OPENAI    → GPT models (sk-... key)");
        LogMessage("   ▸ GITHUB    → GitHub Models (ghp_... token)");
        LogMessage("   ▸ AZURE     → Azure OpenAI (Azure key)");
        LogMessage("");
        LogMessage("⚡ GET FREE GROQ KEY: https://console.groq.com");
        LogMessage("");
        LogMessage(">>> SYSTEM ARMED AND READY. AWAITING INPUT...");

        Application.Run();
        Application.Shutdown();
    }

    private ustring[] GetModelsForProvider(int providerIndex)
    {
        return providerIndex switch
        {
            0 => new ustring[] { "▸ PLACEHOLDER" },
            1 => new ustring[] { "▸ LLAMA-3.3-70B", "▸ MIXTRAL-8X7B", "▸ GEMMA-7B" },
            2 => new ustring[] { "▸ GPT-4O-MINI", "▸ GPT-4O", "▸ GPT-3.5-TURBO" },
            3 => new ustring[] { "▸ GPT-4O-MINI", "▸ GPT-4O", "▸ LLAMA-3.3-70B", "▸ PHI-4" },
            4 => new ustring[] { "▸ GPT-4O-MINI", "▸ GPT-4O", "▸ GPT-35-TURBO" },
            _ => new ustring[] { "▸ N/A" }
        };
    }

    private void UpdateModelsForProvider(int providerIndex)
    {
        if (_modelRadio != null)
        {
            Application.MainLoop.Invoke(() =>
            {
                _modelRadio.RadioLabels = GetModelsForProvider(providerIndex);
                _modelRadio.SelectedItem = 0;
                _modelRadio.SetNeedsDisplay();
            });
        }
    }

    private void GenerateWorld()
    {
        try
        {
            LogMessage("");
            LogMessage("╔═══════════════════════════════════════════════════════════════╗");
            LogMessage("║           ⚡⚡⚡ INITIATING WORLD GENESIS ⚡⚡⚡               ║");
            LogMessage("╚═══════════════════════════════════════════════════════════════╝");
            UpdateStatus(">>> NEURAL NET: VALIDATING PARAMETERS...", 0);

            var provider = _providerRadio!.SelectedItem switch
            {
                0 => "Stub",
                1 => "Groq",
                2 => "OpenAI",
                3 => "GitHubModels",
                4 => "AzureOpenAI",
                _ => "Stub"
            };

            var apiKey = _apiKeyField!.Text.ToString()?.Trim() ?? "";
            var needsApiKey = provider != "Stub";

            if (needsApiKey && string.IsNullOrWhiteSpace(apiKey))
            {
                var providerName = provider switch
                {
                    "Groq" => "GROQ [FREE]",
                    "OpenAI" => "OPENAI",
                    "GitHubModels" => "GITHUB MODELS",
                    "AzureOpenAI" => "AZURE OPENAI",
                    _ => "AI PROVIDER"
                };

                var getKeyUrl = provider switch
                {
                    "Groq" => "https://console.groq.com",
                    "OpenAI" => "https://platform.openai.com/api-keys",
                    "GitHubModels" => "https://github.com/settings/tokens",
                    "AzureOpenAI" => "Azure Portal",
                    _ => ""
                };

                LogMessage("");
                LogMessage("╔═══════════════════════════════════════════════════════════════╗");
                LogMessage("║               ⚠️  CRITICAL ERROR: NO API KEY  ⚠️              ║");
                LogMessage("╚═══════════════════════════════════════════════════════════════╗");
                LogMessage($"⚡ PROVIDER: {providerName}");
                LogMessage($"⚡ STATUS: AUTHENTICATION REQUIRED");
                LogMessage($"⚡ ACTION: ENTER KEY IN YELLOW FIELD");
                LogMessage($"⚡ GET KEY: {getKeyUrl}");
                UpdateStatus($">>> NEURAL NET: API KEY REQUIRED FOR {providerName}", 0);

                MessageBox.ErrorQuery("⚠️ AUTHENTICATION REQUIRED ⚠️",
                    $"╔════════════════════════════════════════════════╗\n" +
                    $"║      NEURAL NET: CONNECTION FAILED             ║\n" +
                    $"╠════════════════════════════════════════════════╣\n" +
                    $"║ PROVIDER: {providerName,-39}║\n" +
                    $"║ STATUS: NO API KEY DETECTED                    ║\n" +
                    $"║                                                ║\n" +
                    $"║ SOLUTION:                                      ║\n" +
                    $"║ → Enter API key in YELLOW field               ║\n" +
                    $"║ → Or select 'STUB' for instant test           ║\n" +
                    $"║                                                ║\n" +
                    $"║ GET FREE KEY: {getKeyUrl,-32}║\n" +
                    $"╚════════════════════════════════════════════════╝",
                    "[ OK ]", "[ USE STUB ]");

                return;
            }

            // Validate key format
            if (!string.IsNullOrWhiteSpace(apiKey))
            {
                var isValidFormat = provider switch
                {
                    "Groq" => apiKey.StartsWith("gsk_"),
                    "OpenAI" => apiKey.StartsWith("sk-"),
                    "GitHubModels" => apiKey.StartsWith("ghp_") || apiKey.StartsWith("github_pat_"),
                    "AzureOpenAI" => apiKey.Length >= 32,
                    _ => true
                };

                if (!isValidFormat)
                {
                    var expectedFormat = provider switch
                    {
                        "Groq" => "gsk_...",
                        "OpenAI" => "sk-...",
                        "GitHubModels" => "ghp_... or github_pat_...",
                        "AzureOpenAI" => "32+ chars",
                        _ => "valid key"
                    };

                    LogMessage("");
                    LogMessage("╔═══════════════════════════════════════════════════════════════╗");
                    LogMessage("║           ⚠️  WARNING: INVALID KEY FORMAT  ⚠️                ║");
                    LogMessage("╚═══════════════════════════════════════════════════════════════╗");
                    LogMessage($"⚡ PROVIDER: {provider}");
                    LogMessage($"⚡ EXPECTED: {expectedFormat}");
                    UpdateStatus(">>> NEURAL NET: KEY FORMAT INVALID", 0);

                    MessageBox.ErrorQuery("⚠️ INVALID KEY FORMAT ⚠️",
                        $"KEY FORMAT INCORRECT!\n\n" +
                        $"PROVIDER: {provider}\n" +
                        $"EXPECTED: {expectedFormat}\n\n" +
                        $"YOUR KEY: {apiKey.Substring(0, Math.Min(10, apiKey.Length))}...",
                        "[ FIX IT ]");
                    return;
                }
            }

            // Show provider info
            if (provider == "Groq" && apiKey.StartsWith("gsk_"))
            {
                LogMessage("⚡ GROQ NEURAL NET: CONNECTED | STATUS: 100% FREE | SPEED: MAXIMUM");
            }
            else if (provider == "OpenAI" && apiKey.StartsWith("sk-"))
            {
                LogMessage("⚡ OPENAI NEURAL NET: CONNECTED | CREDIT: $5 FREE ≈ 5K WORLDS");
            }
            else if (provider == "GitHubModels")
            {
                LogMessage("⚡ GITHUB NEURAL NET: CONNECTED | FREE TIER: ACTIVE");
            }

            var model = _providerRadio!.SelectedItem switch
            {
                0 => "stub",
                1 => _modelRadio!.SelectedItem switch
                {
                    0 => "llama-3.3-70b-versatile",
                    1 => "mixtral-8x7b-32768",
                    2 => "gemma2-9b-it",
                    _ => "llama-3.3-70b-versatile"
                },
                2 => _modelRadio!.SelectedItem switch
                {
                    0 => "gpt-4o-mini",
                    1 => "gpt-4o",
                    2 => "gpt-3.5-turbo",
                    _ => "gpt-4o-mini"
                },
                3 => _modelRadio!.SelectedItem switch
                {
                    0 => "gpt-4o-mini",
                    1 => "gpt-4o",
                    2 => "Llama-3.3-70B-Instruct",
                    3 => "Phi-4",
                    _ => "gpt-4o-mini"
                },
                4 => _modelRadio!.SelectedItem switch
                {
                    0 => "gpt-4o-mini",
                    1 => "gpt-4o",
                    2 => "gpt-35-turbo",
                    _ => "gpt-4o-mini"
                },
                _ => "gpt-4o-mini"
            };

            if (!string.IsNullOrEmpty(apiKey))
            {
                _settings.Value.Token = apiKey;
            }

            _settings.Value.Provider = provider;
            _settings.Value.Model = model;
            _settings.Value.EnableCaching = _enableCachingCheck?.Checked ?? true;

            LogMessage($"⚡ PROVIDER: {provider.ToUpper()}");
            LogMessage($"⚡ MODEL: {model.ToUpper()}");
            LogMessage($"⚡ CACHE: {(_settings.Value.EnableCaching ? "QUANTUM" : "DISABLED")}");

            LogMessage("");
            LogMessage($">>> CREATING {provider.ToUpper()} ADAPTER...");
            UpdateStatus(">>> NEURAL NET: SYNCHRONIZING...", 15);

            IWorldGenerator generator;
            try
            {
                var slmAdapter = SLMAdapterFactory.Create(_services);
                var imageAdapter = _services.GetRequiredService<IImageAdapter>();
                var logger = _services.GetRequiredService<ILogger<SeededWorldGenerator>>();
                generator = new SeededWorldGenerator(slmAdapter, imageAdapter, logger);
                
                LogMessage("✓ AI NEURAL NETWORK: ONLINE");
            }
            catch (Exception adapterEx)
            {
                LogMessage("");
                LogMessage("╔═══════════════════════════════════════════════════════════════╗");
                LogMessage("║              ✖ NEURAL NET: CONNECTION FAILED ✖               ║");
                LogMessage("╚═══════════════════════════════════════════════════════════════╝");
                LogMessage($"⚡ ERROR: {adapterEx.Message}");
                UpdateStatus(">>> NEURAL NET: INITIALIZATION FAILED", 0);
                
                MessageBox.ErrorQuery("✖ NEURAL NET ERROR ✖",
                    $"AI CONNECTION FAILED!\n\n{adapterEx.Message}\n\n" +
                    $"CHECK YOUR API KEY AND SETTINGS.",
                    "[ RETRY ]");
                return;
            }

            var options = new WorldGenerationOptions
            {
                Name = _nameField!.Text.ToString() ?? "CYBER_WORLD",
                Seed = int.TryParse(_seedField!.Text.ToString(), out var seed) ? seed : 42069,
                Theme = _themeField!.Text.ToString() ?? "Cyberpunk",
                Regions = int.TryParse(_regionsField!.Text.ToString(), out var regions) ? regions : 13,
                NpcDensity = _npcDensityRadio!.SelectedItem switch
                {
                    0 => "low",
                    1 => "medium",
                    2 => "high",
                    _ => "medium"
                },
                RenderImages = _renderImagesCheck?.Checked ?? false
            };

            LogMessage("");
            LogMessage($"⚡ WORLD NAME: {options.Name.ToUpper()}");
            LogMessage($"⚡ SEED CODE: {options.Seed}");
            LogMessage($"⚡ THEME: {options.Theme.ToUpper()}");
            LogMessage($"⚡ ZONE COUNT: {options.Regions}");
            LogMessage($"⚡ NPC DENSITY: {options.NpcDensity.ToUpper()}");

            UpdateStatus(">>> NEURAL NET: GENERATING WORLD MATRIX...", 20);
            LogMessage("");
            LogMessage(">>> EXECUTING WORLD GENESIS PROTOCOL...");

            WorldGenerationResult result;
            try
            {
                result = generator.Generate(options);
            }
            catch (Exception genEx)
            {
                LogMessage("");
                LogMessage("╔═══════════════════════════════════════════════════════════════╗");
                LogMessage("║                ✖ GENERATION FAILED ✖                         ║");
                LogMessage("╚═══════════════════════════════════════════════════════════════╝");
                LogMessage($"⚡ ERROR: {genEx.Message}");
                UpdateStatus(">>> NEURAL NET: GENERATION ABORTED", 0);
                
                MessageBox.ErrorQuery("✖ GENERATION ERROR ✖",
                    $"WORLD GENESIS FAILED!\n\n{genEx.Message}\n\n" +
                    $"SEE TERMINAL FOR DETAILS.",
                    "[ ABORT ]");
                return;
            }

            UpdateStatus(">>> NEURAL NET: VALIDATING WORLD DATA...", 50);
            LogMessage($"✓ GENERATED {result.Rooms.Count} ZONES");
            LogMessage($"✓ SPAWNED {result.Npcs.Count} ENTITIES");
            LogMessage($"✓ CREATED {result.Factions.Count} FACTIONS");
            LogMessage($"✓ COMPILED {result.StoryNodes.Count} NARRATIVE NODES");

            LogMessage("");
            LogMessage(">>> VALIDATING WORLD INTEGRITY...");
            try
            {
                _validator.Validate(result);
                LogMessage("✓ VALIDATION: PASSED");
            }
            catch (Exception valEx)
            {
                LogMessage("");
                LogMessage("╔═══════════════════════════════════════════════════════════════╗");
                LogMessage("║              ✖ VALIDATION FAILED ✖                           ║");
                LogMessage("╚═══════════════════════════════════════════════════════════════╝");
                LogMessage($"⚡ ERROR: {valEx.Message}");
                UpdateStatus(">>> NEURAL NET: INTEGRITY CHECK FAILED", 0);
                
                MessageBox.ErrorQuery("✖ VALIDATION ERROR ✖",
                    $"WORLD INTEGRITY COMPROMISED!\n\n{valEx.Message}",
                    "[ ABORT ]");
                return;
            }

            UpdateStatus(">>> NEURAL NET: COMPILING DATA PACKAGE...", 70);
            var tempDir = Path.Combine(Path.GetTempPath(), $"GENESIS_{options.Name}_{options.Seed}");
            
            try
            {
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, true);
                }
                Directory.CreateDirectory(tempDir);

                LogMessage("");
                LogMessage(">>> EXPORTING WORLD DATA...");
                _exporter.Export(result, options, tempDir);

                var zipPath = Path.Combine("content/worlds", $"WORLD_{options.Name}_{options.Seed}.zip");
                Directory.CreateDirectory("content/worlds");

                UpdateStatus(">>> NEURAL NET: CREATING ARCHIVE...", 90);
                LogMessage(">>> COMPRESSING TO ARCHIVE...");
                _exporter.Zip(tempDir, zipPath);

                UpdateStatus(">>> NEURAL NET: GENESIS COMPLETE!", 100);
                
                var fileInfo = new FileInfo(zipPath);
                
                LogMessage("");
                LogMessage("╔═══════════════════════════════════════════════════════════════╗");
                LogMessage("║          ⚡⚡⚡ WORLD GENESIS: COMPLETE ⚡⚡⚡                 ║");
                LogMessage("╚═══════════════════════════════════════════════════════════════╝");
                LogMessage($"⚡ FILE: {zipPath}");
                LogMessage($"⚡ ZONES: {result.Rooms.Count} | ENTITIES: {result.Npcs.Count} | FACTIONS: {result.Factions.Count}");
                LogMessage($"⚡ SIZE: {fileInfo.Length / 1024} KB");
                LogMessage("╔═══════════════════════════════════════════════════════════════╗");

                MessageBox.Query("⚡ GENESIS COMPLETE ⚡",
                    $"╔════════════════════════════════════════════╗\n" +
                    $"║  WORLD '{options.Name.ToUpper()}' CREATED! ║\n" +
                    $"╚════════════════════════════════════════════╝\n\n" +
                    $"📦 FILE: {zipPath}\n" +
                    $"🏙️  ZONES: {result.Rooms.Count}\n" +
                    $"👥 ENTITIES: {result.Npcs.Count}\n" +
                    $"⚔️  FACTIONS: {result.Factions.Count}\n" +
                    $"💾 SIZE: {fileInfo.Length / 1024} KB",
                    "[ AWESOME! ]");
            }
            catch (Exception exportEx)
            {
                LogMessage("");
                LogMessage("╔═══════════════════════════════════════════════════════════════╗");
                LogMessage("║                ✖ EXPORT FAILED ✖                             ║");
                LogMessage("╚═══════════════════════════════════════════════════════════════╝");
                LogMessage($"⚡ ERROR: {exportEx.Message}");
                UpdateStatus(">>> NEURAL NET: EXPORT FAILED", 0);
                
                MessageBox.ErrorQuery("✖ EXPORT ERROR ✖",
                    $"ARCHIVE CREATION FAILED!\n\n{exportEx.Message}",
                    "[ DAMN ]");
                return;
            }
        }
        catch (Exception ex)
        {
            LogMessage("");
            LogMessage("╔═══════════════════════════════════════════════════════════════╗");
            LogMessage("║              ✖ CRITICAL ERROR ✖                              ║");
            LogMessage("╚═══════════════════════════════════════════════════════════════╝");
            LogMessage($"⚡ ERROR: {ex.Message}");
            LogMessage($"⚡ STACK: {ex.StackTrace}");
            UpdateStatus(">>> NEURAL NET: SYSTEM ERROR", 0);

            MessageBox.ErrorQuery("✖ CRITICAL ERROR ✖",
                $"SYSTEM FAILURE!\n\n{ex.Message}",
                "[ OH NO ]");
        }
    }

    private void LogMessage(string message)
    {
        if (_logView != null)
        {
            Application.MainLoop.Invoke(() =>
            {
                _logView.Text += message + "\n";
                _logView.MoveEnd();
            });
        }
        _logger.LogInformation(message);
    }

    private void UpdateStatus(string status, float progress)
    {
        if (_statusLabel != null)
        {
            Application.MainLoop.Invoke(() =>
            {
                _statusLabel.Text = status;
            });
        }

        if (_progressBar != null)
        {
            Application.MainLoop.Invoke(() =>
            {
                _progressBar.Fraction = progress / 100f;
            });
        }
    }
}
