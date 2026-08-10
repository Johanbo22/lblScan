# lblScan

A small command-line tool to extract all instances of a `\label{}` tag in a `.tex` source file.

Iterates through all sub-directories of a LaTeX project and compiles a table with the environment, label name and any associated graphic to that instance of `\label`.

## Requirements
This tool requires the **[.NET 10.0 SDK](https://dotnet.microsoft.com/en-us/download)** or newer installed

## Usage
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

Append a `--full` or a `-f` to have full filepaths included for graphics in the result. Omit to only display the file name of the graphic.