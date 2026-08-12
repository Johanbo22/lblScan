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

        foreach (var item in items)
        {
            var rowData = new List<string> { item.Environment, item.labelName };

            if (showCaption)
            {
                string captionDisplay = "[dim]-[/]";

                if (item.HasCaption)
                {
                    int maxLength = 45;
                    string snippet = item.CaptionSnippet!.Length > maxLength
                        ? item.CaptionSnippet.Substring(0, maxLength - 3) + "..."
                        : item.CaptionSnippet;

                    captionDisplay = Markup.Escape(snippet);
                }

                rowData.Add(captionDisplay);
            }

            if (!hideFile)
            {
                string graphicDisplay = "[dim]-[/]";

                if (item.HasGraphic)
                {
                    if (showFullPath)
                    {
                        graphicDisplay = item.GraphicPath!;
                    }
                    else
                    {
                        var pathParts = item.GraphicPath!.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
                        graphicDisplay = pathParts.Length > 0 ? pathParts[^1] : item.GraphicPath;
                    }
                }

                rowData.Add(graphicDisplay);
            }
            

            table.AddRow(rowData.ToArray());
        }

        AnsiConsole.Write(table);
    }

    public void RenderTree(List<TexItem> items, bool showFullPath, bool showCaption = false, bool hideFile = false)
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
            var pathParts = group.Key.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);

            TreeNode? parentNode = null;
            string accumulatedPath = "";

            for (int i = 0; i < pathParts.Length - 1; i++)
            {
                accumulatedPath = string.IsNullOrEmpty(accumulatedPath) ? pathParts[i] : accumulatedPath + "/" + pathParts[i];

                if (!directoryNodes.TryGetValue(accumulatedPath, out var dirNode))
                {
                    string dirMarkup = $"[blue]{Markup.Escape(pathParts[i])}/[/]";
                    dirNode = parentNode == null ? root.AddNode(dirMarkup) : parentNode.AddNode(dirMarkup);

                    directoryNodes[accumulatedPath] = dirNode;
                }

                parentNode = dirNode;
            }

            string fileName = pathParts.Length > 0 ? pathParts[^1] : group.Key;
            string fileMarkup = $"[yellow]{Markup.Escape(fileName)}[/]";
            var fileNode = parentNode == null ? root.AddNode(fileMarkup) : parentNode.AddNode(fileMarkup);

            foreach (var item in group)
            {
                string nodeText = $"[green]{Markup.Escape(item.labelName)}[/] [dim]({Markup.Escape(item.Environment)}, line {item.LineNumber})[/]";

                List<string> details = new();
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
                    string graphicDisplay = item.GraphicPath!;
                    if (!showFullPath)
                    {
                        var parts = item.GraphicPath!.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
                        graphicDisplay = parts.Length > 0 ? parts[^1] : item.GraphicPath!;
                    }
                    details.Add($"[yellow]Graphic:[/] {Markup.Escape(graphicDisplay)}");
                }

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
        }

        AnsiConsole.Write(root);
    }
}