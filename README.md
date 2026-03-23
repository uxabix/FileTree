## FileTree.Core

FileTree.Core is a .NET 8 library and CLI tool for generating directory trees with rich filtering, depth/width limits, and support for `.gitignore` rules.  
It can be used both as a reusable library in your applications and as a command‑line tool for quickly inspecting project structures.

## Features

- **Multiple output formats**: `Ascii`, `Unicode`, `Markdown`
- **Respects `.gitignore`**: optionally skip ignored files and folders
- **Depth/width limits**: constrain recursion depth, per‑level width, and total node count
- **Flexible filtering**: include/exclude by extension or name, skip empty folders, hide hidden files
- **Library + CLI**: use `FileTreeService` in code or via `dotnet run`

## Requirements

- **.NET SDK**: 8.0 or later

## Getting started

Clone the repository and restore/build:

```bash
dotnet restore
dotnet build
```

### Running the CLI

From the repository root:

```bash
dotnet run --project .\src\FileTree.CLI\FileTree.CLI.csproj -- -d 5 -f Unicode -e .cs,.json -h
```

This command:

- **Scans** the current directory (no explicit path provided)
- **Limits depth** to 5 levels (`-d 5`)
- **Uses Unicode** tree characters (`-f Unicode`)
- **Excludes** `.cs` and `.json` files (`-e .cs,.json`)
- **Skips hidden** files and folders (`-h`)

### Command‑line options

All options are optional; sensible defaults are applied when they are omitted.

| Option | Long name        | Type      | Description |
|--------|------------------|-----------|-------------|
| `path` | —                | value     | Path to the directory to scan. Defaults to the current directory. |
| `-d`   | `--max-depth`    | `int`     | Maximum depth of the tree. `-1` (default) means unlimited. |
| `-w`   | `--max-width`    | `int`     | Maximum number of siblings per level. `-1` (default) means unlimited. |
| `-n`   | `--max-nodes`    | `int`     | Maximum total number of nodes in the tree. `-1` (default) means unlimited. |
| `-g`   | `--use-gitignore`| `bool`    | Use `.gitignore` rules to filter files. Defaults to `true` when omitted. |
| `-f`   | `--format`       | enum      | Output format: `Ascii`, `Markdown`, or `Unicode`. Default is `Ascii`. |
| `-i`   | `--include-ext`  | list      | Comma‑separated list of extensions to include, e.g. `-i .cs,.csproj`. |
| `-e`   | `--exclude-ext`  | list      | Comma‑separated list of extensions to exclude, e.g. `-e .dll,.pdb`. |
| —      | `--include-names`| list      | Comma‑separated file/directory names to always include. |
| —      | `--exclude-names`| list      | Comma‑separated file/directory names to exclude. |
| —      | `--ignore-empty` | flag      | Skip empty folders. |
| `-h`   | `--hidden`       | flag      | Exclude hidden files and folders. Defaults to `true` when omitted. |

#### More examples

Scan a specific folder with Markdown output:

```bash
dotnet run --project .\src\FileTree.CLI\FileTree.CLI.csproj -- "C:\Projects\MyApp" -f Markdown
```

Include only C# sources and ignore empty directories:

```bash
dotnet run --project .\src\FileTree.CLI\FileTree.CLI.csproj -- -i .cs -d 4 --ignore-empty
```

Disable `.gitignore` handling and show hidden files:

```bash
dotnet run --project .\src\FileTree.CLI\FileTree.CLI.csproj -- -g false -h false
```

## Library usage

You can use the core scanning and formatting functionality directly from C# via `FileTreeService` in `FileTree.Core`.

```csharp
using FileTree.Core.Models;
using FileTree.Core.Services;

var options = new FileTreeOptions
{
    MaxDepth = 5,
    MaxWidth = -1,
    MaxNodes = -1,
    UseGitIgnore = true,
    SkipHidden = true,
    Format = OutputFormat.Unicode,
    Filter = new FilterOptions
    {
        IncludeExtensions = new() { ".cs", ".csproj" },
        IgnoreEmptyFolders = true
    }
};

FileTreeService service = new();
string tree = service.Generate(@"C:\Projects\MyApp", options);

Console.WriteLine(tree);
```

## Output formats

- **Ascii**: Plain ASCII tree (`|--`, `+--`)
- **Unicode**: Box‑drawing characters for a nicer console tree
- **Markdown**: Tree formatted for embedding in Markdown documents

The format is selected via `OutputFormat` in code or `-f/--format` in the CLI.

## Development

- **Run tests**:

```bash
dotnet test
```

The main projects are:

- `src/FileTree.Core` — core library (scanning, filtering, formatting)
- `src/FileTree.CLI` — command‑line interface
- `tests/FileTree.Core.Tests` — unit tests

## License

This project is licensed under the **Apache License 2.0**.  
See the `LICENSE` file for full details.

