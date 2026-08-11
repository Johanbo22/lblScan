using System.Text.Json;

namespace lblScan.Services;

public class CacheManager
{
    private readonly string _cacheFilePath;

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
            return JsonSerializer.Deserialize<Dictionary<string, FileCache>>(json) ?? new();
        }
        catch
        {
            return new Dictionary<string, FileCache>();
        }
    }

    public void SaveCache(Dictionary<string, FileCache> cache)
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        var json = JsonSerializer.Serialize(cache, options);

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