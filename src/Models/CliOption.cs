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
}