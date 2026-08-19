namespace lblScan.Services;

/// <summary>
/// Central registry for all CLI arguments supported by lblScan
/// </summary>

public static class ArgumentParser
{
    public static readonly CliOption Path = new("-p", "--path", "Specify a path to a directory containing a Latex project");
    public static readonly CliOption Version = new("-v", "--version", "Display the version number of lblScan");
    public static readonly CliOption Help = new("-h", "--help", "Show help and usage information");
    public static readonly CliOption FullPath = new("-f", "--full", "Show full filepaths for graphics in the Associated File column. Omit to only display the file name");
    public static readonly CliOption Caption = new("-c", "--caption", "Include a snippet of the associated \\caption{} text. By default this is not included");
    public static readonly CliOption NoFile = new("-nf", "--no-file", "Omit the Associated File linked to the \\label.");
    public static readonly CliOption NoCache = new("", "--no-cache", "Force a complete re-scan of the project. By default a cache file is written at first execution");
    public static readonly CliOption ResetCache = new("", "--reset-cache", "Deletes the current cache and forces a re-scan as well as generating a new cache for that scan");
    public static readonly CliOption OnlyGraphics = new("-g", "--graphics", "Only display labels that have an associated graphic file attached");
    public static readonly CliOption Environment = new("-e", "--env", "Only display labels within a specific environment (figure, table, tikzpicture etc.)");
    public static readonly CliOption Interactive = new("-i", "--interactive", "Launch lblScan in interactive mode");
    public static readonly CliOption Tree = new("-t", "--tree", "Display labels in a hierarchy grouped by file");
    public static readonly CliOption TreeDepth = new("", "--tree-depth", "Limit the depth of the tree output. Only applies with --tree");
    public static readonly CliOption SortAsc = new("", "--sort-asc", "Sort labels by name in ascending order");
    public static readonly CliOption SortDesc = new("", "--sort-desc", "Sort labels by name in descending order");
    public static readonly CliOption CsvOutput = new("", "--csv", "Export the results to a CSV file in the current directory");

    public static readonly IReadOnlyList<CliOption> AllOptions = new[]
    {
        Path, Version, Help, FullPath, Caption, NoFile, NoCache, ResetCache, OnlyGraphics, Environment, Interactive, Tree, TreeDepth, SortAsc, SortDesc, CsvOutput
    };

    public static string GetTargetPath(string[] args)
    {
        string? specifiedPath = Path.GetValue(args);
        if (!string.IsNullOrEmpty(specifiedPath))
        {
            return System.IO.Path.GetFullPath(specifiedPath);
        }

        return Directory.GetCurrentDirectory();
    }

    public static bool IsVersion(string[] args) => Version.IsMatch(args);
    public static bool IsHelp(string[] args) => Help.IsMatch(args);
    public static bool IsFullPath(string[] args) => FullPath.IsMatch(args);
    public static bool IsNoCache(string[] args) => NoCache.IsMatch(args);
    public static bool IsResetCache(string[] args) => ResetCache.IsMatch(args);
    public static bool IsCaption(string[] args) => Caption.IsMatch(args);
    public static bool IsNoFile(string[] args) => NoFile.IsMatch(args);
    public static bool IsOnlyGraphics(string[] args) => OnlyGraphics.IsMatch(args);
    public static bool IsInteractive(string[] args) => Interactive.IsMatch(args);
    public static bool IsTree(string[] args) => Tree.IsMatch(args);
    public static bool IsSortAsc(string[] args) => SortAsc.IsMatch(args);
    public static bool IsSortDesc(string[] args) => SortDesc.IsMatch(args);
    public static bool IsCsvOutput(string[] args) => CsvOutput.IsMatch(args);
    public static string? GetEnvironmentFilter(string[] args) => Environment.GetValue(args);
    public static int? GetTreeDepth(string[] args)
    {
        string? value = TreeDepth.GetValue(args);
        return !string.IsNullOrEmpty(value) && int.TryParse(value, out int depth) ? depth : null;
    }

}