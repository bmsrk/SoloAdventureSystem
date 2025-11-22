using Terminal.Gui;
using SoloAdventureSystem.ContentGenerator.Configuration;
using SoloAdventureSystem.ContentGenerator.Adapters;
using SoloAdventureSystem.ContentGenerator.Utils;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using SoloAdventureSystem.ContentGenerator.EmbeddedModel;
using NStack;

namespace SoloAdventureSystem.ContentGenerator.UI;

/// <summary>
/// Minimalistic Terminal UI for AI World Generator
/// </summary>
public class WorldGeneratorUI
{
    private readonly IServiceProvider _services;
    private readonly WorldValidator _validator;
    private readonly WorldExporter _exporter;
    private readonly IOptions<AISettings> _settings;
    private readonly ILogger<WorldGeneratorUI> _logger;

    // UI Components
    private TextField _nameField = null!;
    private TextField _seedField = null!;
    private RadioGroup _providerRadio = null!;
    private RadioGroup _modelRadio = null!;
    private TextView _logView = null!;
    private ProgressBar _progressBar = null!;
    private Label _statusLabel = null!;

    // Cached adapters to avoid re-downloading models
    private LLamaSharpAdapter? _cachedLlamaAdapter;
    private string? _cachedModelKey;

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
        
        try
        {
            MinimalTheme.Apply();
            var top = Application.Top;
            
            var win = new Window("AI World Generator")
            {
                X = 0, Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill()
            };

            // Config section
            var nameLabel = new Label("Name:") { X = 1, Y = 1 };
            _nameField = new TextField("MyWorld") { X = 15, Y = 1, Width = 30 };
            
            var seedLabel = new Label("Seed:") { X = 1, Y = 2 };
            _seedField = new TextField("12345") { X = 15, Y = 2, Width = 30 };

            var providerLabel = new Label("Provider:") { X = 1, Y = 4 };
            _providerRadio = new RadioGroup(new ustring[] { "Stub (fast)", "LLamaSharp (AI)" })
            {
                X = 15, Y = 4,
                SelectedItem = 0
            };
            _providerRadio.SelectedItemChanged += _ => UpdateModelList();

            var modelLabel = new Label("Model:") { X = 1, Y = 8 };
            _modelRadio = new RadioGroup(new ustring[] { "N/A" })
            {
                X = 15, Y = 8,
                SelectedItem = 0
            };

            // Output section
            _statusLabel = new Label("Ready") { X = 1, Y = 13, ColorScheme = MinimalTheme.Accent };
            _progressBar = new ProgressBar { X = 1, Y = 14, Width = Dim.Fill(1) };
            _logView = new TextView
            {
                X = 1, Y = 16,
                Width = Dim.Fill(1),
                Height = Dim.Fill(3),
                ReadOnly = true,
                WordWrap = true
            };

            // Buttons
            var generateBtn = new Button("Generate") { X = 1, Y = Pos.AnchorEnd(1) };
            generateBtn.Clicked += () => Task.Run(async () => await GenerateWorldAsync());
            
            var quitBtn = new Button("Quit") { X = Pos.Right(generateBtn) + 2, Y = Pos.AnchorEnd(1) };
            quitBtn.Clicked += () => Application.RequestStop();

            win.Add(nameLabel, _nameField, seedLabel, _seedField);
            win.Add(providerLabel, _providerRadio, modelLabel, _modelRadio);
            win.Add(_statusLabel, _progressBar, _logView);
            win.Add(generateBtn, quitBtn);

            top.Add(win);
            
            UpdateModelList();
            Log("Ready. Select options and click Generate.");
            
            Application.Run();
        }
        finally
        {
            _cachedLlamaAdapter?.Dispose();
            Application.Shutdown();
        }
    }

    private void UpdateModelList()
    {
        var models = _providerRadio.SelectedItem switch
        {
            0 => new ustring[] { "Stub (deterministic)" },
            1 => new ustring[] { "Phi-3-mini Q4 (2GB)", "TinyLlama Q4 (600MB)", "Llama-3.2-1B Q4 (800MB)" },
            _ => new ustring[] { "N/A" }
        };

        Application.MainLoop.Invoke(() =>
        {
            _modelRadio.RadioLabels = models;
            _modelRadio.SelectedItem = 0;
            _modelRadio.SetNeedsDisplay();
        });
    }

    private async Task GenerateWorldAsync()
    {
        try
        {
            Log("Starting generation...");
            UpdateStatus("Initializing", 0);

            var provider = _providerRadio.SelectedItem == 0 ? "Stub" : "LLamaSharp";
            var model = _providerRadio.SelectedItem switch
            {
                0 => "stub",
                1 => _modelRadio.SelectedItem switch
                {
                    0 => "phi-3-mini-q4",
                    1 => "tinyllama-q4",
                    2 => "llama-3.2-1b-q4",
                    _ => "phi-3-mini-q4"
                },
                _ => "stub"
            };

            _settings.Value.Provider = provider;
            _settings.Value.Model = model;
            if (provider == "LLamaSharp")
                _settings.Value.LLamaModelKey = model;

            Log($"Provider: {provider}, Model: {model}");

            // Initialize adapter (with caching for LLamaSharp)
            ILocalSLMAdapter slmAdapter;
            if (provider == "LLamaSharp")
            {
                try
                {
                    // Check if we can reuse cached adapter
                    if (_cachedLlamaAdapter != null && _cachedModelKey == model)
                    {
                        Log("Using cached LLamaSharp adapter...");
                        slmAdapter = _cachedLlamaAdapter;
                    }
                    else
                    {
                        // Dispose old adapter if exists
                        if (_cachedLlamaAdapter != null)
                        {
                            Log("Model changed, disposing old adapter...");
                            _cachedLlamaAdapter.Dispose();
                            _cachedLlamaAdapter = null;
                        }

                        UpdateStatus("Checking model...", 5);
                        Log("Initializing LLamaSharp adapter...");
                        
                        var llamaAdapter = new LLamaSharpAdapter(_settings, _logger as ILogger<LLamaSharpAdapter>);
                        
                        var progress = new Progress<DownloadProgress>(p =>
                        {
                            var pct = p.PercentComplete;
                            var speedMB = p.SpeedBytesPerSecond / 1024.0 / 1024.0;
                            var downloadedMB = p.DownloadedBytes / 1024.0 / 1024.0;
                            var totalMB = p.TotalBytes / 1024.0 / 1024.0;
                            
                            UpdateStatus($"Downloading: {downloadedMB:F0}/{totalMB:F0} MB ({speedMB:F1} MB/s)", 5 + (pct * 0.4f));
                            Log($"Download progress: {pct}% - {speedMB:F1} MB/s");
                        });

                        Log("Downloading/loading model (this may take a while)...");
                        await llamaAdapter.InitializeAsync(progress);
                        Log("Model loaded successfully!");
                        
                        // Cache the adapter
                        _cachedLlamaAdapter = llamaAdapter;
                        _cachedModelKey = model;
                        slmAdapter = llamaAdapter;
                    }
                }
                catch (Exception llamaEx)
                {
                    Log($"LLamaSharp initialization failed: {llamaEx.Message}");
                    Log("Falling back to Stub adapter...");
                    _logger.LogWarning(llamaEx, "LLamaSharp failed, using Stub");
                    
                    // Fall back to Stub
                    _settings.Value.Provider = "Stub";
                    slmAdapter = SLMAdapterFactory.Create(_services);
                }
            }
            else
            {
                slmAdapter = SLMAdapterFactory.Create(_services);
            }

            var imageAdapter = _services.GetRequiredService<IImageAdapter>();
            var logger = _services.GetRequiredService<ILogger<SeededWorldGenerator>>();
            var generator = new SeededWorldGenerator(slmAdapter, imageAdapter, logger);

            UpdateStatus("Generating world", 50);
            var options = new WorldGenerationOptions
            {
                Name = _nameField.Text.ToString() ?? "MyWorld",
                Seed = int.TryParse(_seedField.Text.ToString(), out var s) ? s : 12345,
                Theme = "Cyberpunk",
                Regions = 5,
                NpcDensity = "medium"
            };

            Log($"Generating '{options.Name}' (seed: {options.Seed})...");
            var result = generator.Generate(options);

            UpdateStatus("Validating", 70);
            _validator.Validate(result);
            Log($"Generated: {result.Rooms.Count} rooms, {result.Npcs.Count} NPCs");

            UpdateStatus("Exporting", 80);
            var tempDir = Path.Combine(Path.GetTempPath(), $"World_{options.Name}_{options.Seed}");
            if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
            Directory.CreateDirectory(tempDir);

            _exporter.Export(result, options, tempDir);

            // Use shared PathHelper to get consistent world storage location
            var zipPath = PathHelper.GetWorldZipPath(options.Name, options.Seed);
            var worldsDir = Path.GetDirectoryName(zipPath);
            
            Log($"Saving to shared worlds directory: {worldsDir}");

            UpdateStatus("Creating archive...", 90);
            _exporter.Zip(tempDir, zipPath);

            UpdateStatus("Complete", 100);
            Log($"✓ Saved: {zipPath}");
            Log($"✓ Location: {Path.GetDirectoryName(zipPath)}");

            Application.MainLoop.Invoke(() =>
            {
                MessageBox.Query("Success", $"World generated!\n\nName: {options.Name}\nSeed: {options.Seed}\n\nSaved to:\n{zipPath}", "OK");
            });
        }
        catch (Exception ex)
        {
            Log($"ERROR: {ex.Message}");
            Log($"Stack trace: {ex.StackTrace}");
            _logger.LogError(ex, "Generation failed");
            UpdateStatus("Failed", 0);
            
            Application.MainLoop.Invoke(() =>
            {
                MessageBox.ErrorQuery("Error", $"{ex.Message}\n\nCheck the log for details.", "OK");
            });
        }
    }

    private void Log(string message)
    {
        Application.MainLoop.Invoke(() =>
        {
            _logView.Text += $"{DateTime.Now:HH:mm:ss} {message}\n";
            _logView.MoveEnd();
        });
        _logger.LogInformation(message);
    }

    private void UpdateStatus(string status, float progress)
    {
        Application.MainLoop.Invoke(() =>
        {
            _statusLabel.Text = status;
            _progressBar.Fraction = progress / 100f;
        });
    }
}
