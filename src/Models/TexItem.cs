namespace lblScan.Models;

public record TexItem(
    string Environment,
    string LabelName,
    string FilePath,
    int LineNumber,
    string? GraphicPath,
    string? CaptionSnippet = null)
{
    public bool HasGraphic => !string.IsNullOrEmpty(GraphicPath);
    public bool HasCaption => !string.IsNullOrEmpty(CaptionSnippet);
}
