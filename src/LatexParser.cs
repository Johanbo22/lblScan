using System.Text.RegularExpressions;

namespace lblScan;

public class LatexParser
{
    private readonly Regex _beginRegex = new(@"\\begin{([^}]+)}", RegexOptions.Compiled);
    private readonly Regex _endRegex = new(@"\\end{([^}]+)}", RegexOptions.Compiled);
    private readonly Regex _graphicsRegex = new(@"\\includegraphics(?:\[.*?\])?{([^}]+)}", RegexOptions.Compiled);
    private readonly Regex _labelRegex = new(@"\\label{([^}]+)}", RegexOptions.Compiled);

    public List<TexItem> ParseDirectory(string rootDirectory)
    {
        var extractedData = new List<TexItem>();
        var texFiles = Directory.EnumerateFiles(rootDirectory, "*.tex", SearchOption.AllDirectories);

        foreach (var file in texFiles)
        {
            var environments = new Stack<string>();
            string currentGraphic = null;

            foreach (var line in File.ReadLines(file))
            {
                if (_beginRegex.Match(line) is { Success: true } bMatch)
                    environments.Push(bMatch.Groups[1].Value);

                if (_graphicsRegex.Match(line) is { Success: true } gMatch)
                    currentGraphic = gMatch.Groups[1].Value;

                if (_labelRegex.Match(line) is { Success: true } lMatch)
                {
                    string labelName = lMatch.Groups[1].Value;
                    string currentEnv = environments.Count > 0 ? environments.Peek() : "document";

                    extractedData.Add(new TexItem(currentEnv, labelName, currentGraphic));
                    currentGraphic = null;
                }

                if (_endRegex.Match(line) is { Success: true } eMatch && environments.Count > 0)
                {
                    environments.Pop();
                    currentGraphic = null;
                }
            }
        }
        return extractedData;
    }
}