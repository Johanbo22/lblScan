namespace lblScan.Models;

public record TexItem(string Environment, string labelName, string? GraphicPath)
{
    public bool HasGraphic => !string.IsNullOrEmpty(GraphicPath);
}
