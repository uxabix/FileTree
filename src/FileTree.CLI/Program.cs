using CommandLine;
using FileTree.Core.Models;
using FileTree.Core.Services;

namespace FileTree.CLI;

internal class Program
{
    private static void Main(string[] args)
    {
        Parser.Default.ParseArguments<CommandLineOptions>(args)
            .WithParsed(RunOptions);
    }

    private static void RunOptions(CommandLineOptions opts)
    {
        var targetPath = opts.Path ?? Directory.GetCurrentDirectory();

        // Build filter rules source from new-style options
        FilterRulesSource? rulesSource = null;

        // Check if any new-style filtering options are provided
        if (opts.FilterRules?.Any() == true ||
            !string.IsNullOrWhiteSpace(opts.FilterFile) ||
            !string.IsNullOrWhiteSpace(opts.GlobalFilterFile) ||
            opts.NoDefaultFilters)
        {
            rulesSource = new FilterRulesSource
            {
                InlineRules = opts.FilterRules?.ToList() ?? new List<string>(),
                LocalConfigPath = opts.FilterFile,
                GlobalConfigPath = opts.GlobalFilterFile,
                UseDefaultGlobalConfig = !opts.NoDefaultFilters
            };
        }

        // Build filter options (supports both new and legacy filtering)
        var filterOptions = new FilterOptions
        {
            RulesSource = rulesSource,
            IgnoreEmptyFolders = opts.IgnoreEmptyFolders ?? false,

            // Legacy options (for backward compatibility)
            #pragma warning disable CS0618 // Type or member is obsolete
            IncludeExtensions = opts.IncludeExtensions?.ToList() ?? new List<string>(),
            ExcludeExtensions = opts.ExcludeExtensions?.ToList() ?? new List<string>(),
            IncludeNames = opts.IncludeNames?.ToList() ?? new List<string>(),
            ExcludeNames = opts.ExcludeNames?.ToList() ?? new List<string>()
            #pragma warning restore CS0618 // Type or member is obsolete
        };

        var options = new FileTreeOptions
        {
            MaxDepth = opts.MaxDepth ?? -1,
            MaxWidth = opts.MaxWidth ?? -1,
            MaxNodes = opts.MaxNodes ?? -1,
            UseGitIgnore = opts.UseGitIgnore ?? true,
            SkipHidden = opts.SkipHidden ?? true,
            Format = opts.Format ?? OutputFormat.Ascii,
            Filter = filterOptions
        };

        Console.WriteLine($"Scanning directory: {targetPath}");
        Console.WriteLine($"Options: MaxDepth={options.MaxDepth}, Format={options.Format}, UseGitIgnore={options.UseGitIgnore}");

        FileTreeService service = new();
        Console.WriteLine(service.Generate(targetPath, options));

        Console.WriteLine("FileTreeService integration pending...");
    }
}