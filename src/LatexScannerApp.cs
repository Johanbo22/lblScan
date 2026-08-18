namespace lblScan;

public class LatexScannerApp
{
    private readonly LatexParser _parser;
    private readonly ConsoleRenderer _consoleRenderer;
    private readonly InteractiveRenderer _interactiveRenderer;
    private readonly Logger _logger;
    private System.Diagnostics.Stopwatch? _stopWatch;

    public LatexScannerApp()
    {
        _parser = new LatexParser();
        _consoleRenderer = new ConsoleRenderer();
        _interactiveRenderer = new InteractiveRenderer();
        _logger = new Logger();
        _stopWatch = new System.Diagnostics.Stopwatch();
    }

    public void Run(string[] args)
    {
        _logger.Initialize();
        _stopWatch?.Start();
        _logger.LogInfo("lblScan started...");
        _logger.LogDebug($"Arguments provided: {string.Join(" ", args)}");
        try
        {
            if (ArgumentParser.Help.IsMatch(args))
            {
                _logger.LogInfo("Help Argument provided, displaying all arguments and usage information");
                _consoleRenderer.RenderHelp(ArgumentParser.AllOptions);
                return;
            }

            var options = ParseOptions(args);
            string rootDir = Directory.GetCurrentDirectory();

            _logger.LogInfo($"Scanning Latex project in: {rootDir} directory");
            AnsiConsole.Write(new Rule("[bold blue]lblScan[/]").LeftJustified());
            AnsiConsole.MarkupLine($"[dim]Scanning LaTeX project in: {rootDir}[/]\n");
        
            var extractedData = ScanProject(rootDir, options);

            if (!extractedData.Any())
            {
                _logger.LogWarning("Scan resulted in 0 labels found");
                AnsiConsole.MarkupLine("[yellow]No labels found in the project.[/]");
                return;
            }

            _logger.LogInfo($"Found {extractedData.Count} label(s) in the project.");
            RenderResults(extractedData, options);
            _logger.LogInfo("Scan finished with no error(s)");
        }
        catch (Exception ex)
        {
            _logger.LogError("An error occurred during scanning", ex);
            AnsiConsole.MarkupLine($"[red]An error occurred while scanning: {Markup.Escape(ex.Message)}[/]");
        }
        finally
        {
            _stopWatch?.Stop();
            if (_stopWatch != null)
            {
                _logger.LogInfo($"Total execution time: {_stopWatch.ElapsedMilliseconds}ms");
            }
            _logger.Dispose();
        }
    }

    private ScanOptions ParseOptions(string[] args)
    {
        _logger.LogDebug("Parsing all flags available");

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
            MaxTreeDepth = ParseTreeDepth(args),
            SortAscending = ArgumentParser.IsSortAsc(args),
            SortDescending = ArgumentParser.IsSortDesc(args),
            CsvOutput = ArgumentParser.IsCsvOutput(args)
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

        _logger.LogInfo("Starting project scan...");

        AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .SpinnerStyle(Style.Parse("green"))
            .Start("Scanning Latex project...", ctx =>
            {
                _logger.LogDebug("Parsing directory with cache enabled: " + !options.NoCache);
                extractedData = _parser.ParseDirectory(rootDir, useCache: !options.NoCache);
            });

        _logger.LogDebug($"{extractedData.Count} items found before filtering");

        if (options.OnlyGraphics)
        {
            _logger.LogDebug("Removing entries with no graphics file associated");
            extractedData = extractedData.Where(item => item.HasGraphic).ToList();
            _logger.LogDebug($"After removing entries with no graphics file: {extractedData.Count} entries");
        }

        if (!string.IsNullOrEmpty(options.EnvironmentFilter))
        {
            _logger.LogDebug($"Displaying only {options.EnvironmentFilter} environment");
            extractedData = FilterByEnvironment(extractedData, options.EnvironmentFilter);
            _logger.LogDebug($"There are {extractedData.Count} entries with the {options.EnvironmentFilter} enabled");
        }

        if (options.SortAscending)
        {
            _logger.LogDebug("Sorting by labelName in ascending order");
            extractedData = extractedData.OrderBy(item => item.labelName).ToList();
        }
        else if (options.SortDescending)
        {
            _logger.LogDebug("Sorting by labelName in descending order");
            extractedData = extractedData.OrderByDescending(item => item.labelName).ToList();
        }

        _logger.LogDebug($"Final result of scan: {extractedData.Count} entries");
        return extractedData;
    }

    private List<TexItem> FilterByEnvironment(List<TexItem> items, string environment)
    {
        _logger.LogDebug($"Filtering by environment: {environment}");
        var filtered = items.Where(item => item.Environment.Equals(environment, StringComparison.OrdinalIgnoreCase)).ToList();

        if (!filtered.Any())
        {
            _logger.LogWarning($"No labels found within in the '{environment}' environment");
            AnsiConsole.MarkupLine($"\n[yellow]No labels found within the '{Markup.Escape(environment)}' environment.[/]");
            return filtered;
        }

        return filtered;
    }

    private void RenderResults(List<TexItem> items, ScanOptions options)
    {
        _logger.LogInfo("Rendering results...");
        _logger.LogDebug(
            $"Rendering Options: Interactive={options.IsInteractive}, Tree={options.ShowTree} (MaxDepth={options.MaxTreeDepth?.ToString() ?? "unlimited"}), " +
            $"FullPath={options.ShowFullPath}, Caption={options.ShowCaption}, HideFile={options.HideFile}"
            );

        if (options.IsInteractive)
        {
            _interactiveRenderer.RenderInteractiveGrid(
                items,
                options.ShowFullPath,
                options.ShowCaption,
                options.HideFile
                );
        }
        else if (options.CsvOutput)
        {
            _logger.LogDebug($"Writing contents to CSV file in: {Directory.GetCurrentDirectory()}");
            CsvExporter.ExportToCsv(items, options.ShowFullPath, options.ShowCaption, options.HideFile);
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

        _logger.LogInfo("Rendering complete");
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
    public bool SortAscending { get; init; }
    public bool SortDescending { get; init; }
    public bool CsvOutput { get; init; }
}