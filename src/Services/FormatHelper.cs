namespace lblScan.Services;

public static class FormatHelper
{
    public static string GetFileName(string? path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return string.Empty;
        }

        var parts = path.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
        return parts.Length > 0 ? parts[^1] : path;
    }

    public static string FormatGraphicPath(string graphicPath, bool showFullPath)
    {
        if (showFullPath)
        {
            return graphicPath;
        }

        return GetFileName(graphicPath);
    }

    public static string Truncate(string? text, int maxLength)
    {
        if (string.IsNullOrEmpty(text))
        {
            return string.Empty;
        }

        if (text.Length <= maxLength)
        {
            return text;
        }

        return text[..(maxLength - 3)] + "...";
    }
}