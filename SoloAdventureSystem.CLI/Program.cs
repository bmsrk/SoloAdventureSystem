using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SoloAdventureSystem.ContentGenerator.Generation;
using SoloAdventureSystem.ContentGenerator.Models;
using SoloAdventureSystem.ContentGenerator.Adapters;
using SoloAdventureSystem.ContentGenerator.Configuration;
using SoloAdventureSystem.CLI.Logging;
using SoloAdventureSystem.ContentGenerator.EmbeddedModel;
using Terminal.Gui;

// Minimal Terminal.Gui host replacing Spectre.Console for interactive full-screen UI.
// Provides preset selection, live log panel, and background generation.

var services = new ServiceCollection();

// Configure logging - we'll add a GUI logger provider later once TextView is created
services.AddLogging(config => config.SetMinimumLevel(LogLevel.Debug));

// Configure AI settings for real LLM usage - adjust as needed or load from environment/appsettings
var aiSettings = new AISettings
{
    Provider = "LLamaSharp",
    LLamaModelKey = "tinyllama-q4",
    Model = "tinyllama-q4",
    ContextSize = 2048,
    UseGPU = false,
    MaxInferenceThreads = 1
};
services.AddSingleton(Options.Create(aiSettings));

var provider = services.BuildServiceProvider();
var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
var logger = loggerFactory.CreateLogger("CLI");

// Presets
var presets = new Dictionary<string, WorldGenerationOptions>
{
    ["Neon Nights"] = new WorldGenerationOptions { Name = "Neon Nights", Theme = "Cyberpunk", Flavor = "Dark and gritty with neon highlights", Description = "A sprawling megacity where corporations own everything", MainPlotPoint = "Expose the corporation covering up illegal AI experiments", TimePeriod = "2084", PowerStructure = "Megacorporations, hackers, and street gangs", Regions = 6 },
    ["Last Light"] = new WorldGenerationOptions { Name = "Last Light", Theme = "Post-Apocalyptic", Flavor = "Melancholic but hopeful", Description = "Scattered communities rebuilding after the collapse", MainPlotPoint = "Unite the settlements against the warlord threat", TimePeriod = "47 years after the Fall", PowerStructure = "Tribal councils and roaming warlords", Regions = 6 },
    ["Station Omega"] = new WorldGenerationOptions { Name = "Station Omega", Theme = "Space Station", Flavor = "Claustrophobic paranoia", Description = "Isolated space station far from Earth", MainPlotPoint = "Find the saboteur before life support fails", TimePeriod = "2156", PowerStructure = "Station administration and rival departments", Regions = 5 },
    ["The Construct"] = new WorldGenerationOptions { Name = "The Construct", Theme = "Virtual", Flavor = "Surreal and disorienting", Description = "Digital prison where minds are trapped", MainPlotPoint = "Discover the glitch that reveals the truth", TimePeriod = "Timeless virtual space", PowerStructure = "AI wardens and rebel hackers", Regions = 8 },
    ["Chrome City 1985"] = new WorldGenerationOptions { Name = "Chrome City 1985", Theme = "Retro-Future", Flavor = "Neon-soaked noir mystery", Description = "Alternative 1980s where AI emerged early", MainPlotPoint = "Solve the murder of a famous scientist", TimePeriod = "Alternative timeline 1985", PowerStructure = "Tech corporations and detective agencies", Regions = 5 }
};

// If headless requested, run a single generation and exit
if (args is not null && args.Length > 0 && Array.Exists(args, a => a == "--headless"))
{
    Console.WriteLine("Headless generation mode starting...");
    var preset = presets["Neon Nights"];
    var options = new WorldGenerationOptions
    {
        Name = preset.Name,
        Regions = preset.Regions,
        Theme = preset.Theme,
        Flavor = preset.Flavor,
        Description = preset.Description,
        MainPlotPoint = preset.MainPlotPoint,
        TimePeriod = preset.TimePeriod,
        PowerStructure = preset.PowerStructure
    };

    try
    {
        // Initialize model/downloader and adapter
        var sfLogger = loggerFactory.CreateLogger("SLMAdapterFactory");
        sfLogger.LogInformation("Ensuring model available: {ModelKey}", aiSettings.LLamaModelKey);
        var downloader = new SoloAdventureSystem.ContentGenerator.EmbeddedModel.GGUFModelDownloader(loggerFactory.CreateLogger<SoloAdventureSystem.ContentGenerator.EmbeddedModel.GGUFModelDownloader>());
        var progress = new Progress<DownloadProgress>(p => Console.WriteLine($"Download: {p.PercentComplete}% {p.DownloadedMB:F1}/{p.TotalMB:F1} MB"));
        var modelPathTask = downloader.EnsureModelAvailableAsync(aiSettings.LLamaModelKey, progress);
        modelPathTask.Wait();
        var modelPath = modelPathTask.Result;
        if (!string.IsNullOrWhiteSpace(modelPath))
        {
            sfLogger.LogInformation("Model ready at {Path}", modelPath);
            aiSettings.LLamaModelKey = modelPath;
        }

        var headlessSlmAdapter = SLMAdapterFactory.Create(provider);
        sfLogger.LogInformation("SLM adapter created (headless)");

        var imageAdapter = new SimpleImageAdapter();
        var headlessWorldGen = new WorldGenerator(
            new FactionGenerator(headlessSlmAdapter, loggerFactory.CreateLogger<FactionGenerator>()),
            new RoomGenerator(headlessSlmAdapter, imageAdapter, loggerFactory.CreateLogger<RoomGenerator>()),
            new RoomConnector(loggerFactory.CreateLogger<RoomConnector>()),
            new NpcGenerator(headlessSlmAdapter, loggerFactory.CreateLogger<NpcGenerator>()),
            loggerFactory.CreateLogger<WorldGenerator>());

        Console.WriteLine("Starting generation...");
        var result = headlessWorldGen.Generate(options);
        var outDir = Path.Combine("content", "worlds");
        Directory.CreateDirectory(outDir);
        var outPath = Path.Combine(outDir, $"{options.Name}_{DateTime.UtcNow:yyyyMMddHHmmss}.json");
        File.WriteAllText(outPath, JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true }));
        Console.WriteLine($"Generation completed. Saved: {outPath}");
        return;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Headless generation failed: {ex}");
        return;
    }
}

// Terminal.Gui setup
Application.Init();
var top = Application.Top;
var win = new Window("SoloAdventureSystem - World Generator") { X = 0, Y = 1, Width = Dim.Fill(), Height = Dim.Fill() };

// Improve appearance: use built-in color schemes, tighter layout and a status bar for hints
var leftWidth = 36;

// Left: presets and controls
var left = new FrameView("Presets") { X = 0, Y = 0, Width = leftWidth, Height = Dim.Fill() };
left.ColorScheme = Colors.Dialog; // easier on the eye
var presetNames = new List<string>(presets.Keys);
var listView = new ListView(presetNames) { X = 1, Y = 1, Width = Dim.Fill() - 2, Height = 7 };
left.Add(listView);

var nameLabel = new Label("Name:") { X = 1, Y = Pos.Bottom(listView) + 1 };
var nameField = new TextField("") { X = 1, Y = Pos.Bottom(nameLabel) + 1, Width = Dim.Fill() - 2 };
left.Add(nameLabel, nameField);

var regionsLabel = new Label("Regions:") { X = 1, Y = Pos.Bottom(nameField) + 1 };
var regionsField = new TextField("3") { X = 1, Y = Pos.Bottom(regionsLabel) + 1, Width = 6 };
left.Add(regionsLabel, regionsField);

var startButton = new Button("Start") { X = 2, Y = Pos.Bottom(regionsField) + 2 };
var exitButton = new Button("Exit") { X = Pos.Right(startButton) + 4, Y = Pos.Top(startButton) };
startButton.ColorScheme = Colors.Dialog;
exitButton.ColorScheme = Colors.Dialog;
left.Add(startButton, exitButton);

// Backend indicator label to show CPU/GPU status
var backendLabel = new Label("Backend: Unknown") { X = 1, Y = Pos.Bottom(startButton) + 2 };
left.Add(backendLabel);

// Status label to show generation state and allow clear visual cue when finished
var statusLabel = new Label("Status: Ready") { X = 1, Y = Pos.Bottom(backendLabel) + 1 };
left.Add(statusLabel);

win.Add(left);

// Right: log view
var right = new FrameView("Logs") { X = Pos.Right(left), Y = 0, Width = Dim.Fill(), Height = Dim.Fill() };
right.ColorScheme = Colors.Base;
var logView = new TextView() { ReadOnly = true, X = 0, Y = 0, Width = Dim.Fill(), Height = Dim.Fill() };
logView.WordWrap = true; // wrap long log lines for readability
right.Add(logView);
win.Add(right);

top.Add(win);

// Status bar with help and quick quit
var status = new StatusBar(new StatusItem[] {
    new StatusItem(Key.F1, "~F1~ Help", () => MessageBox.Query("Help",
        "Select a preset on the left or edit fields, then press Start to generate a world.\nLogs appear on the right. Press Exit or ^Q to quit.", "OK")),
    new StatusItem(Key.CtrlMask | Key.Q, "~^Q~ Quit", () => Application.RequestStop())
});
Application.Top.Add(status);

// Auto-detect GPU availability in background and update AI settings + UI
async Task DetectAndSetGpuAsync()
{
    try
    {
        Application.MainLoop.Invoke(() => backendLabel.Text = "Backend: Detecting GPU...");

        var hasGpu = await Task.Run(() =>
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "nvidia-smi",
                    Arguments = "-L",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var p = Process.Start(psi);
                if (p == null) return false;
                var output = p.StandardOutput.ReadToEnd();
                var err = p.StandardError.ReadToEnd();
                p.WaitForExit(2000);
                return p.ExitCode == 0 && !string.IsNullOrWhiteSpace(output);
            }
            catch
            {
                return false;
            }
        }).ConfigureAwait(false);

        // Update the shared aiSettings instance (Options wrapper references same object)
        aiSettings.UseGPU = hasGpu;

        Application.MainLoop.Invoke(() =>
        {
            backendLabel.Text = hasGpu ? "Backend: GPU detected (will use GPU)" : "Backend: No GPU detected (CPU)";
            AppendLog(hasGpu ? "GPU detected - will attempt to use GPU for model initialization." : "No GPU detected - using CPU backend.");
        });
    }
    catch (Exception ex)
    {
        Application.MainLoop.Invoke(() => backendLabel.Text = "Backend: Detection failed");
        AppendLog($"GPU detection error: {ex.Message}");
    }
}

// Start detection but don't block UI
_ = DetectAndSetGpuAsync();

// Create a GUI logger provider that writes into logView
using var guiProvider = new GuiLoggerProvider(logView);
loggerFactory.AddProvider(guiProvider);

// Also keep the in-memory provider for compatibility with existing code that used it
var inMemProvider = new InMemoryLoggerProvider();
loggerFactory.AddProvider(inMemProvider);

// Helper to append to log safely
void AppendLog(string message)
{
    Application.MainLoop.Invoke(() =>
    {
        // Keep logs compact and readable
        logView.Text += DateTime.Now.ToString("HH:mm:ss") + "  " + message + "\n";
        // Scroll to end
        logView.MoveEnd();
    });
}

// Helper to flash the status label for a short duration (visual cue)
void FlashStatusLabel()
{
    Application.MainLoop.Invoke(() =>
    {
        // Use Dialog colors for a temporary highlight
        statusLabel.ColorScheme = Colors.Dialog;
        // After 1.5s revert to base color
        Application.MainLoop.AddTimeout(TimeSpan.FromSeconds(1.5), _ =>
        {
            statusLabel.ColorScheme = Colors.Base;
            return false; // do not repeat
        });
    });
}

// Populate fields when preset changes
listView.SelectedItem = 0;
void ApplySelectedPreset()
{
    var sel = listView.SelectedItem;
    if (sel >= 0 && sel < presetNames.Count)
    {
        var key = presetNames[sel];
        var options = presets[key];
        nameField.Text = options.Name ?? string.Empty;
        regionsField.Text = options.Regions.ToString();
    }
}
ApplySelectedPreset();
listView.SelectedItemChanged += args => ApplySelectedPreset();

// Reuse existing initialization logic (wrapped into async helpers)
ILocalSLMAdapter slmAdapter = null!;

async Task<bool> TryInitializeAdapterAsync(ILogger log)
{
    try
    {
        Application.MainLoop.Invoke(() => backendLabel.Text = "Backend: Initializing...");

        var progress = new Progress<DownloadProgress>(p =>
        {
            AppendLog($"Download: {p.PercentComplete}% {p.DownloadedMB:F1}/{p.TotalMB:F1} MB - {p.SpeedMBPerSecond / 1024.0 / 1024.0:F2} MB/s - ETA {p.FormattedETA}");
        });

        var settings = provider.GetRequiredService<IOptions<AISettings>>().Value;
        var sfLogger = loggerFactory.CreateLogger("SLMAdapterFactory");

        sfLogger.LogInformation("Ensuring model available: {ModelKey}", settings.LLamaModelKey);
        var downloader = new SoloAdventureSystem.ContentGenerator.EmbeddedModel.GGUFModelDownloader(loggerFactory.CreateLogger<SoloAdventureSystem.ContentGenerator.EmbeddedModel.GGUFModelDownloader>());
        var modelPath = await downloader.EnsureModelAvailableAsync(settings.LLamaModelKey, progress).ConfigureAwait(false);
        if (!string.IsNullOrWhiteSpace(modelPath))
        {
            sfLogger.LogInformation("Model ready at {Path}", modelPath);
            settings.LLamaModelKey = modelPath;
        }

        slmAdapter = SLMAdapterFactory.Create(provider);

        // Update backend indicator based on AI settings (UseGPU flag)
        var mode = settings.UseGPU ? "GPU" : "CPU";
        Application.MainLoop.Invoke(() => backendLabel.Text = $"Backend: {mode}");
        sfLogger.LogInformation("Using backend: {Mode}", mode);

        log.LogInformation("SLM adapter initialized using provider {Provider}", settings.Provider);
        return true;
    }
    catch (Exception ex)
    {
        log.LogError(ex, "Failed to initialize SLM adapter: {Message}", ex.Message);
        Application.MainLoop.Invoke(() => backendLabel.Text = "Backend: Init failed");
        return false;
    }
}

// Create world generator lazily after adapter init
WorldGenerator worldGen = null!;

startButton.Clicked += async () =>
{
    startButton.Enabled = false;
    exitButton.Enabled = false;
    listView.Enabled = false;
    nameField.ReadOnly = true;
    regionsField.ReadOnly = true;

    statusLabel.Text = "Status: Generating...";
    AppendLog("Generation started");

    var selectedIdx = listView.SelectedItem;
    var preset = presets[presetNames[selectedIdx]];
    var options = new WorldGenerationOptions
    {
        Name = nameField.Text.ToString(),
        Regions = int.TryParse(regionsField.Text.ToString(), out var r) ? r : preset.Regions,
        Theme = preset.Theme,
        Flavor = preset.Flavor,
        Description = preset.Description,
        MainPlotPoint = preset.MainPlotPoint,
        TimePeriod = preset.TimePeriod,
        PowerStructure = preset.PowerStructure
    };

    AppendLog($"Starting generation: {options.Name} (Regions: {options.Regions})");

    // Run init + generation off UI thread
    await Task.Run(async () =>
    {
        var log = loggerFactory.CreateLogger("CLI.Background");

        // Initialize adapter
        var inited = await TryInitializeAdapterAsync(log);
        if (!inited)
        {
            AppendLog("Initialization failed. See logs.");
            Application.MainLoop.Invoke(() =>
            {
                startButton.Enabled = true;
                exitButton.Enabled = true;
                listView.Enabled = true;
                nameField.ReadOnly = false;
                regionsField.ReadOnly = false;
                statusLabel.Text = "Status: Initialization failed";
            });
            return;
        }

        // Build worldGen with adapters
        var imageAdapter = new SimpleImageAdapter();
        worldGen = new WorldGenerator(
            new FactionGenerator(slmAdapter, provider.GetService<ILogger<FactionGenerator>>() ),
            new RoomGenerator(slmAdapter, imageAdapter, provider.GetService<ILogger<RoomGenerator>>() ),
            new RoomConnector(provider.GetService<ILogger<RoomConnector>>() ),
            new NpcGenerator(slmAdapter, provider.GetService<ILogger<NpcGenerator>>() ),
            provider.GetService<ILogger<WorldGenerator>>() );

        try
        {
            AppendLog("Generating world...");
            var result = worldGen.Generate(options);
            var outDir = Path.Combine("content", "worlds");
            Directory.CreateDirectory(outDir);
            var outPath = Path.Combine(outDir, $"{options.Name}_{DateTime.UtcNow:yyyyMMddHHmmss}.json");
            File.WriteAllText(outPath, JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true }));

            AppendLog($"Generation completed. Saved: {outPath}");

            // Update UI to show completed status and re-enable controls
            Application.MainLoop.Invoke(() =>
            {
                statusLabel.Text = $"Status: Completed at {DateTime.Now:HH:mm:ss} — Ready";
                // briefly flash the label for a visual cue
                FlashStatusLabel();

                startButton.Enabled = true;
                exitButton.Enabled = true;
                listView.Enabled = true;
                nameField.ReadOnly = false;
                regionsField.ReadOnly = false;
            });
        }
        catch (Exception ex)
        {
            AppendLog($"Generation failed: {ex.Message}");
            loggerFactory.CreateLogger("CLI").LogError(ex, "Generation failed");
            Application.MainLoop.Invoke(() =>
            {
                statusLabel.Text = "Status: Generation failed - see logs";
                startButton.Enabled = true;
                exitButton.Enabled = true;
                listView.Enabled = true;
                nameField.ReadOnly = false;
                regionsField.ReadOnly = false;
            });
        }
        finally
        {
            // nothing else; UI re-enabled above after success/fail
        }

    }).ConfigureAwait(false);
};

exitButton.Clicked += () =>
{
    Application.RequestStop();
};

// Run the GUI
Application.Run();
Application.Shutdown();

// GuiLoggerProvider implementation
public class GuiLoggerProvider : ILoggerProvider
{
    private readonly TextView _textView;
    public GuiLoggerProvider(TextView textView) => _textView = textView ?? throw new ArgumentNullException(nameof(textView));
    public ILogger CreateLogger(string categoryName) => new GuiLogger(_textView, categoryName);
    public void Dispose() { }

    private class GuiLogger : ILogger
    {
        private readonly TextView _textView;
        private readonly string _category;
        public GuiLogger(TextView textView, string category) { _textView = textView; _category = category; }
        public IDisposable BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => true;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            try
            {
                var msg = formatter(state, exception);
                var line = $"[{DateTime.Now:HH:mm:ss}] {logLevel} {_category}: {msg}";
                Application.MainLoop.Invoke(() =>
                {
                    _textView.Text += line + "\n";
                    _textView.MoveEnd();
                });
            }
            catch { }
        }
    }
}
