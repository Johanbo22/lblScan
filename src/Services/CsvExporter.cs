namespace lblScan.Services;

public static class CsvExporter
{
    private const string CsvFileName = "lblscan_output.csv";

    public static void ExportToCsv(List<TexItem> items, bool showFullPath, bool showCaption, bool hideFile)
    {
        var csvPath = Path.Combine(Directory.GetCurrentDirectory(), CsvFileName);

        var headers = BuildHeaders(showCaption, hideFile);
        var rows = items.Select(item => BuildRow(item, showFullPath, showCaption, hideFile));

        var lines = new List<string> { headers };
        lines.AddRange(rows);

        File.WriteAllLines(csvPath, lines);
        AnsiConsole.MarkupLine($"[green]CSV File written to:[/] {csvPath}");
    }

    private static string BuildHeaders(bool showCaption, bool hideFile)
    {
        var columns = new List<string> { "Environment", "Label Name" };

        if (showCaption)
        {
            columns.Add("Caption");
        }

        if (!hideFile)
        {
            columns.Add("Associated File");
        }

        return string.Join(",", columns);
    }

    private static string BuildRow(TexItem item, bool showFullPath, bool showCaption, bool hideFile)
    {
        var fields = new List<string>
        {
            EscapeCsvField(item.Environment),
            EscapeCsvField(item.LabelName)
        };

        if (showCaption)
        {
            fields.Add(item.HasCaption ? EscapeCsvField(item.CaptionSnippet) : "");
        }

        if (!hideFile)
        {
            fields.Add(item.HasGraphic ? EscapeCsvField(FormatHelper.FormatGraphicPath(item.GraphicPath!, showFullPath)) : "");
        }


        return string.Join(",", fields);
    }

    private static string EscapeCsvField(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        if (value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r'))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }

        return value;
    }
}