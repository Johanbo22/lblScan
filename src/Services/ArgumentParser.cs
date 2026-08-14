namespace lblScan.Services;

/// <summary>
/// Central registry for all CLI arguments supported by lblScan
/// </summary>

public static class ArgumentParser
{
    public static readonly CliOption Help = new("-h", "--help", "Show help and usage information");
    public static readonly CliOption FullPath = new("-f", "--full", "Show full filepaths for graphics in the Associated File column. Omit to only display the file name");
    public static readonly CliOption Caption = new("-c", "--caption", "Include a snippet of the associated \\caption{} text. By default this is not included");
    public static readonly CliOption NoFile = new("-nf", "--no-file", "Omit the Associated File linked to the \\label.");
    public static readonly CliOption NoCache = new("", "--no-cache", "Force a complete re-scan of the project. By default a cache file is written at first execution");
    public static readonly CliOption OnlyGraphics = new("-g", "--graphics", "Only display labels that have an associated graphic file attached");
    public static readonly CliOption Environment = new("-e", "--env", "Only display labels within a specific environment (figure, table, tikzpicture etc.)");
    public static readonly CliOption Interactive = new("-i", "--interactive", "Launch lblScan in interactive mode");
    public static readonly CliOption Tree = new("-t", "--tree", "Display labels in a hierarchy grouped by file");
    public static readonly CliOption TreeDepth = new("", "--tree-depth", "Limit the depth of the tree output. Only applies with --tree");

    public static readonly IReadOnlyList<CliOption> AllOptions = new[]
    {
        Help, FullPath, Caption, NoFile, NoCache, OnlyGraphics, Environment, Interactive, Tree, TreeDepth
    };
}