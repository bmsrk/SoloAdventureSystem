using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SoloAdventureSystem.ContentGenerator.Adapters;
using SoloAdventureSystem.ContentGenerator.Configuration;
using SoloAdventureSystem.ContentGenerator.EmbeddedModel;
using SoloAdventureSystem.ContentGenerator.Generation;
using SoloAdventureSystem.ContentGenerator.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SoloAdventureSystem.Web.UI.Services;

/// <summary>
/// Service for managing world generation with LLM models
/// </summary>
public class WorldGenerationService : IDisposable
{
    private readonly ILogger<WorldGenerationService> _logger;
    private readonly AISettings _settings;
    private readonly IImageAdapter _imageAdapter;
    private readonly WorldValidator _validator;
    private readonly WorldExporter _exporter;
    private LLamaSharpAdapter? _adapter;
    private bool _isInitialized;
    private readonly SemaphoreSlim _initLock = new(1, 1);

    public WorldGenerationService(
        ILogger<WorldGenerationService> logger,
        IOptions<AISettings> settings,
        IImageAdapter imageAdapter,
        WorldValidator validator,
        WorldExporter exporter)
    {
        _logger = logger;
        _settings = settings.Value;
        _imageAdapter = imageAdapter;
        _validator = validator;
        _exporter = exporter;
    }

    public bool IsInitialized => _isInitialized;

    public async Task InitializeAsync(IProgress<DownloadProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        await _initLock.WaitAsync(cancellationToken);
        try
        {
            if (_isInitialized)
            {
                _logger.LogInformation("AI adapter already initialized");
                return;
            }

            _logger.LogInformation("Initializing AI adapter with model: {Model}", _settings.LLamaModelKey);
            
            _adapter = new LLamaSharpAdapter(Options.Create(_settings), _logger as ILogger<LLamaSharpAdapter>);
            await _adapter.InitializeAsync(progress);
            
            _isInitialized = true;
            _logger.LogInformation("AI adapter initialized successfully with GPU: {UseGPU}", _settings.UseGPU);
        }
        finally
        {
            _initLock.Release();
        }
    }

    public async Task<WorldGenerationResult> GenerateWorldAsync(
        WorldGenerationOptions options,
        IProgress<string>? progress = null,
        CancellationToken cancellationToken = default)
    {
        if (!_isInitialized || _adapter == null)
        {
            throw new InvalidOperationException("Service not initialized. Call InitializeAsync first.");
        }

        progress?.Report("Starting world generation...");
        
        var generator = new SeededWorldGenerator(_adapter, _imageAdapter, _logger as ILogger<SeededWorldGenerator>);
        
        progress?.Report("Generating world structure...");
        var result = await Task.Run(() => generator.Generate(options), cancellationToken);
        
        progress?.Report("Validating world...");
        _validator.Validate(result);
        
        progress?.Report("World generation complete!");
        
        return result;
    }

    public string ExportWorld(WorldGenerationResult result, WorldGenerationOptions options, string outputPath)
    {
        _logger.LogInformation("Exporting world to: {Path}", outputPath);
        
        var tempDir = Path.Combine(Path.GetTempPath(), $"World_{options.Name}_{options.Seed}");
        if (Directory.Exists(tempDir)) 
            Directory.Delete(tempDir, true);
        Directory.CreateDirectory(tempDir);

        _exporter.Export(result, options, tempDir);

        var zipPath = Path.Combine(outputPath, $"World_{options.Name}_{options.Seed}.zip");
        var zipDir = Path.GetDirectoryName(zipPath);
        if (!string.IsNullOrEmpty(zipDir) && !Directory.Exists(zipDir))
        {
            Directory.CreateDirectory(zipDir);
        }

        _exporter.Zip(tempDir, zipPath);

        if (Directory.Exists(tempDir)) 
            Directory.Delete(tempDir, true);

        _logger.LogInformation("World exported to: {Path}", zipPath);
        return zipPath;
    }

    public void Dispose()
    {
        _adapter?.Dispose();
        _initLock?.Dispose();
        GC.SuppressFinalize(this);
    }
}
