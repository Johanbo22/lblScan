namespace lblScan.UI;

public class ConsoleRenderer
{
    public void RenderTable(List<TexItem> items, bool showFullPath)
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
        table.AddColumn(new TableColumn("[yellow]Associated File[/]"));

        foreach (var item in items)
        {
            string graphicDisplay = "[dim]-[/]";

            if (item.HasGraphic)
            {
                if (showFullPath)
                {
                    graphicDisplay = item.GraphicPath;
                }
                else
                {
                    var pathParts = item.GraphicPath!.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
                    graphicDisplay = pathParts.Length > 0 ? pathParts[^1] : item.GraphicPath;
                }
            }

            table.AddRow(item.Environment, item.labelName, graphicDisplay);
        }

        AnsiConsole.Write(table);
    }
}