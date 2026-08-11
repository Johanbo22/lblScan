namespace lblScan;

/// <summary>
/// Represents the cached data for a single latex file
/// </summary>
public record FileCache(DateTime LastWriteTimeUtc, List<TexItem> Items);