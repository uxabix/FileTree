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
        EnsureGlobalIgnoreFileOnStartup(args);
        EnsureSettingsFileOnStartup(args);

        if (TryHandleConfigCommand(args, out var configExitCode))
        {
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
            return configExitCode;
        }

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
        ApplySettingsDefaultsIfNeeded(opts);

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
            HighlightHiddenFiles = opts.HighlightHiddenFiles ?? true,
            HiddenStyle = opts.HiddenStyle ?? HiddenStyle.Suffix,
            Format = opts.Format ?? OutputFormat.Ascii,
            Filter = new FilterOptions
            {
                RulesSource = BuildFilterRulesSource(opts),
                IncludeExtensions = opts.IncludeExtensions?.ToList() ?? new List<string>(),
                ExcludeExtensions = opts.ExcludeExtensions?.ToList() ?? new List<string>(),
                IncludeNames = opts.IncludeNames?.ToList() ?? new List<string>(),
                ExcludeNames = opts.ExcludeNames?.ToList() ?? new List<string>(),
                IgnoreEmptyFolders = opts.IgnoreEmptyFolders,
                UseLocalFilterFiles = !opts.NoLocalFilters,
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

        if (source.NoLocalFilters)
        {
            target.NoLocalFilters = source.NoLocalFilters;
        }

        if (source.NoDefaultSettings)
        {
            target.NoDefaultSettings = source.NoDefaultSettings;
        }

        if (source.IgnoreEmptyFolders)
        {
            target.IgnoreEmptyFolders = source.IgnoreEmptyFolders;
        }

        if (source.SkipHidden.HasValue)
        {
            target.SkipHidden = source.SkipHidden;
        }

        if (source.HighlightHiddenFiles.HasValue)
        {
            target.HighlightHiddenFiles = source.HighlightHiddenFiles;
        }

        if (source.HiddenStyle.HasValue)
        {
            target.HiddenStyle = source.HiddenStyle;
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

    private static void EnsureSettingsFileOnStartup(string[] args)
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

        if (!AppPaths.TryEnsureGlobalSettingsFileExists(out var path, out var error))
        {
            Console.Error.WriteLine($"Warning: Could not create settings file at '{path}': {error}");
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

    private static bool TryHandleConfigCommand(string[] args, out int exitCode)
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

        if (args.Length < 2)
        {
            PrintConfigHelp();
            exitCode = 1;
            return true;
        }

        var section = args[1];
        var remaining = args.Skip(2).ToArray();

        if (string.Equals(section, "rules", StringComparison.OrdinalIgnoreCase))
        {
            return HandleConfigRules(remaining, out exitCode);
        }

        if (string.Equals(section, "settings", StringComparison.OrdinalIgnoreCase))
        {
            return HandleConfigSettings(remaining, out exitCode);
        }

        PrintConfigHelp();
        exitCode = 1;
        return true;
    }

    private static bool HandleConfigRules(string[] args, out int exitCode)
    {
        exitCode = 0;

        if (args.Length == 0)
        {
            if (!AppPaths.TryOpenGlobalIgnoreFile(out var path, out var error))
            {
                Console.Error.WriteLine($"Error: Could not open global ignore file at '{path}': {error}");
                exitCode = 1;
            }

            return true;
        }

        var action = args[0];
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

        PrintConfigRulesHelp();
        exitCode = 1;
        return true;
    }

    private static bool HandleConfigSettings(string[] args, out int exitCode)
    {
        exitCode = 0;

        if (args.Length == 0)
        {
            if (!AppPaths.TryOpenGlobalSettingsFile(out var path, out var error))
            {
                Console.Error.WriteLine($"Error: Could not open settings file at '{path}': {error}");
                exitCode = 1;
            }

            return true;
        }

        var action = args[0];
        if (string.Equals(action, "path", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine(AppPaths.GetGlobalSettingsPath());
            return true;
        }

        if (string.Equals(action, "reset", StringComparison.OrdinalIgnoreCase))
        {
            if (!AppPaths.TryResetGlobalSettingsFile(out var path, out var error))
            {
                Console.Error.WriteLine($"Error: Could not reset settings file at '{path}': {error}");
                exitCode = 1;
            }
            else
            {
                Console.WriteLine($"Reset settings file: {path}");
            }

            return true;
        }

        PrintConfigSettingsHelp();
        exitCode = 1;
        return true;
    }

    private static void PrintConfigHelp()
    {
        Console.WriteLine("Usage:");
        Console.WriteLine("  FileTree config rules ...");
        Console.WriteLine("  FileTree config settings ...");
    }

    private static void PrintConfigRulesHelp()
    {
        Console.WriteLine("Usage:");
        Console.WriteLine("  FileTree config rules        Open global ignore file in default editor");
        Console.WriteLine("  FileTree config rules path   Show global ignore file path");
        Console.WriteLine("  FileTree config rules reset  Reset global ignore file to defaults");
    }

    private static void PrintConfigSettingsHelp()
    {
        Console.WriteLine("Usage:");
        Console.WriteLine("  FileTree config settings        Open settings file in default editor");
        Console.WriteLine("  FileTree config settings path   Show settings file path");
        Console.WriteLine("  FileTree config settings reset  Reset settings file to defaults");
    }

    private static void ApplySettingsDefaultsIfNeeded(ScanCommandOptions opts)
    {
        if (opts.NoDefaultSettings)
        {
            return;
        }

        var defaults = LoadSettingsDefaults();
        if (defaults == null)
        {
            return;
        }

        ApplySettingsDefaults(opts, defaults);
    }

    private static ScanCommandOptions? LoadSettingsDefaults()
    {
        var path = AppPaths.GetGlobalSettingsPath();
        if (!File.Exists(path))
        {
            return null;
        }

        try
        {
            var args = new List<string>();
            foreach (var line in File.ReadAllLines(path))
            {
                var trimmed = line?.Trim();
                if (string.IsNullOrEmpty(trimmed))
                {
                    continue;
                }

                if (trimmed.StartsWith("#", StringComparison.Ordinal))
                {
                    continue;
                }

                args.AddRange(TokenizeArguments(trimmed));
            }

            if (args.Count == 0)
            {
                return null;
            }

            if (string.Equals(args[0], "scan", StringComparison.OrdinalIgnoreCase))
            {
                args.RemoveAt(0);
            }

            var parser = new Parser(cfg => cfg.HelpWriter = null);
            ScanCommandOptions? settings = null;
            var result = parser.ParseArguments<ScanCommandOptions>(args);

            result
                .WithParsed(parsed => settings = parsed)
                .WithNotParsed(_ => settings = null);

            return settings;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Warning: Could not read settings file '{path}': {ex.Message}");
            return null;
        }
    }

    private static void ApplySettingsDefaults(ScanCommandOptions target, ScanCommandOptions defaults)
    {
        if (string.IsNullOrWhiteSpace(target.PathOption) && string.IsNullOrWhiteSpace(target.Path))
        {
            if (!string.IsNullOrWhiteSpace(defaults.PathOption))
            {
                target.PathOption = defaults.PathOption;
                target.Path = defaults.PathOption;
            }
            else if (!string.IsNullOrWhiteSpace(defaults.Path))
            {
                target.Path = defaults.Path;
            }
        }

        if (!target.MaxDepth.HasValue && defaults.MaxDepth.HasValue)
        {
            target.MaxDepth = defaults.MaxDepth;
        }

        if (!target.MaxWidth.HasValue && defaults.MaxWidth.HasValue)
        {
            target.MaxWidth = defaults.MaxWidth;
        }

        if (!target.MaxNodes.HasValue && defaults.MaxNodes.HasValue)
        {
            target.MaxNodes = defaults.MaxNodes;
        }

        if (!target.UseGitIgnore.HasValue && defaults.UseGitIgnore.HasValue)
        {
            target.UseGitIgnore = defaults.UseGitIgnore;
        }

        if (!target.Format.HasValue && defaults.Format.HasValue)
        {
            target.Format = defaults.Format;
        }

        if (target.FilterRules is null && defaults.FilterRules is not null)
        {
            target.FilterRules = defaults.FilterRules;
        }

        if (string.IsNullOrWhiteSpace(target.FilterFile) && !string.IsNullOrWhiteSpace(defaults.FilterFile))
        {
            target.FilterFile = defaults.FilterFile;
        }

        if (string.IsNullOrWhiteSpace(target.GlobalFilterFile) && !string.IsNullOrWhiteSpace(defaults.GlobalFilterFile))
        {
            target.GlobalFilterFile = defaults.GlobalFilterFile;
        }

        if (!target.NoDefaultFilters && defaults.NoDefaultFilters)
        {
            target.NoDefaultFilters = true;
        }

        if (!target.NoAppGlobalIgnore && defaults.NoAppGlobalIgnore)
        {
            target.NoAppGlobalIgnore = true;
        }

        if (!target.NoLocalFilters && defaults.NoLocalFilters)
        {
            target.NoLocalFilters = true;
        }

        if (!target.IgnoreEmptyFolders && defaults.IgnoreEmptyFolders)
        {
            target.IgnoreEmptyFolders = true;
        }

        if (!target.SkipHidden.HasValue && defaults.SkipHidden.HasValue)
        {
            target.SkipHidden = defaults.SkipHidden;
        }

        if (!target.HighlightHiddenFiles.HasValue && defaults.HighlightHiddenFiles.HasValue)
        {
            target.HighlightHiddenFiles = defaults.HighlightHiddenFiles;
        }

        if (!target.HiddenStyle.HasValue && defaults.HiddenStyle.HasValue)
        {
            target.HiddenStyle = defaults.HiddenStyle;
        }

        if (target.IncludeExtensions is null && defaults.IncludeExtensions is not null)
        {
            target.IncludeExtensions = defaults.IncludeExtensions;
        }

        if (target.ExcludeExtensions is null && defaults.ExcludeExtensions is not null)
        {
            target.ExcludeExtensions = defaults.ExcludeExtensions;
        }

        if (target.IncludeNames is null && defaults.IncludeNames is not null)
        {
            target.IncludeNames = defaults.IncludeNames;
        }

        if (target.ExcludeNames is null && defaults.ExcludeNames is not null)
        {
            target.ExcludeNames = defaults.ExcludeNames;
        }

        if (!target.Wait && defaults.Wait)
        {
            target.Wait = true;
        }
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
