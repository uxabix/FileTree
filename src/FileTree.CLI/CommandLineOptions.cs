using CommandLine;
using FileTree.Core.Models;

namespace FileTree.CLI;

[Verb("scan", isDefault: true, HelpText = "Scan and print the directory tree.")]
/// <summary>
/// Options for the default 'scan' command that generates and prints a visual directory tree.
/// Supports tree limits, formatting, filtering (gitignore-style or legacy), hidden files, and output behaviors.
/// Example: <c>filetree . --max-depth 3 --format Markdown --filter-rules '*.log,bin/'</c>
/// </summary>
public class ScanCommandOptions
{
    /// <param name="Path">Target directory path (positional argument). Defaults to current directory if omitted.</param>
    [Value(0, MetaName = "path", HelpText = "Path to the directory to scan.", Required = false)]
    public string? Path { get; set; }

    /// <param name="PathOption">Alternative path specification via short/long option. Overrides positional if both provided.</param>
    [Option('p', "path", HelpText = "Path to the directory to scan.", Required = false)]
    public string? PathOption { get; set; }

    /// <param name="MaxDepth">Limit tree recursion depth. -1 = unlimited (default).</param>
    [Option('d', "max-depth", HelpText = "Maximum depth of the tree.", Required = false)]
    public int? MaxDepth { get; set; }

    /// <param name="MaxWidth">Limit siblings per directory. -1 = unlimited (default). Helps wide dirs.</param>
    [Option('w', "max-width", HelpText = "Maximum width (number of siblings) per level.", Required = false)]
    public int? MaxWidth { get; set; }

    /// <param name="MaxNodes">Global cap on tree nodes. -1 = unlimited (default). Prevents huge outputs.</param>
    [Option('n', "max-nodes", HelpText = "Maximum total number of nodes in the tree.", Required = false)]
    public int? MaxNodes { get; set; }

    /// <param name="UseGitIgnore">Apply .gitignore/.git/info/exclude rules from repo root. Default: true if .git found.</param>
    [Option('g', "use-gitignore", HelpText = "Use .gitignore rules to filter files (true|false).", Required = false)]
    public bool? UseGitIgnore { get; set; }

    /// <param name="Format">Tree rendering style. Default: <see cref="OutputFormat.Ascii"/>.</param>
    [Option('f', "format", HelpText = "Output format (Ascii, Markdown, Unicode).", Required = false)]
    public OutputFormat? Format { get; set; }

    /// <param name="CollapseThreshold">Collapse dirs with > N children. Null = auto based on width/depth.</param>
    [Option("collapse-threshold",
        HelpText = "Collapse directories when item count exceeds this value.", Required = false)]
    public int? CollapseThreshold { get; set; }

    /// <param name="CollapseKeepStart">Show first N children before placeholder. Default: 1.</param>
    [Option("collapse-keep-start",
        HelpText = "Number of items to keep at the start when collapsing.", Required = false)]
    public int? CollapseKeepStart { get; set; }

    /// <param name="CollapseKeepEnd">Show last N children after placeholder. Default: 1.</param>
    [Option("collapse-keep-end",
        HelpText = "Number of items to keep at the end when collapsing.", Required = false)]
    public int? CollapseKeepEnd { get; set; }

    /// <param name="CollapseStyle">Placeholder representation. Default: <see cref="CollapseStyle.Count"/>.</param>
    [Option("collapse-style",
        HelpText = "Collapse placeholder style (Simple, Count, ByExtension).", Required = false)]
    public CollapseStyle? CollapseStyle { get; set; }

    /// <param name="CollapseFrom">Min depth for collapsing (1=root children). Default: 1.</param>
    [Option("collapse-from",
        HelpText = "Start collapsing only from this depth (1 = first level under root).", Required = false)]
    public int? CollapseFrom { get; set; }

    /// <remarks>
    /// <list type="bullet">
    /// <item><b>Recommended modern filtering</b> with gitignore syntax.</item>
    /// <item>Precedence: inline > local/global files > gitignore.</item>
    /// </list>
    /// </remarks>
    [Option("filter-rules",
        HelpText = "Filter rules in gitignore format (comma-separated). Example: '*.log,bin/,!important.txt'",
        Required = false, Separator = ',')]
    /// <param name="FilterRules">Inline gitignore-style patterns. !negates. Multiple via comma or repeat.</param>
    public IEnumerable<string>? FilterRules { get; set; }

    /// <param name="FilterFile">Path to .filetreeignore in target dir or subdir. Auto-searched if omitted.</param>
    [Option("filter-file", HelpText = "Path to local filter configuration file with gitignore-style rules.",
        Required = false)]
    public string? FilterFile { get; set; }

    /// <param name="GlobalFilterFile">Override app default/global .filetreeignore (near exe). Multiple levels supported.</param>
    [Option("global-filter-file", HelpText = "Path to global filter configuration file with gitignore-style rules.",
        Required = false)]
    public string? GlobalFilterFile { get; set; }

    [Option("no-default-filters",
        HelpText = "Don't load default global filter configuration from ~/.filetreeignore (true|false).",
        Required = false)]
    public bool? NoDefaultFilters { get; set; }

    /// <param name="NoAppGlobalIgnore">Skip FileTree.ignore near executable. Short: '!'.</param>
    [Option('!', "no-app-global-ignore",
        HelpText = "Don't load app-level global filter file (FileTree.ignore near the executable) (true|false).",
        Required = false)]
    public bool? NoAppGlobalIgnore { get; set; }

    /// <param name="NoLocalFilters">Disable recursive .filetreeignore loading. Default: load.</param>
    [Option("no-local-filters",
        HelpText = "Don't load local .filetreeignore files from the directory tree (true|false).", Required = false)]
    public bool? NoLocalFilters { get; set; }

    [Option("no-default-settings", HelpText = "Don't load default settings from FileTree.Settings (true|false).",
        Required = false)]
    public bool? NoDefaultSettings { get; set; }

    /// <remarks><b>Legacy filtering</b> - use <see cref="FilterRules"/> instead for gitignore syntax.</remarks>
    // ============================================================================
    // Legacy filtering options (deprecated but maintained for backward compatibility)
    // ============================================================================

    /// <param name="IncludeExtensions">Only show files with these extensions (e.g. &apos;cs,js&apos;). DEPRECATED.</param>
    [Option('i', "include-ext",
        HelpText = "[DEPRECATED] Include only these extensions (comma-separated). Use --filter-rules instead.",
        Required = false, Separator = ',')]
    public IEnumerable<string>? IncludeExtensions { get; set; }

    [Option('e', "exclude-ext",
        HelpText = "[DEPRECATED] Exclude these extensions (comma-separated). Use --filter-rules instead.",
        Required = false, Separator = ',')]
    public IEnumerable<string>? ExcludeExtensions { get; set; }

    [Option("include-names",
        HelpText = "[DEPRECATED] Include files with these names (comma-separated). Use --filter-rules instead.",
        Required = false, Separator = ',')]
    public IEnumerable<string>? IncludeNames { get; set; }

    [Option("exclude-names",
        HelpText = "[DEPRECATED] Exclude files with these names (comma-separated). Use --filter-rules instead.",
        Required = false, Separator = ',')]
    public IEnumerable<string>? ExcludeNames { get; set; }

    [Option("ignore-empty", HelpText = "Skip empty folders (true|false).", Required = false)]
    public bool? IgnoreEmptyFolders { get; set; }

    /// <param name="SkipHidden">Hide hidden files/dirs (Unix dotfiles, Windows hidden attr). Default: true. Short: &apos;h&apos;.</param>
    [Option('h', "hidden", HelpText = "Exclude hidden files and folders (true|false).", Required = false)]
    public bool? SkipHidden { get; set; }

    /// <param name="HighlightHiddenFiles">Style hidden nodes (if not skipped). Default: true.</param>
    [Option("highlight-hidden", HelpText = "Highlight hidden files and folders in output (true|false).",
        Required = false)]
    public bool? HighlightHiddenFiles { get; set; }

    /// <param name="HiddenStyle">Marker for hidden (Prefix/Suffix/Minimal). Default: Suffix.</param>
    [Option("hidden-style", HelpText = "Hidden file style (Prefix, Suffix, Minimal).", Required = false)]
    public HiddenStyle? HiddenStyle { get; set; }

    /// <param name="Wait">Interactive mode: accept more options until 'show'.</param>
    [Option("wait",
        HelpText =
            "Do not run immediately; enter interactive mode, accept more options, and run on 'show' (true|false).",
        Required = false)]
    public bool? Wait { get; set; }

    /// <param name="Copy">Copy tree to clipboard after generation. Short: 'c'.</param>
    [Option('c', "copy", HelpText = "Copy output to clipboard")]
    public bool Copy { get; set; }

    /// <param name="Silent">Suppress console output (use with --copy). Short: &apos;s&apos;.</param>
    [Option('s', "silent", HelpText = "Do not print output to console")]
    public bool Silent { get; set; }

    /// <param name="ShowOptions">Print summary options before tree.</param>
    [Option("show-options", HelpText = "Print core scan options before output")]
    public bool ShowOptions { get; set; }

    /// <param name="ShowOptionsAll">Print verbose all-options before tree.</param>
    [Option("show-options-all", HelpText = "Print all scan options before output")]
    public bool ShowOptionsAll { get; set; }
}

[Verb("install", HelpText = "Install FileTree into the system (PATH, context menu, etc.).")]
/// <summary>
/// Options for the 'install' command. Installs FileTree to user PATH, creates shims/aliases, adds Windows context menus, ensures global config files.
/// Platform-specific: Full on Windows, instructions on Linux.
/// </summary>
public class InstallCommandOptions
{
}

[Verb("uninstall", HelpText = "Uninstall FileTree from the system for the current user.")]
/// <summary>
/// Options for the 'uninstall' command. Removes user-specific installation (PATH, shims, aliases, context menus, config files).
/// </summary>
public class UninstallCommandOptions
{
}

[Verb("uninstall-deep",
    HelpText = "Deep uninstall FileTree (search and remove all FileTree entries for the current user).")]
/// <summary>
/// Options for the 'uninstall-deep' command. Scans and prompts removal of all FileTree entries (PATH/shims/aliases/context menus).
/// Safer than uninstall for multiple/unknown installs.
/// </summary>
public class UninstallDeepCommandOptions
{
}

[Verb("paths", HelpText = "Show the executable location and global filter file path.")]
/// <summary>
/// Options for the 'paths' command. Displays executable location, directory, and global config file paths.
/// Useful for scripting/config verification.
/// Example output: Executable: C:\path\to\FileTree.exe, Global ignore: ...\FileTree.ignore.txt
/// </summary>
public class PathsCommandOptions
{
}
