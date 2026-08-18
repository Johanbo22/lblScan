namespace lblScan.Models;

public record ScanOptions
{
    public bool ShowFullPath { get; init; }
    public bool NoCache { get; init; }
    public bool ShowCaption { get; init; }
    public bool HideFile { get; init; }
    public bool OnlyGraphics { get; init; }
    public bool IsInteractive { get; init; }
    public bool ShowTree { get; init; }
    public string? EnvironmentFilter { get; init; }
    public int? MaxTreeDepth { get; init; }
    public bool SortAscending { get; init; }
    public bool SortDescending { get; init; }
    public bool CsvOutput { get; init; }
}