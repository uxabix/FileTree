using CommandLine;
using FileTree.Core.Models;

namespace FileTree.CLI;

public class CommandLineOptions
{
    [Value(0, MetaName = "path", HelpText = "Path to the directory to scan.", Required = false)]
    public string? Path { get; set; }

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

    [Option('i', "include-ext", HelpText = "Include only these extensions (comma-separated).", Required = false, Separator = ',')]
    public IEnumerable<string>? IncludeExtensions { get; set; }

    [Option('e', "exclude-ext", HelpText = "Exclude these extensions (comma-separated).", Required = false, Separator = ',')]
    public IEnumerable<string>? ExcludeExtensions { get; set; }

    [Option("include-names", HelpText = "Include files with these names (comma-separated).", Required = false, Separator = ',')]
    public IEnumerable<string>? IncludeNames { get; set; }

    [Option("exclude-names", HelpText = "Exclude files with these names (comma-separated).", Required = false, Separator = ',')]
    public IEnumerable<string>? ExcludeNames { get; set; }

    [Option("ignore-empty", HelpText = "Skip empty folders.", Required = false)]
    public bool? IgnoreEmptyFolders { get; set; }

    [Option('h', "hidden", HelpText = "Exclude hidden files and folders.", Required = false)]
    public bool? SkipHidden { get; set; }
}
