namespace lblScan;

public class LatexScannerApp
{
    private readonly LatexParser _parser;
    private readonly ConsoleRenderer _consoleRenderer;
    private readonly InteractiveRenderer _interactiveRenderer;

    public LatexScannerApp()
    {
        _parser = new LatexParser();
        _consoleRenderer = new ConsoleRenderer();
        _interactiveRenderer = new InteractiveRenderer();
    }

    public void Run(string[] args)
    {
        if (ArgumentParser.Help.IsMatch(args))
        {
            _consoleRenderer.RenderHelp(ArgumentParser.AllOptions);
            return;
        }

        var options = ParseOptions(args);
        string rootDir = Directory.GetCurrentDirectory();

        AnsiConsole.Write(new Rule("[bold blue]lblScan[/]").LeftJustified());
        AnsiConsole.MarkupLine($"[dim]Scanning LaTeX project in: {rootDir}[/]\n");

        try
        {
            var extractedData = ScanProject(rootDir, options);

            if (!extractedData.Any())
            {
                AnsiConsole.MarkupLine("[yellow]No labels found in the project.[/]");
                return;
            }

            RenderResults(extractedData, options);
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]An error occurred while scanning: {ex.Message}[/]");
        }
    }

    private ScanOptions ParseOptions(string[] args)
    {
        return new ScanOptions
        {
            ShowFullPath = ArgumentParser.FullPath.IsMatch(args),
            NoCache = ArgumentParser.NoCache.IsMatch(args),
            ShowCaption = ArgumentParser.Caption.IsMatch(args),
            HideFile = ArgumentParser.NoFile.IsMatch(args),
            OnlyGraphics = ArgumentParser.OnlyGraphics.IsMatch(args),
            IsInteractive = ArgumentParser.Interactive.IsMatch(args),
            ShowTree = ArgumentParser.Tree.IsMatch(args),
            EnvironmentFilter = ArgumentParser.Environment.GetValue(args),
            MaxTreeDepth = ParseTreeDepth(args)
        };
    }

    private static int? ParseTreeDepth(string[] args)
    {
        string? treeDepthValue = ArgumentParser.TreeDepth.GetValue(args);
        if (!string.IsNullOrEmpty(treeDepthValue) && int.TryParse(treeDepthValue, out int depth))
        {
            return depth;
        }
        return null;
    }

    private List<TexItem> ScanProject(string rootDir, ScanOptions options)
    {
        List<TexItem> extractedData = new();

        AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .SpinnerStyle(Style.Parse("green"))
            .Start("Scanning Latex project...", ctx =>
            {
                extractedData = _parser.ParseDirectory(rootDir, useCache: !options.NoCache);
            });

        if (options.OnlyGraphics)
        {
            extractedData = extractedData.Where(item => item.HasGraphic).ToList();
        }

        if (!string.IsNullOrEmpty(options.EnvironmentFilter))
        {
            extractedData = FilterByEnvironment(extractedData, options.EnvironmentFilter);
        }

        return extractedData;
    }

    private List<TexItem> FilterByEnvironment(List<TexItem> items, string environment)
    {
        var filtered = items.Where(item => item.Environment.Equals(environment, StringComparison.OrdinalIgnoreCase)).ToList();

        if (!filtered.Any())
        {
            AnsiConsole.MarkupLine($"\n[yellow]No labels found within the '{Markup.Escape(environment)}' environment.[/]");
        }

        return filtered;
    }

    private void RenderResults(List<TexItem> items, ScanOptions options)
    {
        if (options.IsInteractive)
        {
            _interactiveRenderer.RenderInteractiveGrid(
                items,
                options.ShowFullPath,
                options.ShowCaption,
                options.HideFile
                );
        }
        else if (options.ShowTree)
        {
            _consoleRenderer.RenderTree(
                items,
                options.ShowFullPath,
                options.ShowCaption,
                options.HideFile,
                options.MaxTreeDepth
                );
        }
        else
        {
            _consoleRenderer.RenderTable(
                items,
                options.ShowFullPath,
                options.ShowCaption,
                options.HideFile
                );
        }
    }

}

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
}