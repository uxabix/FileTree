using System.Runtime.Versioning;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using FileTree.CLI;

namespace FileTree.CLI.SystemIntegrator;

[SupportedOSPlatform("windows")]
/// <summary>
/// Windows-specific implementation: adds to user PATH, BAT/PowerShell shims/aliases, registry context menus (dirs/files/desktop/background, default/custom).
/// Deep uninstall prompts/scans for entries. Broadcasts env changes.
/// </summary>
internal sealed class WindowsSystemIntegrator : ISystemIntegrator
{
    private const string DirectoryMenuKey =
        @"Software\Classes\Directory\shell\FileTree";

    private const string FileMenuKey =
        @"Software\Classes\*\shell\FileTree";

    private const string DesktopMenuKey =
        @"Software\Classes\DesktopBackground\shell\FileTree";

    private const string DirectoryBackgroundMenuKey =
        @"Software\Classes\Directory\Background\shell\FileTree";

    private const string DirectoryMenuKeyCustom =
        @"Software\Classes\Directory\shell\FileTreeCustom";

    private const string FileMenuKeyCustom =
        @"Software\Classes\*\shell\FileTreeCustom";

    private const string DesktopMenuKeyCustom =
        @"Software\Classes\DesktopBackground\shell\FileTreeCustom";

    private const string DirectoryBackgroundMenuKeyCustom =
        @"Software\Classes\Directory\Background\shell\FileTreeCustom";

    /// <inheritdoc />
    public Task InstallAsync()
    {
        var exePath = GetExecutablePath();
        var exeDirectory = Path.GetDirectoryName(exePath)!;
        var ignorePath = AppPaths.GetGlobalIgnorePath();
        var settingsPath = AppPaths.GetGlobalSettingsPath();

        var alreadyInPath = IsDirectoryInUserPath(exeDirectory);
        var contextMenuExists = ContextMenuExists();

        if (alreadyInPath && contextMenuExists)
        {
            Console.WriteLine("FileTree is already installed for the current user.");
            return Task.CompletedTask;
        }

        if (!File.Exists(ignorePath))
        {
            if (AppPaths.TryEnsureGlobalIgnoreFileExists(out _, out var error))
            {
                Console.WriteLine("+ Created FileTree.ignore");
            }
            else
            {
                Console.WriteLine($"! Failed to create FileTree.ignore: {error}");
            }
        }

        if (!File.Exists(settingsPath))
        {
            if (AppPaths.TryEnsureGlobalSettingsFileExists(out _, out var error))
            {
                Console.WriteLine("+ Created FileTree.Settings");
            }
            else
            {
                Console.WriteLine($"! Failed to create FileTree.Settings: {error}");
            }
        }

        if (!alreadyInPath)
        {
            AddDirectoryToUserPath(exeDirectory);
            CreateCommandShims(exePath);
            CreatePowerShellAlias(exePath);
            Console.WriteLine("+Added to user PATH");
        }

        if (!contextMenuExists)
        {
            AddContextMenu(exePath);
            Console.WriteLine("+Added to directory context menu");
        }

        BroadcastEnvironmentChange();

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task UninstallAsync()
    {
        var exePath = GetExecutablePath();
        var exeDirectory = Path.GetDirectoryName(exePath)!;
        var ignorePath = AppPaths.GetGlobalIgnorePath();
        var settingsPath = AppPaths.GetGlobalSettingsPath();

        var removedShims = RemoveCommandShims(exePath);
        if (removedShims)
            Console.WriteLine("+ Removed command shims (FileTree.bat, FT.bat)");
        var removedAlias = RemovePowerShellAlias();
        if (removedAlias)
            Console.WriteLine("+ Removed command aliases");
        var removedFromPath = RemoveDirectoryFromUserPath(exeDirectory);
        var removedContextMenu = RemoveContextMenu();

        if (removedFromPath)
        {
            Console.WriteLine("+ Removed from user PATH");
        }

        if (removedContextMenu)
        {
            Console.WriteLine("+ Removed from directory context menu");
        }

        if (!removedFromPath && !removedContextMenu)
        {
            Console.WriteLine("FileTree does not appear to be installed for the current user.");
        }
        else
        {
            BroadcastEnvironmentChange();
        }

        if (File.Exists(ignorePath))
        {
            if (AppPaths.TryDeleteGlobalIgnoreFile(out _, out var error))
            {
                Console.WriteLine("+ Removed FileTree.ignore");
            }
            else
            {
                Console.WriteLine($"! Failed to remove FileTree.ignore: {error}");
            }
        }

        if (File.Exists(settingsPath))
        {
            if (AppPaths.TryDeleteGlobalSettingsFile(out _, out var error))
            {
                Console.WriteLine("+ Removed FileTree.Settings");
            }
            else
            {
                Console.WriteLine($"! Failed to remove FileTree.Settings: {error}");
            }
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task UninstallDeepAsync()
    {
        var exePath = GetExecutablePath();
        var exeDirectory = Path.GetDirectoryName(exePath)!;
        var ignorePath = AppPaths.GetGlobalIgnorePath();
        var settingsPath = AppPaths.GetGlobalSettingsPath();

        Console.WriteLine("Searching for FileTree entries created by any installation...");

        var removedAny = false;
        removedAny |= PromptAndRemovePathEntries(exeDirectory);
        removedAny |= PromptAndRemoveCommandShims(exePath);
        removedAny |= PromptAndRemovePowerShellAliases(exePath);
        removedAny |= PromptAndRemoveContextMenuEntries(exePath);

        if (removedAny)
        {
            BroadcastEnvironmentChange();
            Console.WriteLine("Uninstall-deep completed.");
        }
        else
        {
            Console.WriteLine("No FileTree entries were removed.");
        }

        if (File.Exists(ignorePath))
        {
            if (AppPaths.TryDeleteGlobalIgnoreFile(out _, out var error))
            {
                Console.WriteLine("+ Removed FileTree.ignore");
            }
            else
            {
                Console.WriteLine($"! Failed to remove FileTree.ignore: {error}");
            }
        }

        if (File.Exists(settingsPath))
        {
            if (AppPaths.TryDeleteGlobalSettingsFile(out _, out var error))
            {
                Console.WriteLine("+ Removed FileTree.Settings");
            }
            else
            {
                Console.WriteLine($"! Failed to remove FileTree.Settings: {error}");
            }
        }

        return Task.CompletedTask;
    }

    private static string GetExecutablePath()
    {
        var path = Environment.ProcessPath;
        if (!string.IsNullOrWhiteSpace(path))
        {
            return path;
        }

        using var current = Process.GetCurrentProcess();
        var modulePath = current.MainModule?.FileName;
        if (string.IsNullOrWhiteSpace(modulePath))
        {
            throw new InvalidOperationException("Unable to determine executable path.");
        }

        return modulePath;
    }

    private static bool IsDirectoryInUserPath(string directory)
    {
        var userPath = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User);
        if (string.IsNullOrEmpty(userPath))
        {
            return false;
        }

        var paths = userPath.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return paths.Any(p =>
            string.Equals(NormalizePath(p), NormalizePath(directory), StringComparison.OrdinalIgnoreCase));
    }

    private static void AddDirectoryToUserPath(string directory)
    {
        var userPath = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User);
        if (string.IsNullOrEmpty(userPath))
        {
            Environment.SetEnvironmentVariable("PATH", directory, EnvironmentVariableTarget.User);
            return;
        }

        var paths = userPath.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();
        if (paths.Any(p =>
                string.Equals(NormalizePath(p), NormalizePath(directory), StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        paths.Add(directory);
        var newPath = string.Join(';', paths);
        Environment.SetEnvironmentVariable("PATH", newPath, EnvironmentVariableTarget.User);
    }

    private static bool RemoveDirectoryFromUserPath(string directory)
    {
        var userPath = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User);
        if (string.IsNullOrEmpty(userPath))
        {
            return false;
        }

        var normalizedTarget = NormalizePath(directory);
        var paths = userPath.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();
        var originalCount = paths.Count;

        paths = paths
            .Where(p => !string.Equals(NormalizePath(p), normalizedTarget, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (paths.Count == originalCount)
        {
            return false;
        }

        var newPath = string.Join(';', paths);
        Environment.SetEnvironmentVariable("PATH", newPath, EnvironmentVariableTarget.User);
        return true;
    }

    private static void CreateCommandShims(string exePath)
    {
        var exeDirectory = Path.GetDirectoryName(exePath)!;
        var fileTreeBat = Path.Combine(exeDirectory, "FileTree.bat");
        var ftBat = Path.Combine(exeDirectory, "FT.bat");

        File.WriteAllText(fileTreeBat, $"@\"{exePath}\" %*");
        File.WriteAllText(ftBat, $"@\"{exePath}\" %*");
    }

    private static bool RemoveCommandShims(string exePath)
    {
        var exeDirectory = Path.GetDirectoryName(exePath)!;
        var fileTreeBat = Path.Combine(exeDirectory, "FileTree.bat");
        var ftBat = Path.Combine(exeDirectory, "FT.bat");

        bool removedAny = false;
        if (File.Exists(fileTreeBat))
        {
            File.Delete(fileTreeBat);
            removedAny = true;
        }

        if (File.Exists(ftBat))
        {
            File.Delete(ftBat);
            removedAny = true;
        }

        return removedAny;
    }

    private static void CreatePowerShellAlias(string exePath)
    {
        var profilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "PowerShell", "Microsoft.PowerShell_profile.ps1");

        Directory.CreateDirectory(Path.GetDirectoryName(profilePath)!);

        var aliasCommands = $@"
            Set-Alias FT ""{exePath}""
            Set-Alias FileTree ""{exePath}""
        ";

        File.AppendAllText(profilePath, aliasCommands);
    }

    private static bool RemovePowerShellAlias()
    {
        var profilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "PowerShell", "Microsoft.PowerShell_profile.ps1");

        if (!File.Exists(profilePath))
            return true;

        var lines = File.ReadAllLines(profilePath)
            .Where(line => !line.Contains("Set-Alias FT") && !line.Contains("Set-Alias FileTree"))
            .ToArray();

        File.WriteAllLines(profilePath, lines);
        return true;
    }

    private static bool ContextMenuExists()
    {
        using var key = Registry.CurrentUser.OpenSubKey(DirectoryMenuKey);
        return key is not null;
    }

    private static void AddContextMenu(string exePath)
    {
        // Call with default options
        AddContextMenuForKey(DirectoryMenuKey, exePath, "\"%1\" --pause-exit");
        AddContextMenuForKey(FileMenuKey, exePath, "\"%1\" --pause-exit");
        AddContextMenuForKey(DesktopMenuKey, exePath, "--pause-exit");
        AddContextMenuForKey(DirectoryBackgroundMenuKey, exePath, "--pause-exit");
        
        // Customizable call
        AddContextMenuForKey(DirectoryMenuKeyCustom, exePath, "\"%1\" --wait --pause-exit", "FileTree Customizable");
        AddContextMenuForKey(FileMenuKeyCustom, exePath, "\"%1\" --wait --pause-exit", "FileTree Customizable");
        AddContextMenuForKey(DesktopMenuKeyCustom, exePath, "--wait --pause-exit", "FileTree Customizable");
        AddContextMenuForKey(DirectoryBackgroundMenuKeyCustom, exePath, "--wait --pause-exit", "FileTree Customizable");
    }
    
    private static void AddContextMenuForKey(
        string keyPath,
        string exePath,
        string argument,
        string menuText="FileTree")
    {
        using var mainKey = Registry.CurrentUser.CreateSubKey(keyPath);

        mainKey.SetValue(string.Empty, menuText);
        mainKey.SetValue("Icon", exePath);

        using var commandKey = mainKey.CreateSubKey("command");

        var command = string.IsNullOrWhiteSpace(argument)
            ? $"\"{exePath}\""
            : $"\"{exePath}\" {argument}";

        commandKey.SetValue(string.Empty, command);
    }

    private static bool RemoveContextMenu()
    {
        bool removedAny = false;

        removedAny |= RemoveContextMenuKey(DirectoryMenuKey);
        removedAny |= RemoveContextMenuKey(FileMenuKey);
        removedAny |= RemoveContextMenuKey(DesktopMenuKey);
        removedAny |= RemoveContextMenuKey(DirectoryBackgroundMenuKey);
        removedAny |= RemoveContextMenuKey(DirectoryMenuKeyCustom);
        removedAny |= RemoveContextMenuKey(FileMenuKeyCustom);
        removedAny |= RemoveContextMenuKey(DesktopMenuKeyCustom);
        removedAny |= RemoveContextMenuKey(DirectoryBackgroundMenuKeyCustom);

        return removedAny;
    }

    private static bool RemoveContextMenuKey(string keyPath)
    {
        try
        {
            var parentPath = Path.GetDirectoryName(keyPath);
            var subKeyName = Path.GetFileName(keyPath);
            if (string.IsNullOrWhiteSpace(parentPath) || string.IsNullOrWhiteSpace(subKeyName))
            {
                return false;
            }

            using var parent = Registry.CurrentUser.OpenSubKey(parentPath, writable: true);

            if (parent == null)
                return false;

            parent.DeleteSubKeyTree(subKeyName, throwOnMissingSubKey: false);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static string NormalizePath(string path)
    {
        return Path.GetFullPath(path.Trim());
    }

    private static bool PromptAndRemovePathEntries(string exeDirectory)
    {
        var userPath = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User);
        if (string.IsNullOrEmpty(userPath))
        {
            return false;
        }

        var segments = userPath.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();
        if (segments.Count == 0)
        {
            return false;
        }

        var updated = new List<string>(segments.Count);
        var removedAny = false;

        foreach (var segment in segments)
        {
            var isCurrent = PathsEqual(segment, exeDirectory);
            if (isCurrent || segment.IndexOf("FileTree", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                var label = OwnershipLabel(isCurrent);
                if (PromptYesNo($"PATH entry: {segment} ({label}) Remove?"))
                {
                    removedAny = true;
                    continue;
                }
            }

            updated.Add(segment);
        }

        if (removedAny)
        {
            Environment.SetEnvironmentVariable("PATH", string.Join(';', updated), EnvironmentVariableTarget.User);
            Console.WriteLine("+ Updated user PATH");
        }

        return removedAny;
    }

    private static bool PromptAndRemoveCommandShims(string exePath)
    {
        var exeDirectory = Path.GetDirectoryName(exePath)!;
        var directories = GetUserPathDirectories();

        if (!directories.Any(d => PathsEqual(d, exeDirectory)))
        {
            directories.Add(exeDirectory);
        }

        var removedAny = false;
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var directory in directories)
        {
            if (!seen.Add(NormalizePath(directory)))
            {
                continue;
            }

            var fileTreeBat = Path.Combine(directory, "FileTree.bat");
            var ftBat = Path.Combine(directory, "FT.bat");

            removedAny |= PromptAndRemoveShim(fileTreeBat, exePath);
            removedAny |= PromptAndRemoveShim(ftBat, exePath);
        }

        return removedAny;
    }

    private static bool PromptAndRemoveShim(string shimPath, string exePath)
    {
        if (!File.Exists(shimPath))
        {
            return false;
        }

        var isCurrent = PathsEqual(Path.GetDirectoryName(shimPath)!, Path.GetDirectoryName(exePath)!);
        try
        {
            var contents = File.ReadAllText(shimPath);
            if (contents.IndexOf(exePath, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                isCurrent = true;
            }
        }
        catch
        {
            // Best-effort; continue.
        }

        var label = OwnershipLabel(isCurrent);
        if (PromptYesNo($"Command shim: {shimPath} ({label}) Remove?"))
        {
            File.Delete(shimPath);
            Console.WriteLine($"+ Removed {Path.GetFileName(shimPath)}");
            return true;
        }

        return false;
    }

    private static bool PromptAndRemovePowerShellAliases(string exePath)
    {
        var profilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "PowerShell", "Microsoft.PowerShell_profile.ps1");

        if (!File.Exists(profilePath))
        {
            return false;
        }

        var lines = File.ReadAllLines(profilePath);
        var updated = new List<string>(lines.Length);
        var removedAny = false;

        foreach (var line in lines)
        {
            var isCandidate =
                line.IndexOf("FileTree", StringComparison.OrdinalIgnoreCase) >= 0 ||
                line.IndexOf("Set-Alias FT", StringComparison.OrdinalIgnoreCase) >= 0;

            if (isCandidate)
            {
                var isCurrent = line.IndexOf(exePath, StringComparison.OrdinalIgnoreCase) >= 0;
                var label = OwnershipLabel(isCurrent);
                var display = line.Trim();
                if (PromptYesNo($"PowerShell profile entry: {display} ({label}) Remove?"))
                {
                    removedAny = true;
                    continue;
                }
            }

            updated.Add(line);
        }

        if (removedAny)
        {
            File.WriteAllLines(profilePath, updated);
            Console.WriteLine("+ Updated PowerShell profile");
        }

        return removedAny;
    }

    private static bool PromptAndRemoveContextMenuEntries(string exePath)
    {
        var removedAny = false;

        foreach (var parentPath in ContextMenuParentKeys)
        {
            using var parentKey = Registry.CurrentUser.OpenSubKey(parentPath, writable: true);
            if (parentKey is null)
            {
                continue;
            }

            foreach (var subKeyName in parentKey.GetSubKeyNames())
            {
                if (subKeyName.IndexOf("FileTree", StringComparison.OrdinalIgnoreCase) < 0)
                {
                    continue;
                }

                string? command = null;
                string? icon = null;
                using (var subKey = parentKey.OpenSubKey(subKeyName))
                {
                    command = subKey?.OpenSubKey("command")?.GetValue(string.Empty) as string;
                    icon = subKey?.GetValue("Icon") as string;
                }
                var isCurrent = ContainsIgnoreCase(command, exePath) || ContainsIgnoreCase(icon, exePath);

                var label = OwnershipLabel(isCurrent);
                var display = $@"HKCU\{parentPath}\{subKeyName}";
                if (PromptYesNo($"Context menu entry: {display} ({label}) Remove?"))
                {
                    parentKey.DeleteSubKeyTree(subKeyName, throwOnMissingSubKey: false);
                    Console.WriteLine($"+ Removed context menu entry {subKeyName}");
                    removedAny = true;
                }
            }
        }

        return removedAny;
    }

    private static List<string> GetUserPathDirectories()
    {
        var userPath = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User);
        if (string.IsNullOrEmpty(userPath))
        {
            return new List<string>();
        }

        return userPath.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();
    }

    private static bool PromptYesNo(string prompt)
    {
        while (true)
        {
            Console.Write($"{prompt} [y/N]: ");
            var input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input))
            {
                return false;
            }

            if (input.Equals("y", StringComparison.OrdinalIgnoreCase) ||
                input.Equals("yes", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (input.Equals("n", StringComparison.OrdinalIgnoreCase) ||
                input.Equals("no", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }
    }

    private static string OwnershipLabel(bool isCurrent)
    {
        return isCurrent ? "current install" : "other install";
    }

    private static bool PathsEqual(string pathA, string pathB)
    {
        return string.Equals(NormalizePath(pathA), NormalizePath(pathB), StringComparison.OrdinalIgnoreCase);
    }

    private static bool ContainsIgnoreCase(string? source, string value)
    {
        return !string.IsNullOrEmpty(source) &&
               source.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private static readonly string[] ContextMenuParentKeys =
    {
        @"Software\Classes\Directory\shell",
        @"Software\Classes\*\shell",
        @"Software\Classes\DesktopBackground\shell",
        @"Software\Classes\Directory\Background\shell",
    };

    private static void BroadcastEnvironmentChange()
    {
        try
        {
            SendMessageTimeout(new IntPtr(HWND_BROADCAST), WM_SETTINGCHANGE, IntPtr.Zero, "Environment",
                SMTO_ABORTIFHUNG, 5000, out _);
        }
        catch
        {
            // Best-effort; ignore failures.
        }
    }

    private const int HWND_BROADCAST = 0xFFFF;
    private const int WM_SETTINGCHANGE = 0x001A;
    private const int SMTO_ABORTIFHUNG = 0x0002;

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern IntPtr SendMessageTimeout(
        IntPtr hWnd,
        uint Msg,
        IntPtr wParam,
        string lParam,
        uint fuFlags,
        uint uTimeout,
        out IntPtr lpdwResult);
}
