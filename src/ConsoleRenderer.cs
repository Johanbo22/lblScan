using Spectre.Console;

namespace lblScan;

public class ConsoleRenderer
{
    public void RenderTable(List<TexItem> items, bool showFullPath)
    {
        var table = new Table();
        table.Border(TableBorder.Rounded);
        table.AddColumn("[cyan]Environment[/]");
        table.AddColumn("[green]Label Name[/]");
        table.AddColumn("[yellow]Associated File[/]");

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
                    var pathParts = item.GraphicPath.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
                    graphicDisplay = pathParts.Length > 0 ? pathParts[^1] : item.GraphicPath;
                }
            }

            table.AddRow(item.Environment, item.labelName, graphicDisplay);
            table.AddRow("", "", "");
        }

        AnsiConsole.Write(table);
    }
}