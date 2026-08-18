using System.Text;
using System.Text.RegularExpressions;

namespace lblScan.Services;

public class LatexParser
{
    private readonly Regex _lexerRegex = new(
        @"\\(?:(?<begin>begin)\s*\{(?<env>[^}]+)\}|(?<end>end)\s*\{(?<env_end>[^}]+)\}|(?<label>label)\s*\{(?<lab>[^}]+)\}|(?<inc>includegraphics)\s*(?:\[[^\]]*\])?\s*\{(?<path>[^}]+)\}|(?<caption>caption)\s*(?:\[[^\]]*\])?\s*(?=\{(?<cap>(?>[^{}\\]+|\\.|\{(?<DEPTH>)|\}(?<-DEPTH>))*)(?(DEPTH)(?!))\}))",
        RegexOptions.Compiled | RegexOptions.Singleline);

    public List<TexItem> ParseDirectory(string rootDirectory, bool useCache = true)
    {
        var cacheManager = new CacheManager(rootDirectory);
        var cache = useCache ? cacheManager.LoadCache() : new Dictionary<string, FileCache>();
        var updatedCache = new Dictionary<string, FileCache>();

        var texFiles = Directory.EnumerateFiles(rootDirectory, "*.tex", SearchOption.AllDirectories)
            .Where(file => IsWithinRootDirectory(file, rootDirectory));

        int cacheHits = 0;
        int parsedFiles = 0;
        var extractedData = new List<TexItem>();

        foreach (var file in texFiles)
        {
            var lastModified = File.GetLastWriteTimeUtc(file);
            string relativePath = Path.GetRelativePath(rootDirectory, file);

            if (useCache && cache.TryGetValue(relativePath, out var cachedData) && cachedData.LastWriteTimeUtc == lastModified)
            {
                extractedData.AddRange(cachedData.Items);
                updatedCache[relativePath] = cachedData;
                cacheHits++;
            }
            else
            {
                var items = ParseFile(file, relativePath);
                extractedData.AddRange(items);
                updatedCache[relativePath] = new FileCache(lastModified, items);
                parsedFiles++;
            }
        }

        if (useCache)
        {
            cacheManager.SaveCache(updatedCache);
            ReportCacheUsage(cacheHits, parsedFiles);
        }

        return extractedData;
    }

    private static void ReportCacheUsage(int cacheHits, int parsedFiles)
    {
        if (cacheHits > 0 || parsedFiles > 0)
        {
            AnsiConsole.MarkupLine($"[dim]Cache: {cacheHits} unmodified files loaded, {parsedFiles} files parsed.[/]\n");
        }
    }

    private List<TexItem> ParseFile(string filePath, string relativePath)
    {
        var fileItems = new List<TexItem>();
        var content = File.ReadAllText(filePath);
        var cleanContent = StripComments(content);

        var lineStarts = BuildLineStarts(cleanContent);

        var scopes = new Stack<EnvironmentScope>();
        scopes.Push(new EnvironmentScope("document"));

        var matches = _lexerRegex.Matches(cleanContent);

        foreach (Match match in matches)
        {
            ProcessMatch(match, scopes, fileItems, relativePath, lineStarts);
        }

        return fileItems;
    }

    private static List<int> BuildLineStarts(string content)
    {
        var lineStarts = new List<int> { 0 };
        for (int i = 0; i < content.Length; i++)
        {
            if (content[i] == '\n')
                lineStarts.Add(i + 1);
        }
        return lineStarts;
    }

    private void ProcessMatch(Match match, Stack<EnvironmentScope> scopes, List<TexItem> fileItems, string relativePath, List<int> lineStarts)
    {
        if (match.Groups["begin"].Success)
        {
            scopes.Push(new EnvironmentScope(match.Groups["env"].Value.Trim()));
        }
        else if (match.Groups["end"].Success)
        {
            if (scopes.Count > 1)
                scopes.Pop();
        }
        else if (match.Groups["inc"].Success)
        {
            scopes.Peek().LatestGraphic = match.Groups["path"].Value.Trim();
        }
        else if (match.Groups["caption"].Success)
        {
            var rawCaption = match.Groups["cap"].Value;
            var cleanCaption = Regex.Replace(rawCaption, @"\s+", " ").Trim();
            scopes.Peek().LatestCaption = cleanCaption;
        }
        else if (match.Groups["label"].Success)
        {
            AddLabel(match, scopes, fileItems, relativePath, lineStarts);
        }
    }

    private void AddLabel(Match match, Stack<EnvironmentScope> scopes, List<TexItem> fileItems, string relativePath, List<int> lineStarts)
    {
        var currentScope = scopes.Peek();
        string labelName = match.Groups["lab"].Value.Trim();

        fileItems.Add(new TexItem(
            currentScope.Name,
            labelName,
            relativePath,
            GetLineNumber(match.Index, lineStarts),
            currentScope.LatestGraphic ?? string.Empty,
            currentScope.LatestCaption));
    }

    private static int GetLineNumber(int matchIndex, List<int> lineStarts)
    {
        int index = lineStarts.BinarySearch(matchIndex);
        if (index < 0)
            index = ~index - 1;
        return index + 1;
    }

    private static string StripComments(string text)
    {
        var commentStringBuilder = new StringBuilder(text.Length);
        bool escaped = false;
        bool inComment = false;

        foreach (char c in text)
        {
            if (inComment)
            {
                if (c == '\n' || c == '\r')
                {
                    inComment = false;
                    commentStringBuilder.Append(c);
                }
                continue;
            }

            if (c == '\\')
            {
                escaped = !escaped;
                commentStringBuilder.Append(c);
                continue;
            }
            
            if (c == '%' && !escaped)
            {
                inComment = true;
                continue;
            }

            escaped = false;
            commentStringBuilder.Append(c);
        }

        return commentStringBuilder.ToString();
    }

    private static bool IsWithinRootDirectory(string filePath, string rootDirectory)
    {
        var absoluteFilePath = Path.GetFullPath(filePath);
        var absoluteRootDirectory = Path.GetFullPath(rootDirectory);

        if (!absoluteFilePath.StartsWith(absoluteRootDirectory, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var relativePath = Path.GetRelativePath(absoluteRootDirectory, absoluteFilePath);
        return !relativePath.StartsWith("..", StringComparison.Ordinal);
    }
}