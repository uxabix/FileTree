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
        
        var options = new FileTreeOptions
        {
            MaxDepth = opts.MaxDepth ?? -1,
            MaxWidth = opts.MaxWidth ?? -1,
            MaxNodes = opts.MaxNodes ?? -1,
            UseGitIgnore = opts.UseGitIgnore ?? false,
            Hidden = opts.Hidden ?? false,
            Format = opts.Format ?? OutputFormat.Ascii,
            Filter = new FilterOptions
            {
                IncludeExtensions = opts.IncludeExtensions?.ToList() ?? new List<string>(),
                ExcludeExtensions = opts.ExcludeExtensions?.ToList() ?? new List<string>(),
                IncludeNames = opts.IncludeNames?.ToList() ?? new List<string>(),
                ExcludeNames = opts.ExcludeNames?.ToList() ?? new List<string>(),
                IgnoreEmptyFolders = opts.IgnoreEmptyFolders ?? false
            }
        };

        Console.WriteLine($"Scanning directory: {targetPath}");
        Console.WriteLine($"Options: MaxDepth={options.MaxDepth}, Format={options.Format}, UseGitIgnore={options.UseGitIgnore}");
        
        FileTreeService service = new();
        Console.WriteLine(service.Generate(targetPath, options));
        
        Console.WriteLine("FileTreeService integration pending...");
    }
}