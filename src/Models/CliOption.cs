namespace lblScan.Models;

/// <summary>
/// Defines a Command Line Interface option and handles argument matching
/// </summary>

public record CliOption(string ShortName, string LongName, string Description)
{
    public bool IsMatch(string[] args)
    {
        return (!string.IsNullOrEmpty(ShortName) && args.Contains(ShortName)) ||
               (!string.IsNullOrEmpty(LongName) && args.Contains(LongName));
    }

    public string? GetValue(string[] args)
    {
        for (int i = 0; i < args.Length; i++)
        {
            bool isMatch = (!string.IsNullOrEmpty(ShortName) && args[i] == ShortName) ||
                           (!string.IsNullOrEmpty(LongName) && args[i] == LongName);

            if (!isMatch)
            {
                continue;
            }

            if (i + 1 < args.Length && !args[i + 1].StartsWith("-"))
            {
                return args[i + 1];
            }
        }
        return null;
    }
}