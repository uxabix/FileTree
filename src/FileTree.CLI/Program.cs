using System.Text;
using CommandLine;
using FileTree.Core.Models;
using FileTree.Core.Services;
using FileTree.CLI.SystemIntegrator;

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
        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();

        return res;
    }

    private static int RunScan(ScanCommandOptions opts)
    {
        if (opts.Wait == true)
        {
            return RunScanInteractive(opts);
        }

        return RunScanOnce(opts);
    }

    private static int RunScanOnce(ScanCommandOptions opts)
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

    private static int RunScanInteractive(ScanCommandOptions opts)
    {
        var current = opts;

        Console.WriteLine("Interactive mode. Type additional options (e.g. -w 10 -n 200), then 'show' to print the tree or 'exit' to quit.");
        Console.WriteLine();

        while (true)
        {
            Console.Write("> ");
            var line = Console.ReadLine();

            if (line is null)
            {
                return 0;
            }

            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var trimmed = line.Trim();
            if (string.Equals(trimmed, "show", StringComparison.OrdinalIgnoreCase))
            {
                // In interactive mode we ignore the wait flag when running.
                current.Wait = false;
                return RunScanOnce(current);
            }

            if (string.Equals(trimmed, "exit", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(trimmed, "quit", StringComparison.OrdinalIgnoreCase))
            {
                return 0;
            }

            var args = TokenizeArguments(trimmed);
            if (args.Length == 0)
            {
                continue;
            }

            var parser = new Parser(cfg => cfg.HelpWriter = null);
            var result = parser.ParseArguments<ScanCommandOptions>(args);

            result
                .WithParsed(newOpts =>
                {
                    MergeScanOptions(current, newOpts);
                    Console.WriteLine("Updated options.");
                })
                .WithNotParsed(_ =>
                {
                    Console.WriteLine("Could not parse input. Please enter valid options or 'show'/'exit'.");
                });
        }
    }

    private static void MergeScanOptions(ScanCommandOptions target, ScanCommandOptions source)
    {
        if (!string.IsNullOrWhiteSpace(source.Path))
        {
            target.Path = source.Path;
        }

        if (source.MaxDepth.HasValue)
        {
            target.MaxDepth = source.MaxDepth;
        }

        if (source.MaxWidth.HasValue)
        {
            target.MaxWidth = source.MaxWidth;
        }

        if (source.MaxNodes.HasValue)
        {
            target.MaxNodes = source.MaxNodes;
        }

        if (source.UseGitIgnore.HasValue)
        {
            target.UseGitIgnore = source.UseGitIgnore;
        }

        if (source.Format.HasValue)
        {
            target.Format = source.Format;
        }

        if (source.IncludeExtensions is not null)
        {
            target.IncludeExtensions = source.IncludeExtensions;
        }

        if (source.ExcludeExtensions is not null)
        {
            target.ExcludeExtensions = source.ExcludeExtensions;
        }

        if (source.IncludeNames is not null)
        {
            target.IncludeNames = source.IncludeNames;
        }

        if (source.ExcludeNames is not null)
        {
            target.ExcludeNames = source.ExcludeNames;
        }

        if (source.IgnoreEmptyFolders.HasValue)
        {
            target.IgnoreEmptyFolders = source.IgnoreEmptyFolders;
        }

        if (source.SkipHidden.HasValue)
        {
            target.SkipHidden = source.SkipHidden;
        }

        if (source.Wait.HasValue)
        {
            target.Wait = source.Wait;
        }
    }

    private static string[] TokenizeArguments(string input)
    {
        var args = new List<string>();
        var current = new StringBuilder();
        var inQuotes = false;

        for (var i = 0; i < input.Length; i++)
        {
            var c = input[i];

            if (c == '"')
            {
                inQuotes = !inQuotes;
                continue;
            }

            if (char.IsWhiteSpace(c) && !inQuotes)
            {
                if (current.Length > 0)
                {
                    args.Add(current.ToString());
                    current.Clear();
                }

                continue;
            }

            current.Append(c);
        }

        if (current.Length > 0)
        {
            args.Add(current.ToString());
        }

        return args.ToArray();
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