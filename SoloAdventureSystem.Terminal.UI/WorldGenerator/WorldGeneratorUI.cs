using Terminal.Gui;
using SoloAdventureSystem.ContentGenerator.Configuration;
using SoloAdventureSystem.ContentGenerator.Adapters;
using SoloAdventureSystem.ContentGenerator.Utils;
using SoloAdventureSystem.ContentGenerator.Generation;
using SoloAdventureSystem.ContentGenerator.Models;
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
public class WorldGeneratorUI : IDisposable
{
    private readonly IServiceProvider _services;
    private readonly WorldValidator _validator;
    private readonly WorldExporter _exporter;
    private readonly IOptions<AISettings> _settings;
    private readonly ILogger<WorldGeneratorUI> _logger;

    // UI Components
    private TextField _nameField = null!;
    private TextField _seedField = null!;
    private TextField _flavorField = null!;
    private TextField _descriptionField = null!;
    private TextField _plotField = null!;
    private TextField _timePeriodField = null!;
    private TextField _regionsField = null!;
    private RadioGroup _modelRadio = null!;
    private TextView _logView = null!;
    private ProgressBar _progressBar = null!;
    private Label _statusLabel = null!;

    // Cached adapters to avoid re-downloading models
    private LLamaSharpAdapter? _cachedLlamaAdapter;
    private string? _cachedModelKey;
    private CancellationTokenSource? _cancellationTokenSource;

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
    /// Disposes cached resources to prevent memory leaks.
    /// </summary>
    public void Dispose()
    {
        if (_cachedLlamaAdapter != null)
        {
            _logger?.LogInformation("Disposing cached LLamaSharp adapter to free memory...");
            _cachedLlamaAdapter.Dispose();
            _cachedLlamaAdapter = null;
            _cachedModelKey = null;
        }
        
        _cancellationTokenSource?.Dispose();
        GC.SuppressFinalize(this);
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

        // Configuration Frame - Improved layout with wider fields
        var configFrame = ComponentFactory.CreateFrame("[ Configuration ]");
        configFrame.X = 1;
        configFrame.Y = 3;
        configFrame.Width = Dim.Fill(1);
        configFrame.Height = 16;

        // Row 1: Name and Regions (side by side)
        var nameLabel = ComponentFactory.CreateLabel("Name:");
        nameLabel.X = 2;
        nameLabel.Y = 1;
        
        _nameField = ComponentFactory.CreateTextField("MyWorld");
        _nameField.X = 16;
        _nameField.Y = 1;
        _nameField.Width = Dim.Percent(50);
        
        var regionsLabel = ComponentFactory.CreateLabel("Regions:");
        regionsLabel.X = Pos.Percent(55);
        regionsLabel.Y = 1;
        
        _regionsField = ComponentFactory.CreateTextField("5");
        _regionsField.X = Pos.Right(regionsLabel) + 2;
        _regionsField.Y = 1;
        _regionsField.Width = 8;
        
        // Row 2: Seed
        var seedLabel = ComponentFactory.CreateLabel("Seed:");
        seedLabel.X = 2;
        seedLabel.Y = 2;
        
        _seedField = ComponentFactory.CreateTextField("12345");
        _seedField.X = 16;
        _seedField.Y = 2;
        _seedField.Width = Dim.Percent(50);
        
        // Row 3: Flavor (full width)
        var flavorLabel = ComponentFactory.CreateLabel("Flavor:");
        flavorLabel.X = 2;
        flavorLabel.Y = 3;
        
        _flavorField = ComponentFactory.CreateTextField("Dark and mysterious");
        _flavorField.X = 16;
        _flavorField.Y = 3;
        _flavorField.Width = Dim.Fill(3);
        
        // Row 4: Setting (full width)
        var descLabel = ComponentFactory.CreateLabel("Setting:");
        descLabel.X = 2;
        descLabel.Y = 4;
        
        _descriptionField = ComponentFactory.CreateTextField("A cyberpunk megacity ruled by AI");
        _descriptionField.X = 16;
        _descriptionField.Y = 4;
        _descriptionField.Width = Dim.Fill(3);
        
        // Row 5: Main Plot (full width)
        var plotLabel = ComponentFactory.CreateLabel("Main Plot:");
        plotLabel.X = 2;
        plotLabel.Y = 5;
        
        _plotField = ComponentFactory.CreateTextField("Uncover the conspiracy behind the disappearances");
        _plotField.X = 16;
        _plotField.Y = 5;
        _plotField.Width = Dim.Fill(3);
        
        // Row 6: Time Period (full width)
        var timeLabel = ComponentFactory.CreateLabel("Time Period:");
        timeLabel.X = 2;
        timeLabel.Y = 6;
        
        _timePeriodField = ComponentFactory.CreateTextField("Near future (2077)");
        _timePeriodField.X = 16;
        _timePeriodField.Y = 6;
        _timePeriodField.Width = Dim.Fill(3);

        // Row 7-9: Model selection
        var modelLabel = ComponentFactory.CreateLabel("Model:");
        modelLabel.X = 2;
        modelLabel.Y = 8;
        
        _modelRadio = ComponentFactory.CreateRadioGroup(new ustring[] 
        { 
            GetModelDisplayName("phi-3-mini-q4", "Phi-3-mini Q4 (2GB) - Best quality"),
            GetModelDisplayName("tinyllama-q4", "TinyLlama Q4 (600MB) - Fastest"),
            GetModelDisplayName("llama-3.2-1b-q4", "Llama-3.2-1B Q4 (800MB) - Balanced")
        });
        _modelRadio.X = 16;
        _modelRadio.Y = 8;
        _modelRadio.SelectedItem = 1; // Default to TinyLlama for speed
        
        // Helpful tip
        var helpLabel = ComponentFactory.CreateMutedLabel("?? Customize Flavor, Setting, and Plot to create unique worlds!");
        helpLabel.X = 2;
        helpLabel.Y = 13;

        configFrame.Add(nameLabel, _nameField, regionsLabel, _regionsField);
        configFrame.Add(seedLabel, _seedField);
        configFrame.Add(flavorLabel, _flavorField);
        configFrame.Add(descLabel, _descriptionField);
        configFrame.Add(plotLabel, _plotField);
        configFrame.Add(timeLabel, _timePeriodField);
        configFrame.Add(modelLabel, _modelRadio);
        configFrame.Add(helpLabel);

        // Progress Frame - More compact, positioned better
        var progressFrame = ComponentFactory.CreateFrame("[ Progress ]");
        progressFrame.X = 1;
        progressFrame.Y = 20;
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
            _cancellationTokenSource = new CancellationTokenSource();
            generatedWorldPath = await GenerateWorldAsync(_cancellationTokenSource.Token);
            if (generatedWorldPath != null)
            {
                Application.RequestStop();
            }
        });
        
        var cancelBtn = ComponentFactory.CreateButton("[ < ] Cancel");
        cancelBtn.X = Pos.Right(generateBtn) + 2;
        cancelBtn.Y = Pos.AnchorEnd(1);
        cancelBtn.Clicked += () => 
        {
            // Cancel ongoing generation if in progress
            if (_cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested)
            {
                _cancellationTokenSource.Cancel();
                Log("Generation cancelled by user");
                UpdateStatus("Cancelled", 0);
            }
            Application.RequestStop();
        };

        win.Add(title, configFrame, progressFrame, generateBtn, cancelBtn);
        
        Log("Ready. Select model and click Generate.");
        
        Application.Run(win);
        
        return generatedWorldPath;
    }

    /// <summary>
    /// Gets display name for model, showing cached status.
    /// </summary>
    private string GetModelDisplayName(string modelKey, string baseName)
    {
        var isCached = GGUFModelDownloader.IsModelDownloaded(modelKey);
        return isCached ? $"{baseName} ? Cached" : baseName;
    }

    private async Task<string?> GenerateWorldAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Check for cancellation at the start
            cancellationToken.ThrowIfCancellationRequested();
            
            Log("Starting generation...");
            UpdateStatus("Initializing", 0);

            var model = _modelRadio.SelectedItem switch
            {
                0 => "phi-3-mini-q4",
                1 => "tinyllama-q4",
                2 => "llama-3.2-1b-q4",
                _ => "phi-3-mini-q4"
            };

            _settings.Value.Provider = "LLamaSharp";
            _settings.Value.Model = model;
            _settings.Value.LLamaModelKey = model;

            Log($"Provider: LLamaSharp, Model: {model}");

            // Initialize LLamaSharp adapter (with caching)
            ILocalSLMAdapter slmAdapter;
            try
            {
                // Check for cancellation before loading model
                cancellationToken.ThrowIfCancellationRequested();
                
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
                    _logger?.LogInformation("?? Creating new LLamaSharp adapter for model: {Model}", model);
                    
                    var llamaAdapter = new LLamaSharpAdapter(_settings, _logger as ILogger<LLamaSharpAdapter>);
                    
                    var progress = new Progress<DownloadProgress>(p =>
                    {
                        var pct = p.PercentComplete;
                        var speedMB = p.SpeedBytesPerSecond / 1024.0 / 1024.0;
                        var downloadedMB = p.DownloadedBytes / 1024.0 / 1024.0;
                        var totalMB = p.TotalBytes / 1024.0 / 1024.0;
                        
                        UpdateStatus($"Downloading: {downloadedMB:F0}/{totalMB:F0} MB ({speedMB:F1} MB/s)", 5 + (pct * 0.4f));
                        Log($"Download progress: {pct}% - {speedMB:F1} MB/s - ETA: {p.FormattedETA}");
                    });

                    Log("Downloading/loading model (this may take a while)...");
                    _logger?.LogInformation("?? Model download/load starting...");
                    
                    await llamaAdapter.InitializeAsync(progress);
                    
                    Log("Model loaded successfully!");
                    _logger?.LogInformation("? Model initialization complete");
                    
                    // Cache the adapter
                    _cachedLlamaAdapter = llamaAdapter;
                    _cachedModelKey = model;
                    _logger?.LogInformation("?? Adapter cached for reuse (model: {Model})", model);
                    slmAdapter = llamaAdapter;
                }
            }
            catch (Exception llamaEx)
            {
                Log($"LLamaSharp initialization failed: {llamaEx.Message}");
                _logger.LogError(llamaEx, "LLamaSharp initialization failed");
                throw;
            }

            // Check for cancellation before starting generation
            cancellationToken.ThrowIfCancellationRequested();

            var imageAdapter = _services.GetRequiredService<IImageAdapter>();
            var logger = _services.GetRequiredService<ILogger<SeededWorldGenerator>>();
            var generator = new SeededWorldGenerator(slmAdapter, imageAdapter, logger);

            UpdateStatus("Generating world", 50);
            var options = new WorldGenerationOptions
            {
                Name = _nameField.Text.ToString() ?? "MyWorld",
                Seed = int.TryParse(_seedField.Text.ToString(), out var s) ? s : 12345,
                Theme = "Cyberpunk",
                Regions = int.TryParse(_regionsField.Text.ToString(), out var r) ? Math.Max(3, r) : 5,
                NpcDensity = "medium",
                Flavor = _flavorField.Text.ToString() ?? "Atmospheric and mysterious",
                Description = _descriptionField.Text.ToString() ?? "A cyberpunk world where technology and humanity collide",
                MainPlotPoint = _plotField.Text.ToString() ?? "Uncover the conspiracy behind recent disappearances",
                TimePeriod = _timePeriodField.Text.ToString() ?? "Near future (2077)",
                PowerStructure = "Corporations, hackers, and underground resistance"
            };

            Log($"Generating '{options.Name}' (seed: {options.Seed})...");
            Log($"Flavor: '{options.Flavor}'");
            Log($"Setting: '{options.Description}'");
            Log($"Plot: '{options.MainPlotPoint}'");
            
            // Check for cancellation before generation
            cancellationToken.ThrowIfCancellationRequested();
            
            var result = generator.Generate(options);

            // Basic structural validation
            UpdateStatus("Validating structure", 70);
            _validator.Validate(result);
            Log($"Generated: {result.Rooms.Count} rooms, {result.Npcs.Count} NPCs");

            // LLM-based quality validation
            UpdateStatus("Validating quality (LLM)", 75);
            Log("Running LLM-based quality checks...");
            
            // Create a validator with the SLM adapter for quality checks
            var qualityValidator = new WorldValidator(slmAdapter, _logger as ILogger<WorldValidator>);
            var qualityResult = qualityValidator.ValidateQuality(result, options.Theme);
            
            Log($"Quality Scores: Rooms={qualityResult.Metrics.RoomQualityScore}, NPCs={qualityResult.Metrics.NpcQualityScore}, " +
                $"Factions={qualityResult.Metrics.FactionQualityScore}, Overall={qualityResult.Metrics.OverallScore}/100");
            
            if (qualityResult.Warnings.Count > 0)
            {
                Log($"Quality Warnings ({qualityResult.Warnings.Count}):");
                foreach (var warning in qualityResult.Warnings)
                {
                    Log($"  ?? {warning}");
                }
            }
            
            if (!qualityResult.IsValid)
            {
                Log("? Quality validation failed!");
                foreach (var error in qualityResult.Errors)
                {
                    Log($"  ? {error}");
                }
                
                Application.MainLoop.Invoke(() =>
                {
                    var retry = MessageBox.Query("Quality Issues Detected", 
                        $"The generated world has quality issues:\n\n" +
                        $"Overall Score: {qualityResult.Metrics.OverallScore}/100\n" +
                        $"Warnings: {qualityResult.Warnings.Count}\n\n" +
                        $"Continue saving anyway?", 
                        "Save Anyway", "Cancel");
                    
                    if (retry == 1) // Cancel
                    {
                        throw new OperationCanceledException("User cancelled due to quality issues");
                    }
                });
            }
            else
            {
                Log("? Quality validation passed!");
            }

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
        catch (OperationCanceledException)
        {
            Log("Generation cancelled by user");
            UpdateStatus("Cancelled", 0);
            return null;
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
        try
        {
            // Thread-safe UI update with null checks
            if (Application.MainLoop != null && _logView != null)
            {
                Application.MainLoop.Invoke(() =>
                {
                    _logView.Text += $"{DateTime.Now:HH:mm:ss} {message}\n";
                    _logView.MoveEnd();
                });
            }
            _logger?.LogInformation(message);
        }
        catch (Exception ex)
        {
            // Gracefully handle UI update failures
            _logger?.LogWarning(ex, "Failed to update UI log: {Message}", message);
        }
    }

    private void UpdateStatus(string status, float progress)
    {
        try
        {
            // Thread-safe UI update with null checks
            if (Application.MainLoop != null && _statusLabel != null && _progressBar != null)
            {
                Application.MainLoop.Invoke(() =>
                {
                    _statusLabel.Text = status;
                    _progressBar.Fraction = progress / 100f;
                });
            }
        }
        catch (Exception ex)
        {
            // Gracefully handle UI update failures
            _logger?.LogWarning(ex, "Failed to update UI status: {Status}", status);
        }
    }
}
