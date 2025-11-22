using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using SoloAdventureSystem.ContentGenerator.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace SoloAdventureSystem.ContentGenerator.Adapters;

/// <summary>
/// Caching wrapper for any ILocalSLMAdapter.
/// Ensures deterministic outputs by caching results keyed by (method + context + seed).
/// </summary>
public class CachedSLMAdapter : ILocalSLMAdapter
{
    private readonly ILocalSLMAdapter _inner;
    private readonly AISettings _settings;
    private readonly ILogger<CachedSLMAdapter> _logger;
    private readonly string _cacheDir;

    public CachedSLMAdapter(
        ILocalSLMAdapter inner, 
        IOptions<AISettings> settings, 
        ILogger<CachedSLMAdapter> logger)
    {
        _inner = inner;
        _settings = settings.Value;
        _logger = logger;
        _cacheDir = _settings.CacheDirectory;

        if (_settings.EnableCaching && !Directory.Exists(_cacheDir))
        {
            Directory.CreateDirectory(_cacheDir);
            _logger.LogInformation("Created cache directory: {CacheDir}", _cacheDir);
        }
    }

    public string GenerateRoomDescription(string context, int seed)
    {
        return GetOrGenerate("RoomDescription", context, seed, 
            () => _inner.GenerateRoomDescription(context, seed));
    }

    public string GenerateNpcBio(string context, int seed)
    {
        return GetOrGenerate("NpcBio", context, seed, 
            () => _inner.GenerateNpcBio(context, seed));
    }

    public string GenerateFactionFlavor(string context, int seed)
    {
        return GetOrGenerate("FactionFlavor", context, seed, 
            () => _inner.GenerateFactionFlavor(context, seed));
    }

    public List<string> GenerateLoreEntries(string context, int seed, int count)
    {
        var cacheKey = GetCacheKey("LoreEntries", $"{context}|count:{count}", seed);
        var cachePath = GetCachePath(cacheKey);

        if (_settings.EnableCaching && File.Exists(cachePath))
        {
            _logger.LogDebug("Cache hit for LoreEntries: {Context}", context);
            var json = File.ReadAllText(cachePath);
            return JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
        }

        _logger.LogDebug("Cache miss for LoreEntries: {Context}", context);
        var result = _inner.GenerateLoreEntries(context, seed, count);

        if (_settings.EnableCaching)
        {
            var json = JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(cachePath, json);
            _logger.LogDebug("Cached LoreEntries to {Path}", cachePath);
        }

        return result;
    }

    private string GetOrGenerate(string method, string context, int seed, Func<string> generator)
    {
        var cacheKey = GetCacheKey(method, context, seed);
        var cachePath = GetCachePath(cacheKey);

        if (_settings.EnableCaching && File.Exists(cachePath))
        {
            _logger.LogDebug("Cache hit for {Method}: {Context}", method, context);
            return File.ReadAllText(cachePath);
        }

        _logger.LogDebug("Cache miss for {Method}: {Context}", method, context);
        var result = generator();

        if (_settings.EnableCaching)
        {
            File.WriteAllText(cachePath, result);
            _logger.LogDebug("Cached {Method} to {Path}", method, cachePath);
        }

        return result;
    }

    private string GetCacheKey(string method, string context, int seed)
    {
        var input = $"{method}|{context}|{seed}|{_settings.Model}|{_settings.Temperature}";
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private string GetCachePath(string cacheKey)
    {
        return Path.Combine(_cacheDir, $"{cacheKey}.txt");
    }
}
