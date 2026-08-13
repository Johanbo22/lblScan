namespace lblScan.UI;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Spectre.Console;
using TextCopy;
using lblScan.Models;

public class InteractiveRenderer
{
    public void RenderInteractiveGrid(List<TexItem> items, bool showFullPath, bool showCaption, bool hideFile)
    {
        if (!items.Any())
        {
            AnsiConsole.MarkupLine("[yellow]No labels found in the project.[/]");
            return;
        }

        string filter = string.Empty;
        int selectedIndex = 0;
        bool isRunning = true;

        if (!Console.IsOutputRedirected)
            AnsiConsole.Cursor.Hide();

        AnsiConsole.Live(new Text(string.Empty))
            .Overflow(VerticalOverflow.Crop)
            .Start(ctx =>
            {
                while (isRunning)
                {
                    var filteredItems = GetFilteredItems(items, filter);
                    EnsureValidSelection(ref selectedIndex, filteredItems.Count);

                    var table = BuildTable(filteredItems, selectedIndex, filter, showFullPath, showCaption, hideFile);

                    ctx.UpdateTarget(table);
                    ctx.Refresh();

                    isRunning = HandleInput(ref filter, ref selectedIndex, filteredItems);
                }
            });

        if (!Console.IsOutputRedirected)
            AnsiConsole.Cursor.Show();
    }

    private static List<TexItem> GetFilteredItems(List<TexItem> items, string filter)
    {
        if (string.IsNullOrWhiteSpace(filter))
            return items.ToList();

        return items.Where(i =>
            i.Environment.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
            i.labelName.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
            (i.HasCaption && i.CaptionSnippet!.Contains(filter, StringComparison.OrdinalIgnoreCase)) ||
            (i.HasGraphic && i.GraphicPath!.Contains(filter, StringComparison.OrdinalIgnoreCase))
        ).ToList();
    }

    private static void EnsureValidSelection(ref int selectedIndex, int itemCount)
    {
        if (selectedIndex >= itemCount)
        {
            selectedIndex = Math.Max(0, itemCount - 1);
        }
    }

    private Table BuildTable(List<TexItem> filteredItems, int selectedIndex, string filter, bool showFullPath, bool showCaption, bool hideFile)
    {
        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Grey)
            .Title($"[b]LaTeX Labels[/] | Filter: [yellow]{(string.IsNullOrEmpty(filter) ? "<type to filter>" : Markup.Escape(filter))}[/]")
            .Caption("[grey](Up/Down to navigate, Enter to copy, Backspace to delete, Esc to exit)[/]")
            .ShowRowSeparators();

        table.AddColumn(new TableColumn("[cyan]Environment[/]").Centered());
        table.AddColumn(new TableColumn("[green]Label Name[/]"));

        if (showCaption) table.AddColumn(new TableColumn("[magenta]Caption[/]"));
        if (!hideFile) table.AddColumn(new TableColumn("[yellow]Associated File[/]"));

        var (startIdx, endIdx) = GetPaginationBounds(filteredItems.Count, selectedIndex);

        if (filteredItems.Count == 0)
        {
            table.AddRow(new[] { "[dim]No matches[/]", "", "", "" }.Take(table.Columns.Count).ToArray());
            return table;
        }

        for (int i = startIdx; i < endIdx; i++)
        {
            AddRowToTable(table, filteredItems[i], i == selectedIndex, showFullPath, showCaption, hideFile);
        }

        return table;
    }

    private (int Start, int End) GetPaginationBounds(int itemCount, int selectedIndex)
    {
        int displayLimit = 10;
        try
        {
            if (Console.WindowHeight > 15)
                displayLimit = Console.WindowHeight - 10;
        }
        catch { }

        int startIdx = Math.Max(0, selectedIndex - (displayLimit / 2));
        int endIdx = Math.Min(itemCount, startIdx + displayLimit);

        if (endIdx - startIdx < displayLimit && itemCount > displayLimit)
        {
            startIdx = Math.Max(0, endIdx - displayLimit);
        }

        return (startIdx, endIdx);
    }

    private static void AddRowToTable(Table table, TexItem item, bool isSelected, bool showFullPath, bool showCaption, bool hideFile)
    {
        string style = isSelected ? "[invert]" : "";
        string endStyle = isSelected ? "[/]" : "";

        var rowData = new List<string> {
            $"{style}{(isSelected ? "► " : "")}{Markup.Escape(item.Environment)}{endStyle}",
            $"{style}{Markup.Escape(item.labelName)}{endStyle}"
        };

        if (showCaption)
        {
            rowData.Add($"{style}{FormatCaption(item)}{endStyle}");
        }

        if (!hideFile)
        {
            rowData.Add($"{style}{FormatGraphic(item, showFullPath)}{endStyle}");
        }

        table.AddRow(rowData.ToArray());
    }

    private static string FormatCaption(TexItem item)
    {
        if (!item.HasCaption) return "-";

        const int MaxLength = 40;
        string snippet = item.CaptionSnippet!.Length > MaxLength
            ? item.CaptionSnippet.Substring(0, MaxLength - 3) + "..."
            : item.CaptionSnippet;

        return Markup.Escape(snippet);
    }

    private static string FormatGraphic(TexItem item, bool showFullPath)
    {
        if (!item.HasGraphic) return "-";
        if (showFullPath) return Markup.Escape(item.GraphicPath!);

        var pathParts = item.GraphicPath!.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
        return Markup.Escape(pathParts.Length > 0 ? pathParts[^1] : item.GraphicPath!);
    }

    private bool HandleInput(ref string filter, ref int selectedIndex, List<TexItem> filteredItems)
    {
        if (!Console.KeyAvailable)
        {
            Thread.Sleep(16);
            return true;
        }

        var key = Console.ReadKey(intercept: true);

        switch (key.Key)
        {
            case ConsoleKey.Escape:
                return false;

            case ConsoleKey.UpArrow:
                selectedIndex = Math.Max(0, selectedIndex - 1);
                break;

            case ConsoleKey.DownArrow:
                selectedIndex = Math.Min(filteredItems.Count - 1, selectedIndex + 1);
                break;

            case ConsoleKey.Enter:
                if (filteredItems.Count > 0)
                {
                    var selectedItem = filteredItems[selectedIndex];
                    ClipboardService.SetText(selectedItem.labelName);
                    AnsiConsole.MarkupLine($"\n[bold green]Copied '{Markup.Escape(selectedItem.labelName)}' to clipboard![/]");
                    return false;
                }
                break;

            case ConsoleKey.Backspace:
                if (filter.Length > 0)
                    filter = filter[..^1];
                break;

            default:
                if (!char.IsControl(key.KeyChar))
                {
                    filter += key.KeyChar;
                }
                break;
        }

        return true;
    }
}