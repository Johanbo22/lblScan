using System;
using System.IO;
using System.Linq;
using Spectre.Console;
using lblScan;

bool showFullPath = args.Contains("--full") || args.Contains("-f");
string rootDir = Directory.GetCurrentDirectory();

AnsiConsole.MarkupLine($"[dim]Scanning LaTeX project in: {rootDir}[/]\n");

try
{
    var parser = new LatexParser();
    var extractedData = parser.ParseDirectory(rootDir);

    var renderer = new ConsoleRenderer();
    renderer.RenderTable(extractedData, showFullPath);
}
catch (Exception ex)
{
    AnsiConsole.MarkupLine($"[red]An error occurred while scanning: {ex.Message}[/]");
}