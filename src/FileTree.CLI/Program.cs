using System.Runtime.InteropServices;
using CommandLine;
using FileTree.Core.Models;
using FileTree.Core.Services;

namespace FileTree.CLI;

internal class Program
{
    private static int Main(string[] args)
    {
        int res = Parser.Default
            .ParseArguments<ScanCommandOptions, InstallCommandOptions, UninstallCommandOptions>(args)
            .MapResult(
                (ScanCommandOptions opts) => RunScan(opts),
                (InstallCommandOptions _) => RunInstallAsync().GetAwaiter().GetResult(),
                (UninstallCommandOptions _) => RunUninstallAsync().GetAwaiter().GetResult(),
                _ => 1);
        Console.ReadKey();

        return res;
    }

    private static int RunScan(ScanCommandOptions opts)
    {
        var targetPath = opts.Path ?? Directory.GetCurrentDirectory();

        var options = new FileTreeOptions
        {
            MaxDepth = opts.MaxDepth ?? -1,
            MaxWidth = opts.MaxWidth ?? -1,
            MaxNodes = opts.MaxNodes ?? -1,
            UseGitIgnore = opts.UseGitIgnore ?? true,
            SkipHidden = opts.SkipHidden ?? true,
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

        return 0;
    }

    private static async Task<int> RunInstallAsync()
    {
        var integrator = SystemIntegratorFactory.Create();
        await integrator.InstallAsync();
        return 0;
    }

    private static async Task<int> RunUninstallAsync()
    {
        var integrator = SystemIntegratorFactory.Create();
        await integrator.UninstallAsync();
        return 0;
    }
}