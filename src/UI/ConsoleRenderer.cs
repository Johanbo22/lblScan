namespace lblScan.UI;

public class ConsoleRenderer
{
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
}