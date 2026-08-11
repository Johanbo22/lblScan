
bool showFullPath = args.Contains("--full") || args.Contains("-f");
bool noCache = args.Contains("--no-cache");
string rootDir = Directory.GetCurrentDirectory();

AnsiConsole.MarkupLine($"[dim]Scanning LaTeX project in: {rootDir}[/]\n");

try
{
    var parser = new LatexParser();
    var extractedData = parser.ParseDirectory(rootDir, useCache: !noCache);

    var renderer = new ConsoleRenderer();
    renderer.RenderTable(extractedData, showFullPath);
}
catch (Exception ex)
{
    AnsiConsole.MarkupLine($"[red]An error occurred while scanning: {ex.Message}[/]");
}