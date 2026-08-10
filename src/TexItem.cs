namespace lblScan;

public record TexItem(string Environment, string labelName, string GraphicPath)
{
    public bool HasGraphic => !string.IsNullOrEmpty(GraphicPath);
}
