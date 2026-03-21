using System.Text;
using CommandLine;
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

        if (IsHelpRequest(args))
        {
            var helpParser = new Parser(cfg => cfg.HelpWriter = Console.Out);
            helpParser.ParseArguments<ScanCommandOptions, InstallCommandOptions, UninstallCommandOptions, UninstallDeepCommandOptions, PathsCommandOptions>(args);
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
            return 0;
        }

        var parser = new Parser(cfg =>
        {
            cfg.HelpWriter = null;
            cfg.AutoHelp = false;
        });

        var normalizedArgs = NormalizeBooleanFlags(args);

        int res = parser
            .ParseArguments<ScanCommandOptions, InstallCommandOptions, UninstallCommandOptions, UninstallDeepCommandOptions, PathsCommandOptions>(normalizedArgs)
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
        var state = ScanOptionsState.FromCli(opts);
        ApplySettingsDefaultsIfNeeded(state);

        if (state.Wait == true)
        {
            return RunScanInteractive(state);
        }

        return RunScanOnce(state);
    }

    private static int RunScanOnce(ScanOptionsState state)
    {
        var targetPath = state.GetTargetPath();
        var options = state.ToFileTreeOptions();

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

    private static int RunScanInteractive(ScanOptionsState state)
    {
        var current = state;

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

            args = NormalizeBooleanFlags(args);

            var parser = new Parser(cfg =>
            {
                cfg.HelpWriter = null;
                cfg.AutoHelp = false;
            });
            var result = parser.ParseArguments<ScanCommandOptions>(args);

            result
                .WithParsed(newOpts =>
                {
                    var newState = ScanOptionsState.FromCli(newOpts);
                    current.MergeFrom(newState);
                    Console.WriteLine("Updated options.");
                })
                .WithNotParsed(_ =>
                {
                    Console.WriteLine("Could not parse input. Please enter valid options or 'show'/'exit'.");
                });
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

    private static bool IsHelpRequest(string[] args)
    {
        return args.Any(arg =>
            string.Equals(arg, "--help", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(arg, "-?", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(arg, "/?", StringComparison.OrdinalIgnoreCase));
    }

    private static string[] NormalizeBooleanFlags(string[] args)
    {
        if (args.Length == 0)
        {
            return args;
        }

        var normalized = new List<string>(args.Length);

        for (var i = 0; i < args.Length; i++)
        {
            var arg = args[i];

            if (arg.StartsWith("--", StringComparison.Ordinal))
            {
                var eqIndex = arg.IndexOf('=');
                var name = eqIndex > 2 ? arg.Substring(2, eqIndex - 2) : arg.Substring(2);

                if (IsBooleanLongOption(name))
                {
                    if (eqIndex >= 0)
                    {
                        normalized.Add(arg);
                        continue;
                    }

                    if (i + 1 < args.Length && IsBooleanLiteral(args[i + 1]))
                    {
                        normalized.Add($"{arg}={args[i + 1]}");
                        i++;
                        continue;
                    }

                    normalized.Add($"{arg}=true");
                    continue;
                }
            }

            if (IsShortBooleanOption(arg))
            {
                if (i + 1 < args.Length && IsBooleanLiteral(args[i + 1]))
                {
                    normalized.Add(arg);
                    normalized.Add(args[i + 1]);
                    i++;
                    continue;
                }

                normalized.Add(arg);
                normalized.Add("true");
                continue;
            }

            normalized.Add(arg);
        }

        return normalized.ToArray();
    }

    private static bool IsBooleanLongOption(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return false;
        }

        return name.Equals("use-gitignore", StringComparison.OrdinalIgnoreCase) ||
               name.Equals("no-default-filters", StringComparison.OrdinalIgnoreCase) ||
               name.Equals("no-app-global-ignore", StringComparison.OrdinalIgnoreCase) ||
               name.Equals("no-local-filters", StringComparison.OrdinalIgnoreCase) ||
               name.Equals("no-default-settings", StringComparison.OrdinalIgnoreCase) ||
               name.Equals("ignore-empty", StringComparison.OrdinalIgnoreCase) ||
               name.Equals("hidden", StringComparison.OrdinalIgnoreCase) ||
               name.Equals("highlight-hidden", StringComparison.OrdinalIgnoreCase) ||
               name.Equals("wait", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsShortBooleanOption(string arg)
    {
        if (arg.Length != 2 || arg[0] != '-')
        {
            return false;
        }

        var option = arg[1];
        return option == 'g' || option == 'h' || option == '!';
    }

    private static bool IsBooleanLiteral(string value)
    {
        return string.Equals(value, "true", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(value, "false", StringComparison.OrdinalIgnoreCase);
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

    private static void ApplySettingsDefaultsIfNeeded(ScanOptionsState state)
    {
        if (state.NoDefaultSettings == true)
        {
            return;
        }

        var defaults = LoadSettingsDefaults();
        if (defaults == null)
        {
            return;
        }

        state.ApplyDefaults(defaults);
    }

    private static ScanOptionsState? LoadSettingsDefaults()
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

            args = NormalizeBooleanFlags(args.ToArray()).ToList();

            if (string.Equals(args[0], "scan", StringComparison.OrdinalIgnoreCase))
            {
                args.RemoveAt(0);
            }

            var parser = new Parser(cfg =>
            {
                cfg.HelpWriter = null;
                cfg.AutoHelp = false;
            });
            ScanCommandOptions? settings = null;
            var result = parser.ParseArguments<ScanCommandOptions>(args);

            result
                .WithParsed(parsed => settings = parsed)
                .WithNotParsed(_ => settings = null);

            return settings == null ? null : ScanOptionsState.FromCli(settings);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Warning: Could not read settings file '{path}': {ex.Message}");
            return null;
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
