using Terminal.Gui;
using SoloAdventureSystem.ContentGenerator.Configuration;
using SoloAdventureSystem.ContentGenerator.Adapters;
using SoloAdventureSystem.ContentGenerator.Utils;
using SoloAdventureSystem.ContentGenerator;
using SoloAdventureSystem.ContentGenerator.Generation;
using SoloAdventureSystem.ContentGenerator.EmbeddedModel;
using SoloAdventureSystem.UI.Themes;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using NStack;

namespace SoloAdventureSystem.UI.WorldGenerator;

/// <summary>
/// Terminal UI for AI World Generator
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

    /// <summary>
    /// Shows the world generator UI and returns the path to the generated world, or null if cancelled.
    /// </summary>
    public string? GenerateWorld()
    {
        string? generatedWorldPath = null;
        
        var win = ComponentFactory.CreateWindow("AI World Generator");
        win.X = 0;
        win.Y = 0;
        win.Width = Dim.Fill();
        win.Height = Dim.Fill();

        // Title
        var title = ComponentFactory.CreateTitle("[ + ] AI World Generator");
        title.X = Pos.Center();
        title.Y = 1;

        // Configuration Frame
        var configFrame = ComponentFactory.CreateFrame("[ Configuration ]");
        configFrame.X = 1;
        configFrame.Y = 3;
        configFrame.Width = Dim.Fill(1);
        configFrame.Height = 14;

        var nameLabel = ComponentFactory.CreateLabel("Name:");
        nameLabel.X = 2;
        nameLabel.Y = 1;
        
        _nameField = ComponentFactory.CreateTextField("MyWorld");
        _nameField.X = 16;
        _nameField.Y = 1;
        _nameField.Width = 30;
        
        var seedLabel = ComponentFactory.CreateLabel("Seed:");
        seedLabel.X = 2;
        seedLabel.Y = 2;
        
        _seedField = ComponentFactory.CreateTextField("12345");
        _seedField.X = 16;
        _seedField.Y = 2;
        _seedField.Width = 30;

        var providerLabel = ComponentFactory.CreateLabel("Provider:");
        providerLabel.X = 2;
        providerLabel.Y = 4;
        
        _providerRadio = ComponentFactory.CreateRadioGroup(new ustring[] { "Stub (fast)", "LLamaSharp (AI)" });
        _providerRadio.X = 16;
        _providerRadio.Y = 4;
        _providerRadio.SelectedItem = 0;
        _providerRadio.SelectedItemChanged += _ => UpdateModelList();

        var modelLabel = ComponentFactory.CreateLabel("Model:");
        modelLabel.X = 2;
        modelLabel.Y = 8;
        
        _modelRadio = ComponentFactory.CreateRadioGroup(new ustring[] { "N/A" });
        _modelRadio.X = 16;
        _modelRadio.Y = 8;
        _modelRadio.SelectedItem = 0;

        configFrame.Add(nameLabel, _nameField, seedLabel, _seedField);
        configFrame.Add(providerLabel, _providerRadio, modelLabel, _modelRadio);

        // Progress Frame
        var progressFrame = ComponentFactory.CreateFrame("[ Progress ]");
        progressFrame.X = 1;
        progressFrame.Y = 18;
        progressFrame.Width = Dim.Fill(1);
        progressFrame.Height = Dim.Fill(4);

        _statusLabel = ComponentFactory.CreateAccentLabel("Ready");
        _statusLabel.X = 1;
        _statusLabel.Y = 1;
        
        _progressBar = ComponentFactory.CreateProgressBar();
        _progressBar.X = 1;
        _progressBar.Y = 2;
        _progressBar.Width = Dim.Fill(1);
        
        var logLabel = ComponentFactory.CreateMutedLabel("Log:");
        logLabel.X = 1;
        logLabel.Y = 4;
        
        _logView = ComponentFactory.CreateTextView();
        _logView.X = 1;
        _logView.Y = 5;
        _logView.Width = Dim.Fill(1);
        _logView.Height = Dim.Fill(1);
        _logView.ReadOnly = true;
        _logView.WordWrap = true;

        progressFrame.Add(_statusLabel, _progressBar, logLabel, _logView);

        // Buttons
        var generateBtn = ComponentFactory.CreatePrimaryButton("[ > ] Generate");
        generateBtn.X = 1;
        generateBtn.Y = Pos.AnchorEnd(1);
        generateBtn.Clicked += () => Task.Run(async () => 
        {
            generatedWorldPath = await GenerateWorldAsync();
            if (generatedWorldPath != null)
            {
                Application.RequestStop();
            }
        });
        
        var cancelBtn = ComponentFactory.CreateButton("[ < ] Cancel");
        cancelBtn.X = Pos.Right(generateBtn) + 2;
        cancelBtn.Y = Pos.AnchorEnd(1);
        cancelBtn.Clicked += () => Application.RequestStop();

        win.Add(title, configFrame, progressFrame, generateBtn, cancelBtn);
        
        UpdateModelList();
        Log("Ready. Select options and click Generate.");
        
        Application.Run(win);
        
        return generatedWorldPath;
    }

    private void UpdateModelList()
    {
        var models = _providerRadio.SelectedItem switch
        {
            0 => new ustring[] { "Stub (deterministic)" },
            1 => new ustring[] 
            { 
                GetModelDisplayName("phi-3-mini-q4", "Phi-3-mini Q4 (2GB)"),
                GetModelDisplayName("tinyllama-q4", "TinyLlama Q4 (600MB)"),
                GetModelDisplayName("llama-3.2-1b-q4", "Llama-3.2-1B Q4 (800MB)")
            },
            _ => new ustring[] { "N/A" }
        };

        Application.MainLoop.Invoke(() =>
        {
            _modelRadio.RadioLabels = models;
            _modelRadio.SelectedItem = 0;
            _modelRadio.SetNeedsDisplay();
        });
    }

    /// <summary>
    /// Gets display name for model, showing cached status.
    /// </summary>
    private string GetModelDisplayName(string modelKey, string baseName)
    {
        var isCached = GGUFModelDownloader.IsModelDownloaded(modelKey);
        return isCached ? $"{baseName} ? Cached" : baseName;
    }

    private async Task<string?> GenerateWorldAsync()
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
                    2 => "llama-3.2-1B Q4 (800MB)",
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
            Log($"[OK] Saved: {zipPath}");
            Log($"[>>] Location: {Path.GetDirectoryName(zipPath)}");

            Application.MainLoop.Invoke(() =>
            {
                MessageBox.Query("Success", $"World generated!\n\nName: {options.Name}\nSeed: {options.Seed}\n\nSaved to:\n{zipPath}", "OK");
            });
            
            return zipPath;
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
            
            return null;
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
