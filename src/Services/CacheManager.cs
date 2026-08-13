using System.Text.Json;

namespace lblScan.Services;

public class CacheManager
{
    private readonly string _cacheFilePath;
    private const string CacheVersion = "1.3";

    private class CacheContainer
    {
        public string Version { get; set; } = string.Empty;
        public Dictionary<string, FileCache> Data { get; set; } = new();
    }

    public CacheManager(string rootDirectory)
    {
        _cacheFilePath = Path.Combine(rootDirectory, ".lblscan_cache.json");
    }

    public Dictionary<string, FileCache> LoadCache()
    {
        if (!File.Exists(_cacheFilePath))
            return new Dictionary<string, FileCache>();

        try
        {
            var json = File.ReadAllText(_cacheFilePath);
            var container = JsonSerializer.Deserialize<CacheContainer>(json);

            if (container == null || container.Version != CacheVersion)
            {
                return new Dictionary<string, FileCache>();
            }

            return container.Data ?? new Dictionary<string, FileCache>();
        }
        catch
        {
            return new Dictionary<string, FileCache>();
        }
    }

    public void SaveCache(Dictionary<string, FileCache> cache)
    {
        var container = new CacheContainer
        {
            Version = CacheVersion,
            Data = cache
        };

        var options = new JsonSerializerOptions { WriteIndented = false };
        var json = JsonSerializer.Serialize(container, options);

        if (File.Exists(_cacheFilePath))
        {
            try
            {
                var currentAttributes = File.GetAttributes(_cacheFilePath);
                if (currentAttributes.HasFlag(FileAttributes.Hidden))
                {
                    File.SetAttributes(_cacheFilePath, currentAttributes & ~FileAttributes.Hidden);
                }
            }
            catch { }
        }

        File.WriteAllText(_cacheFilePath, json);

        try
        {
            var attr = File.GetAttributes(_cacheFilePath);
            if (!attr.HasFlag(FileAttributes.Hidden))
            {
                File.SetAttributes(_cacheFilePath, attr | FileAttributes.Hidden);
            }
        }
        catch { }
    }
}