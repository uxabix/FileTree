using System.Text;
using CommandLine;
using FileTree.Core.Services;
using FileTree.Core.Utilities;
using FileTree.CLI.SystemIntegrator;
using TextCopy;

namespace FileTree.CLI;

/// <summary>
/// Main entry point for FileTree CLI application.
/// Handles argument parsing, global options (--pause-exit), config commands (config rules/settings), scan/install/uninstall.
/// Uses CommandLineParser, supports interactive mode, settings from FileTree.settings.txt, platform install via SystemIntegrator.
/// </summary>
internal class Program
{
    private const string PauseExitLongOption = "pause-exit";
    private const char PauseExitShortOption = 'k';

    /// <summary>
    /// Entry point. Processes args, handles globals/config/help, dispatches to scan/install/uninstall/paths.
    /// Applies settings, normalizes booleans, pauses if --pause-exit/&apos;-k&apos;.
    /// </summary>
    /// <param name="args">CLI arguments.</param>
    /// <returns>0 success, 1 error/parse fail.</returns>
    private static int Main(string[] args)
    {
        var argsList = args.ToList();
        var pauseOnExit = ConsumeGlobalBooleanOption(argsList, PauseExitLongOption, PauseExitShortOption);
        var effectiveArgs = argsList.ToArray();

        EnsureGlobalIgnoreFileOnStartup(effectiveArgs);
        EnsureSettingsFileOnStartup(effectiveArgs);

        if (TryHandleConfigCommand(effectiveArgs, out var configExitCode))
        {
            PauseIfNeeded(pauseOnExit);
            return configExitCode;
        }

        if (IsHelpRequest(effectiveArgs))
        {
            var helpParser = new Parser(cfg => cfg.HelpWriter = Console.Out);
            helpParser.ParseArguments<ScanCommandOptions, InstallCommandOptions, UninstallCommandOptions, UninstallDeepCommandOptions, PathsCommandOptions>(effectiveArgs);
            PauseIfNeeded(pauseOnExit);
            return 0;
        }

        var parser = new Parser(cfg =>
        {
            cfg.HelpWriter = null;
            cfg.AutoHelp = false;
        });

        var normalizedArgs = NormalizeBooleanFlags(effectiveArgs);

        int res = parser
            .ParseArguments<ScanCommandOptions, InstallCommandOptions, UninstallCommandOptions, UninstallDeepCommandOptions, PathsCommandOptions>(normalizedArgs)
            .MapResult(
                (ScanCommandOptions opts) => RunScan(opts),
                (InstallCommandOptions _) => RunInstallAsync().GetAwaiter().GetResult(),
                (UninstallCommandOptions _) => RunUninstallAsync().GetAwaiter().GetResult(),
                (UninstallDeepCommandOptions _) => RunUninstallDeepAsync().GetAwaiter().GetResult(),
                (PathsCommandOptions _) => RunPaths(),
                _ => 1);
        PauseIfNeeded(pauseOnExit);

        return res;
    }

    /// <summary>Orchestrates scan: CLI to state, apply defaults, dispatch interactive/once.</summary>
    /// <param name="opts">Parsed scan options.</param>
    /// <returns>0 success, 1 error.</returns>
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

    /// <summary>Executes single tree scan/print/copy. Handles validation/output/errors.</summary>
    /// <param name="state">Resolved scan state/options.</param>
    /// <returns>0 success, 1 validation error.</returns>
    private static int RunScanOnce(ScanOptionsState state)
    {
        var targetPath = state.GetTargetPath();
        var options = state.ToFileTreeOptions();
        var shouldCopy = state.Copy == true;
        var shouldPrint = state.Silent != true;

        if (shouldPrint)
        {
            Console.WriteLine($"Scanning directory: {targetPath}");
            if (state.ShowOptionsAll == true)
            {
                PrintAllOptions(state, options, targetPath);
            }
            else if (state.ShowOptions == true)
            {
                Console.WriteLine($"Options: MaxDepth={options.MaxDepth}, Format={options.Format}, UseGitIgnore={options.UseGitIgnore}");
            }
        }

        var service = new FileTreeService();
        try
        {
            var output = service.Generate(targetPath, options);
            if (shouldPrint)
            {
                Console.WriteLine(output);
            }

            if (shouldCopy)
            {
                TryCopyToClipboard(output, shouldPrint);
            }

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

    /// <summary>Interactive shell for scan options. Parses input until &apos;show&apos;/&apos;exit&apos;.</summary>
    /// <param name="state">Initial scan state, merged with interactive inputs.</param>
    /// <returns>0 success/exit, 1 error.</returns>
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
               name.Equals("wait", StringComparison.OrdinalIgnoreCase) ||
               name.Equals("copy", StringComparison.OrdinalIgnoreCase) ||
               name.Equals("silent", StringComparison.OrdinalIgnoreCase) ||
               name.Equals("show-options", StringComparison.OrdinalIgnoreCase) ||
               name.Equals("show-options-all", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsShortBooleanOption(string arg)
    {
        if (arg.Length != 2 || arg[0] != '-')
        {
            return false;
        }

        var option = arg[1];
        return option == 'g' || option == 'h' || option == '!' || option == 'c' || option == 's';
    }

    private static bool IsBooleanLiteral(string value)
    {
        return string.Equals(value, "true", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(value, "false", StringComparison.OrdinalIgnoreCase);
    }

    private static void TryCopyToClipboard(string output, bool shouldPrint)
    {
        try
        {
            ClipboardService.SetText(output ?? string.Empty);
            if (shouldPrint)
            {
                Console.WriteLine("[Copied to clipboard]");
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Warning: Could not copy output to clipboard: {ex.Message}");
        }
    }

    private static void PauseIfNeeded(bool pauseOnExit)
    {
        if (!pauseOnExit)
        {
            return;
        }

        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }

    private static bool ConsumeGlobalBooleanOption(List<string> args, string longName, char shortName)
    {
        var value = false;
        for (var i = 0; i < args.Count; i++)
        {
            var arg = args[i];
            var longToken = $"--{longName}";
            var shortToken = $"-{shortName}";

            if (string.Equals(arg, longToken, StringComparison.OrdinalIgnoreCase))
            {
                value = true;
                args.RemoveAt(i);
                i--;
                continue;
            }

            if (arg.StartsWith($"{longToken}=", StringComparison.OrdinalIgnoreCase))
            {
                var raw = arg.Substring(longToken.Length + 1);
                if (IsBooleanLiteral(raw))
                {
                    value = string.Equals(raw, "true", StringComparison.OrdinalIgnoreCase);
                }

                args.RemoveAt(i);
                i--;
                continue;
            }

            if (string.Equals(arg, shortToken, StringComparison.OrdinalIgnoreCase))
            {
                value = true;
                args.RemoveAt(i);
                i--;
            }
        }

        return value;
    }

    private static void PrintAllOptions(ScanOptionsState state, FileTree.Core.Models.FileTreeOptions options, string targetPath)
    {
        Console.WriteLine(
            "Options (all): " +
            $"Path={targetPath}, MaxDepth={options.MaxDepth}, MaxWidth={options.MaxWidth}, MaxNodes={options.MaxNodes}, " +
            $"Format={options.Format}, UseGitIgnore={options.UseGitIgnore}, SkipHidden={options.SkipHidden}, " +
            $"HighlightHidden={options.HighlightHiddenFiles}, HiddenStyle={options.HiddenStyle}, " +
            $"CollapseThreshold={options.CollapseThreshold}, CollapseKeepStart={options.CollapseKeepStart}, " +
            $"CollapseKeepEnd={options.CollapseKeepEnd}, CollapseStyle={options.CollapseStyle}, CollapseFrom={options.CollapseFrom}, " +
            $"IgnoreEmptyFolders={options.Filter.IgnoreEmptyFolders}, UseLocalFilterFiles={options.Filter.UseLocalFilterFiles}, " +
            $"NoDefaultFilters={state.NoDefaultFilters ?? false}, NoAppGlobalIgnore={state.NoAppGlobalIgnore ?? false}, " +
            $"NoLocalFilters={state.NoLocalFilters ?? false}, NoDefaultSettings={state.NoDefaultSettings ?? false}, " +
            $"Copy={state.Copy == true}, Silent={state.Silent == true}, Wait={state.Wait == true}");
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

    /// <summary>Handles &apos;config rules/settings&apos; subcommands (open/reset/path).</summary>
    /// <param name="args">Remaining args after &apos;config&apos;.</param>
    /// <param name="exitCode">Output: 0 success, 1 invalid.</param>
    /// <returns>true if handled, false otherwise.</returns>
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

    /// <summary>Loads defaults from FileTree.settings.txt, parses as CLI args, converts to state.</summary>
    /// <returns>Parsed state or null if missing/unparseable.</returns>
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

    /// <summary>Delegates to platform integrator for install.</summary>
    /// <returns>0 success.</returns>
    private static async Task<int> RunInstallAsync()
    {
        var integrator = SystemIntegratorFactory.Create();
        await integrator.InstallAsync();
        return 0;
    }

    /// <summary>Delegates to platform integrator for user uninstall.</summary>
    /// <returns>0 success.</returns>
    private static async Task<int> RunUninstallAsync()
    {
        var integrator = SystemIntegratorFactory.Create();
        await integrator.UninstallAsync();
        return 0;
    }

    /// <summary>Delegates to platform integrator for deep uninstall.</summary>
    /// <returns>0 success.</returns>
    private static async Task<int> RunUninstallDeepAsync()
    {
        var integrator = SystemIntegratorFactory.Create();
        await integrator.UninstallDeepAsync();
        return 0;
    }
}
