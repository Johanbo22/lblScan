namespace lblScan.UI;

public class ConsoleRenderer
{
    public void RenderHelp(IEnumerable<CliOption> options)
    {
        AnsiConsole.Write(new Rule("[bold blue]lblScan[/]").LeftJustified());
        AnsiConsole.MarkupLine("A CLI tool to extract [green]\\label{}[/] tags from Latex projects.\n");
        AnsiConsole.MarkupLine("[bold]Usage:[/] lblScan [grey][[OPTIONS]][/]\n");
        AnsiConsole.MarkupLine("[bold]Options:[/]");

        var grid = new Grid();
        grid.AddColumn(new GridColumn().PadRight(2));
        grid.AddColumn(new GridColumn().PadRight(2));
        grid.AddColumn(new GridColumn());

        foreach (var opt in options)
        {
            string shortDisplay = string.IsNullOrEmpty(opt.ShortName) ? "" : $"[green]{opt.ShortName}[/]";
            string longDisplay = string.IsNullOrEmpty(opt.LongName) ? "" : $"[blue]{opt.LongName}[/]";

            if (!string.IsNullOrEmpty(shortDisplay) && !string.IsNullOrEmpty(longDisplay))
            {
                shortDisplay += ",";
            }

            grid.AddRow(shortDisplay, longDisplay, opt.Description);
        }

        AnsiConsole.Write(grid);
    }


    public void RenderTable(
        List<TexItem> items,
        bool showFullPath,
        bool showCaption = false,
        bool hideFile = false
        )
    {
        if (!items.Any())
        {
            AnsiConsole.MarkupLine("[yellow]No labels found in the project.[/]");
            return;
        }

        var table = BuildBaseTable(showCaption, hideFile);

        foreach (var item in items)
        {
            var rowData = BuildTableRow(item, showFullPath, showCaption, hideFile);
            table.AddRow(rowData.ToArray());
        }

        AnsiConsole.Write(table);
    }
    private static Table BuildBaseTable(bool showCaption, bool hideFile)
    {
        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Grey)
            .Title("[b]LaTeX Labels[/]")
            .ShowRowSeparators();

        table.AddColumn(new TableColumn("[cyan]Environment[/]").Centered());
        table.AddColumn(new TableColumn("[green]Label Name[/]"));

        if (showCaption)
        {
            table.AddColumn(new TableColumn("[magenta]Caption[/]"));
        }

        if (!hideFile)
        {
            table.AddColumn(new TableColumn("[yellow]Associated File[/]"));
        }

        return table;
    }

    private List<string> BuildTableRow(TexItem item, bool showFullPath, bool showCaption, bool hideFile)
    {
        var rowData = new List<string> { item.Environment, item.labelName };

        if (showCaption)
        {
            rowData.Add(FormatCaptionDisplay(item));
        }

        if (!hideFile)
        {
            rowData.Add(FormatGraphicDisplay(item, showFullPath));
        }

        return rowData;
    }

    private static string FormatCaptionDisplay(TexItem item)
    {
        if (!item.HasCaption)
            return "[dim]-[/]";

        int maxLength = 45;
        string snippet = item.CaptionSnippet!.Length > maxLength
            ? item.CaptionSnippet.Substring(0, maxLength - 3) + "..."
            : item.CaptionSnippet;

        return Markup.Escape(snippet);
    }

    private static string FormatGraphicDisplay(TexItem item, bool showFullPath)
    {
        if (!item.HasGraphic)
            return "[dim]-[/]";

        return showFullPath ? item.GraphicPath! : GetFileName(item.GraphicPath);
    }

    private static string GetFileName(string path)
    {
        var pathParts = path.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
        return pathParts.Length > 0 ? pathParts[^1] : path;
    }

    public void RenderTree(List<TexItem> items, bool showFullPath, bool showCaption = false, bool hideFile = false, int? maxDepth = null)
    {
        if (!items.Any())
        {
            AnsiConsole.MarkupLine("[yellow]No labels found in the project.[/]");
            return;
        }

        var root = new Tree("[b blue]LaTeX Project Labels.[/]");

        var directoryNodes = new Dictionary<string, TreeNode>();
        var groupedItems = items.GroupBy(i => i.FilePath).OrderBy(g => g.Key);

        foreach (var group in groupedItems)
        {
            AddFileToTree(root, directoryNodes, group, showFullPath, showCaption, hideFile, maxDepth);
        }

        AnsiConsole.Write(root);
    }

    private void AddFileToTree(
        Tree root,
        Dictionary<string, TreeNode> directoryNodes,
        IGrouping<string, TexItem> group,
        bool showFullPath,
        bool showCaption,
        bool hideFile,
        int? maxDepth
        )
    {
        var pathParts = group.Key.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);

        TreeNode? parentNode = null;
        string accumulatedPath = "";

        for (int i = 0; i < pathParts.Length - 1; i++)
        {
            if (ShouldSkipDueToDepth(i + 1, maxDepth))
            {
                break;
            }

            accumulatedPath = string.IsNullOrEmpty(accumulatedPath) ? pathParts[i] : accumulatedPath + "/" + pathParts[i];

            if (!directoryNodes.TryGetValue(accumulatedPath, out var dirNode))
            {
                string dirMarkup = $"[blue]{Markup.Escape(pathParts[i])}/[/]";
                dirNode = parentNode == null ? root.AddNode(dirMarkup) : parentNode.AddNode(dirMarkup);
                directoryNodes[accumulatedPath] = dirNode;
            }

            parentNode = dirNode;
        }

        if (ShouldSkipDueToDepth(pathParts.Length, maxDepth))
        {
            return;
        }

        string fileName = pathParts.Length > 0 ? pathParts[^1] : group.Key;
        string fileMarkup = $"[yellow]{Markup.Escape(fileName)}[/]";
        var fileNode = parentNode == null ? root.AddNode(fileMarkup) : parentNode.AddNode(fileMarkup);

        foreach (var item in group)
        {
            AddLabelToFileNode(fileNode, item, showCaption, hideFile, showFullPath);
        }
    }

    private static bool ShouldSkipDueToDepth(int currentDepth, int? maxDepth)
    {
        return maxDepth.HasValue && currentDepth >= maxDepth.Value;
    }

    private static void AddLabelToFileNode(TreeNode fileNode, TexItem item, bool showCaption, bool hideFile, bool showFullPath)
    {
        string nodeText = $"[green]{Markup.Escape(item.labelName)}[/] [dim] ({Markup.Escape(item.Environment)}, line {item.LineNumber})[/]";

        List<string> details = BuildLabelDetails(item, showCaption, hideFile, showFullPath);

        if (details.Any())
        {
            var labelNode = fileNode.AddNode(nodeText);
            foreach (var detail in details)
            {
                labelNode.AddNode(detail);
            }
        }
        else
        {
            fileNode.AddNode(nodeText);
        }
    }

    private static List<string> BuildLabelDetails(TexItem item, bool showCaption, bool hideFile, bool showFullpath)
    {
        var details = new List<string>();

        if (showCaption && item.HasCaption)
        {
            int maxLength = 45;
            string snippet = item.CaptionSnippet!.Length > maxLength
                ? item.CaptionSnippet.Substring(0, maxLength - 3) + "..."
                : item.CaptionSnippet;
            details.Add($"[magenta]Caption:[/] {Markup.Escape(snippet)}");
        }

        if (!hideFile && item.HasGraphic)
        {
            string graphicDisplay = FormatGraphicPath(item.GraphicPath!, showFullpath);
            details.Add($"[yellow]Graphic:[/] {Markup.Escape(graphicDisplay)}");
        }

        return details;
    }

    private static string FormatGraphicPath(string graphicPath, bool showFullPath)
    {
        if (showFullPath)
            return graphicPath;

        var parts = graphicPath.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
        return parts.Length > 0 ? parts[^1] : graphicPath;
    }
}