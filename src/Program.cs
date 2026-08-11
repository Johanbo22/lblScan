
bool showFullPath = args.Contains("--full") || args.Contains("-f");
bool noCache = args.Contains("--no-cache");
bool showCaption = args.Contains("--caption") || args.Contains("-c");
string rootDir = Directory.GetCurrentDirectory();

AnsiConsole.Write(new Rule("[bold blue]lblScan[/]").LeftJustified());
AnsiConsole.MarkupLine($"[dim]Scanning LaTeX project in: {rootDir}[/]\n");

try
{
    List<TexItem> extractedData = new();
    var parser = new LatexParser();

    AnsiConsole.Status()
        .Spinner(Spinner.Known.Dots)
        .SpinnerStyle(Style.Parse("green"))
        .Start("Scanning Latex project...", ctx =>
        {
            extractedData = parser.ParseDirectory(rootDir, useCache: !noCache);
        });

    var renderer = new ConsoleRenderer();
    renderer.RenderTable(extractedData, showFullPath, showCaption);
}
catch (Exception ex)
{
    AnsiConsole.MarkupLine($"[red]An error occurred while scanning: {ex.Message}[/]");
}