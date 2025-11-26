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
using System.IO;
using System.Collections.Generic;
using SoloAdventureSystem.ContentGenerator.Utils;

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
    private ILocalSLMAdapter? _adapter;
    private bool _isInitialized;
    private readonly SemaphoreSlim _initLock = new(1, 1);
    private readonly SemaphoreSlim _generationLock = new(1, 1);

    public WorldGenerationService(
        ILogger<WorldGenerationService> logger,
        IOptions<AISettings> settings,
        IImageAdapter imageAdapter,
        WorldValidator validator,
        WorldExporter exporter,
        ILocalSLMAdapter adapter)
    {
        _logger = logger;
        _settings = settings.Value;
        _imageAdapter = imageAdapter;
        _validator = validator;
        _exporter = exporter;
        _adapter = adapter;
    }

    public bool IsInitialized => _isInitialized;

    public async Task InitializeAsync(IProgress<DownloadProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        await _initLock.WaitAsync(cancellationToken);
        try
        {
            // Dispose existing adapter if reinitializing
            if (_isInitialized && _adapter != null)
            {
                _logger.LogInformation("Disposing existing adapter for reinitialization");
                if (_adapter is IDisposable disposableAdapter)
                {
                    disposableAdapter.Dispose();
                }
                _isInitialized = false;
            }

            _logger.LogInformation("Initializing AI adapter with model: {Model}", _settings.LLamaModelKey);

            await _adapter.InitializeAsync(progress);

            _isInitialized = true;
            _logger.LogInformation("AI adapter initialized successfully");
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

        // Prevent concurrent generations
        await _generationLock.WaitAsync(cancellationToken);
        try
        {
            progress?.Report("Starting world generation...");

            // Run generation and validation on a background thread to avoid blocking the Blazor sync context
            var result = await Task.Run(() =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                var generator = new SeededWorldGenerator(_adapter, _imageAdapter, _logger as ILogger<SeededWorldGenerator>);

                progress?.Report("Generating world structure...");
                var r = generator.Generate(options);

                cancellationToken.ThrowIfCancellationRequested();

                progress?.Report("Validating world...");
                _validator.Validate(r);

                return r;
            }, cancellationToken);

            progress?.Report("World generation complete!");

            return result;
        }
        finally
        {
            _generationLock.Release();
        }
    }

    // NOTE: Prefill/LLM helper methods were removed temporarily. Reintroduce when ready to rework the UI wizard.

    public string ExportWorld(WorldGenerationResult result, WorldGenerationOptions options, string outputPath)
    {
        _logger.LogInformation("Exporting world to: {Path}", outputPath);

        // Use a timestamp-based id for filename uniqueness instead of seed
        var id = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var tempDir = Path.Combine(Path.GetTempPath(), $"World_{options.Name}_{id}");
        if (Directory.Exists(tempDir))
            Directory.Delete(tempDir, true);
        Directory.CreateDirectory(tempDir);

        _exporter.Export(result, options, tempDir);

        var zipPath = Path.Combine(outputPath, $"World_{options.Name}_{id}.zip");
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
        if (_adapter is IDisposable disposableAdapter)
        {
            disposableAdapter.Dispose();
        }
        _initLock?.Dispose();
        _generationLock?.Dispose();
        GC.SuppressFinalize(this);
    }
}
