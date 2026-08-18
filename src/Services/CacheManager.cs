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
            using var fileStream = new FileStream(_cacheFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var reader = new StreamReader(fileStream);
            var json = reader.ReadToEnd();
            var container = JsonSerializer.Deserialize<CacheContainer>(json);

            if (container == null || container.Version != CacheVersion)
            {
                return new Dictionary<string, FileCache>();
            }

            return container.Data ?? new Dictionary<string, FileCache>();
        }
        catch (UnauthorizedAccessException)
        {
            return new Dictionary<string, FileCache>();
        }
        catch (IOException)
        {
            return new Dictionary<string, FileCache>();
        }
        catch (JsonException)
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

        var tempFilePath = _cacheFilePath + ".tmp" + Guid.NewGuid().ToString("N");

        try
        {
            using (var fileStream = new FileStream(tempFilePath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                using var writer = new StreamWriter(fileStream);
                writer.Write(json);
            }

            SetSecureFilePermissions(tempFilePath);

            if (File.Exists(_cacheFilePath))
            {
                File.Replace(tempFilePath, _cacheFilePath, null, true);
                return;
            }

            File.Move(tempFilePath, _cacheFilePath);
        }
        catch
        {
            if (File.Exists(tempFilePath))
            {
                try { File.Delete(tempFilePath); } catch { }
            }
            throw;
        }
    }

    private void SetSecureFilePermissions(string filePath)
    {
        try
        {
            File.SetAttributes(filePath, FileAttributes.Hidden);
        }
        catch { }
    }
}