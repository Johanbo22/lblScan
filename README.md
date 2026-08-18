# lblScan

A command-line tool to extract all instances of a `\label{}` tag in a `.tex` source file.

Iterates through all sub-directories of a LaTeX project and compiles a table or a tree-view with the environment, label name and any associated graphic to that instance of `\label`.

## Requirements
This tool requires the **[.NET 10.0 SDK](https://dotnet.microsoft.com/en-us/download)** or newer installed

## Installation and usage
1. Clone repository

```
git clone https://github.com/Johanbo22/lblScan.git
cd lblScan
```

2. Build solution from root

```
dotnet pack
```

3. Install tool globally as a NuGet package

```
dotnet tool install --global --add-source ./nupkg lblScan
```

4. Navigate to a folder containing a LaTeX project and run

```
lblScan
```

## Arguments
The following flags alter the provided output:
- `--version` or `-v`: View the current version of lblScan.
- `--help` or `-h`: View all arguments and usage information.
- `--interactive` or `-i`: Launch an interactive table to filter results by typing, use arrow keys to navigate, and hit Enter to copy a label to your clipboard.
- `--tree` or `-t`: Display labels in a hierarchy tree grouped by the `.tex` file they were found within.
- `--tree-depth <value>`: Limit the tree view to a certain directory depth. 
- `--full` or `-f`: Full filepaths for graphics in the Associated file column. Omit to only display the file name of the graphic.
- `--caption` or `-c`: Include a snippet of the associated `\caption{}` text. By default this is not included.
- `--no-file` or `-nf`: Omit the Associated file linked to the `\label`.
- `--graphics` or `-g`: Only display rows that have an associated graphics file attached.
- `--env <"name">` or `-e <"name">`: Only display labels within a specific environment type (`table`, `figure`, `subfigure`, `tikzpicture`, etc.).
- `--sort-asc`: Sort the labels in alphabetical order (A-Z).
- `--sort-desc`: Sort the labels in reverse alphabetical order (Z-A).
- `--csv`: Write the contents of the scan to a CSV file.
- `--no-cache`: Force a complete re-scan of the project. By default `lblScan` writes a hidden `.lblscan_cache.json` file and will only re parse `.tex` files that have been modified since last execution.