namespace lblScan.Models;

/// <summary>
/// Represents the state of a single Latex environment scope
/// </summary>
public class EnvironmentScope
{
    public string Name { get; }

    // Tracks the most recent graphic found within this specific scope
    public string? LatestGraphic { get; set; }

    public EnvironmentScope(string name)
    {
        Name = name;
    }
}