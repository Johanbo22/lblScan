
if (ArgumentParser.Help.IsMatch(args))
{
    var helpRenderer = new ConsoleRenderer();
    helpRenderer.RenderHelp(ArgumentParser.AllOptions);
    return;
}

bool showFullPath = ArgumentParser.FullPath.IsMatch(args);
bool noCache = ArgumentParser.NoCache.IsMatch(args);
bool showCaption = ArgumentParser.Caption.IsMatch(args);
bool hideFile = ArgumentParser.NoFile.IsMatch(args);
bool onlyGraphics = ArgumentParser.OnlyGraphics.IsMatch(args);
bool isInteractive = ArgumentParser.Interactive.IsMatch(args);
bool showTree = ArgumentParser.Tree.IsMatch(args);
string? envFilter = ArgumentParser.Environment.GetValue(args);

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

    if (onlyGraphics)
    {
        extractedData = extractedData.Where(item => item.HasGraphic).ToList();
    }

    if (!string.IsNullOrEmpty(envFilter))
    {
        extractedData = extractedData.Where(item => item.Environment.Equals(envFilter, StringComparison.OrdinalIgnoreCase)).ToList();

        if (!extractedData.Any())
        {
            AnsiConsole.MarkupLine($"\n[yellow]No labels found within the '{Markup.Escape(envFilter)}' environment.[/]");
        }
    }

    if (isInteractive)
    {
        var interactiveRenderer = new InteractiveRenderer();
        interactiveRenderer.RenderInteractiveGrid(extractedData, showFullPath, showCaption, hideFile);
    }
    else if (showTree)
    {
        var renderer = new ConsoleRenderer();
        renderer.RenderTree(extractedData, showFullPath, showCaption, hideFile);
    }
    else
    {
        var renderer = new ConsoleRenderer();
        renderer.RenderTable(extractedData, showFullPath, showCaption, hideFile);  
    }

}
catch (Exception ex)
{
    AnsiConsole.MarkupLine($"[red]An error occurred while scanning: {ex.Message}[/]");
}