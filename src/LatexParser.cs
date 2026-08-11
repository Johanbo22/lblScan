using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace lblScan;

public class LatexParser
{
    private readonly Regex _lexerRegex = new(
        @"\\(?:(?<begin>begin)\s*\{(?<env>[^}]+)\}|(?<end>end)\s*\{(?<env_end>[^}]+)\}|(?<label>label)\s*\{(?<lab>[^}]+)\}|(?<inc>includegraphics)\s*(?:\[[^\]]*\])?\s*\{(?<path>[^}]+)\})",
        RegexOptions.Compiled | RegexOptions.Singleline);

    public List<TexItem> ParseDirectory(string rootDirectory)
    {
        var extractedData = new List<TexItem>();
        var texFiles = Directory.EnumerateFiles(rootDirectory, "*.tex", SearchOption.AllDirectories);

        foreach (var file in texFiles)
        {
            var content = File.ReadAllText(file);
            var cleanContent = StripComments(content);

            var scopes = new Stack<EnvironmentScope>();
            scopes.Push(new EnvironmentScope("document"));

            var matches = _lexerRegex.Matches(cleanContent);

            foreach (Match match in matches)
            {
                if (match.Groups["begin"].Success)
                {
                    scopes.Push(new EnvironmentScope(match.Groups["env"].Value.Trim()));
                }
                else if (match.Groups["end"].Success)
                {
                    if (scopes.Count > 1)
                    {
                        scopes.Pop();
                    }
                }
                else if (match.Groups["inc"].Success)
                {
                    scopes.Peek().LatestGraphic = match.Groups["path"].Value.Trim();
                }
                else if (match.Groups["label"].Success)
                {
                    var currentScope = scopes.Peek();
                    string labelName = match.Groups["lab"].Value.Trim();

                    extractedData.Add(new TexItem(
                        currentScope.Name, labelName, currentScope.LatestGraphic ?? string.Empty));
                }
            }
        }

        return extractedData;
    }

    private static string StripComments(string text)
    {
        var sb = new StringBuilder(text.Length);
        bool escaped = false;
        bool inComment = false;

        foreach (char c in text)
        {
            if (inComment)
            {
                if (c == '\n' || c == '\r')
                {
                    inComment = false;
                    sb.Append(c);
                }
                continue;
            }

            if (c == '\\')
            {
                escaped = !escaped;
                sb.Append(c);
            }
            else if (c == '%' && !escaped)
            {
                inComment = true;
            }
            else
            {
                escaped = false;
                sb.Append(c);
            }
        }

        return sb.ToString();
    }
}