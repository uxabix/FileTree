using System.Text;
using CommandLine;
using FileTree.Core.Models;
using FileTree.Core.Services;
using FileTree.Core.Utilities;
using FileTree.CLI.SystemIntegrator;

namespace FileTree.CLI;

internal class Program
{
    private static int Main(string[] args)
    {
        if (TryHandleConfigRules(args, out var configExitCode))
        {
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
            return configExitCode;
        }

        EnsureGlobalIgnoreFileOnStartup(args);

        int res = Parser.Default
            .ParseArguments<ScanCommandOptions, InstallCommandOptions, UninstallCommandOptions, UninstallDeepCommandOptions, PathsCommandOptions>(args)
            .MapResult(
                (ScanCommandOptions opts) => RunScan(opts),
                (InstallCommandOptions _) => RunInstallAsync().GetAwaiter().GetResult(),
                (UninstallCommandOptions _) => RunUninstallAsync().GetAwaiter().GetResult(),
                (UninstallDeepCommandOptions _) => RunUninstallDeepAsync().GetAwaiter().GetResult(),
                (PathsCommandOptions _) => RunPaths(),
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
        // Basic validation to catch common parsing mistakes
        if (opts.Path == "true" || opts.Path == "false")
        {
            opts.Path = null; // Treat as if no path was provided
        }
        var targetPath = opts.PathOption ?? opts.Path ?? Directory.GetCurrentDirectory();

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
                RulesSource = BuildFilterRulesSource(opts),
                IncludeExtensions = opts.IncludeExtensions?.ToList() ?? new List<string>(),
                ExcludeExtensions = opts.ExcludeExtensions?.ToList() ?? new List<string>(),
                IncludeNames = opts.IncludeNames?.ToList() ?? new List<string>(),
                ExcludeNames = opts.ExcludeNames?.ToList() ?? new List<string>(),
                IgnoreEmptyFolders = opts.IgnoreEmptyFolders,
            }
        };

        Console.WriteLine($"Scanning directory: {targetPath}");
        Console.WriteLine($"Options: MaxDepth={options.MaxDepth}, Format={options.Format}, UseGitIgnore={options.UseGitIgnore}");

        var service = new FileTreeService();
        try
        {
            Console.WriteLine(service.Generate(targetPath, options));
            return 0;
        }
        catch (PathValidationException ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine($"Error: {ex.Message}");
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Error.WriteLine($"Path: {ex.Path}");
            Console.Error.WriteLine("Type: " + ex.ErrorType);
            Console.ResetColor();
            Console.Error.WriteLine();
            return 1;
        }
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
                    try
                    {
                        return RunScanOnce(current);
                    }
                    catch (PathValidationException)
                    {
                        // Error already handled in RunScanOnce
                        return 1;
                    }
                }

            if (string.Equals(trimmed, "exit", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(trimmed, "quit", StringComparison.OrdinalIgnoreCase))
            {
                return 0;
            }

            if (string.Equals(trimmed, "help", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(trimmed, "?", StringComparison.OrdinalIgnoreCase))
            {
                var helpParser = new Parser(cfg => cfg.HelpWriter = Console.Out);
                helpParser.ParseArguments<ScanCommandOptions>(new[] { "--help" });
                continue;
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
        if (!string.IsNullOrWhiteSpace(source.PathOption))
        {
            target.Path = source.PathOption;
            target.PathOption = source.PathOption;
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

        if (source.FilterRules is not null)
        {
            target.FilterRules = source.FilterRules;
        }

        if (!string.IsNullOrWhiteSpace(source.FilterFile))
        {
            target.FilterFile = source.FilterFile;
        }

        if (!string.IsNullOrWhiteSpace(source.GlobalFilterFile))
        {
            target.GlobalFilterFile = source.GlobalFilterFile;
        }

        if (source.NoDefaultFilters)
        {
            target.NoDefaultFilters = source.NoDefaultFilters;
        }

        if (source.NoAppGlobalIgnore)
        {
            target.NoAppGlobalIgnore = source.NoAppGlobalIgnore;
        }

        if (source.IgnoreEmptyFolders)
        {
            target.IgnoreEmptyFolders = source.IgnoreEmptyFolders;
        }

        if (source.SkipHidden.HasValue)
        {
            target.SkipHidden = source.SkipHidden;
        }

        if (source.Wait)
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

    private static FilterRulesSource BuildFilterRulesSource(ScanCommandOptions opts)
    {
        var inlineRules = opts.FilterRules?
            .Where(rule => !string.IsNullOrWhiteSpace(rule))
            .Select(rule => rule.Trim())
            .ToList() ?? new List<string>();

        return new FilterRulesSource
        {
            AppGlobalConfigPath = AppPaths.GetGlobalIgnorePath(),
            UseAppGlobalConfig = !opts.NoAppGlobalIgnore,
            UseDefaultGlobalConfig = !opts.NoDefaultFilters,
            GlobalConfigPath = opts.GlobalFilterFile,
            LocalConfigPath = opts.FilterFile,
            InlineRules = inlineRules,
        };
    }

    private static void EnsureGlobalIgnoreFileOnStartup(string[] args)
    {
        if (args.Length > 0)
        {
            var command = args[0];
            if (string.Equals(command, "uninstall", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(command, "uninstall-deep", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
        }

        if (!AppPaths.TryEnsureGlobalIgnoreFileExists(out var path, out var error))
        {
            Console.Error.WriteLine($"Warning: Could not create global ignore file at '{path}': {error}");
        }
    }

    private static int RunPaths()
    {
        var exePath = AppPaths.GetExecutablePath();
        var exeDirectory = AppPaths.GetExecutableDirectory();
        var ignorePath = AppPaths.GetGlobalIgnorePath();

        Console.WriteLine($"Executable: {exePath}");
        Console.WriteLine($"Executable directory: {exeDirectory}");
        Console.WriteLine($"Global ignore file: {ignorePath}");
        return 0;
    }

    private static bool TryHandleConfigRules(string[] args, out int exitCode)
    {
        exitCode = 0;

        if (args.Length == 0)
        {
            return false;
        }

        if (!string.Equals(args[0], "config", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (args.Length < 2 || !string.Equals(args[1], "rules", StringComparison.OrdinalIgnoreCase))
        {
            PrintConfigRulesHelp();
            exitCode = 1;
            return true;
        }

        if (args.Length == 2)
        {
            if (!AppPaths.TryOpenGlobalIgnoreFile(out var path, out var error))
            {
                Console.Error.WriteLine($"Error: Could not open global ignore file at '{path}': {error}");
                exitCode = 1;
            }

            return true;
        }

        if (args.Length >= 3)
        {
            var action = args[2];
            if (string.Equals(action, "path", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine(AppPaths.GetGlobalIgnorePath());
                return true;
            }

            if (string.Equals(action, "reset", StringComparison.OrdinalIgnoreCase))
            {
                if (!AppPaths.TryResetGlobalIgnoreFile(out var path, out var error))
                {
                    Console.Error.WriteLine($"Error: Could not reset global ignore file at '{path}': {error}");
                    exitCode = 1;
                }
                else
                {
                    Console.WriteLine($"Reset global ignore file: {path}");
                }

                return true;
            }
        }

        PrintConfigRulesHelp();
        exitCode = 1;
        return true;
    }

    private static void PrintConfigRulesHelp()
    {
        Console.WriteLine("Usage:");
        Console.WriteLine("  FileTree config rules        Open global ignore file in default editor");
        Console.WriteLine("  FileTree config rules path   Show global ignore file path");
        Console.WriteLine("  FileTree config rules reset  Reset global ignore file to defaults");
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

    private static async Task<int> RunUninstallDeepAsync()
    {
        var integrator = SystemIntegratorFactory.Create();
        await integrator.UninstallDeepAsync();
        return 0;
    }
}
