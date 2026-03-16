using CommandLine;
using FileTree.Core.Models;

namespace FileTree.CLI;

[Verb("scan", isDefault: true, HelpText = "Scan and print the directory tree.")]
public class ScanCommandOptions
{
    [Value(0, MetaName = "path", HelpText = "Path to the directory to scan.", Required = false)]
    public string? Path { get; set; }

    [Option('p', "path", HelpText = "Path to the directory to scan.", Required = false)]
    public string? PathOption { get; set; }

    [Option('d', "max-depth", HelpText = "Maximum depth of the tree.", Required = false)]
    public int? MaxDepth { get; set; }

    [Option('w', "max-width", HelpText = "Maximum width (number of siblings) per level.", Required = false)]
    public int? MaxWidth { get; set; }

    [Option('n', "max-nodes", HelpText = "Maximum total number of nodes in the tree.", Required = false)]
    public int? MaxNodes { get; set; }

    [Option('g', "use-gitignore", HelpText = "Use .gitignore rules to filter files.", Required = false)]
    public bool? UseGitIgnore { get; set; }

    [Option('f', "format", HelpText = "Output format (Ascii, Markdown, Unicode).", Required = false)]
    public OutputFormat? Format { get; set; }

    // ============================================================================
    // New gitignore-style filtering options (recommended)
    // ============================================================================

    [Option("filter-rules", HelpText = "Filter rules in gitignore format (comma-separated). Example: '*.log,bin/,!important.txt'", Required = false, Separator = ',')]
    public IEnumerable<string>? FilterRules { get; set; }

    [Option("filter-file", HelpText = "Path to local filter configuration file with gitignore-style rules.", Required = false)]
    public string? FilterFile { get; set; }

    [Option("global-filter-file", HelpText = "Path to global filter configuration file with gitignore-style rules.", Required = false)]
    public string? GlobalFilterFile { get; set; }

    [Option("no-default-filters", HelpText = "Don't load default global filter configuration from ~/.filetreeignore", Required = false)]
    public bool NoDefaultFilters { get; set; }

    [Option('!', "no-app-global-ignore", HelpText = "Don't load app-level global filter file (FileTree.ignore near the executable).", Required = false)]
    public bool NoAppGlobalIgnore { get; set; }

    [Option("no-local-filters", HelpText = "Don't load local .filetreeignore files from the directory tree.", Required = false)]
    public bool NoLocalFilters { get; set; }

    // ============================================================================
    // Legacy filtering options (deprecated but maintained for backward compatibility)
    // ============================================================================

    [Option('i', "include-ext", HelpText = "[DEPRECATED] Include only these extensions (comma-separated). Use --filter-rules instead.", Required = false, Separator = ',')]
    public IEnumerable<string>? IncludeExtensions { get; set; }

    [Option('e', "exclude-ext", HelpText = "[DEPRECATED] Exclude these extensions (comma-separated). Use --filter-rules instead.", Required = false, Separator = ',')]
    public IEnumerable<string>? ExcludeExtensions { get; set; }

    [Option("include-names", HelpText = "[DEPRECATED] Include files with these names (comma-separated). Use --filter-rules instead.", Required = false, Separator = ',')]
    public IEnumerable<string>? IncludeNames { get; set; }

    [Option("exclude-names", HelpText = "[DEPRECATED] Exclude files with these names (comma-separated). Use --filter-rules instead.", Required = false, Separator = ',')]
    public IEnumerable<string>? ExcludeNames { get; set; }

    [Option("ignore-empty", HelpText = "Skip empty folders.", Required = false)]
    public bool IgnoreEmptyFolders { get; set; }

    [Option('h', "hidden", HelpText = "Exclude hidden files and folders.", Required = false)]
    public bool? SkipHidden { get; set; }

    [Option("wait", HelpText = "Do not run immediately; enter interactive mode, accept more options, and run on 'show'.", Required = false)]
    public bool Wait { get; set; }
}

[Verb("install", HelpText = "Install FileTree into the system (PATH, context menu, etc.).")]
public class InstallCommandOptions
{
}

[Verb("uninstall", HelpText = "Uninstall FileTree from the system for the current user.")]
public class UninstallCommandOptions
{
}

[Verb("uninstall-deep", HelpText = "Deep uninstall FileTree (search and remove all FileTree entries for the current user).")]
public class UninstallDeepCommandOptions
{
}

[Verb("paths", HelpText = "Show the executable location and global filter file path.")]
public class PathsCommandOptions
{
}
