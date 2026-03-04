using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace FileTree.CLI;

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

    public Task InstallAsync()
    {
        var exePath = GetExecutablePath();
        var exeDirectory = Path.GetDirectoryName(exePath)!;

        var alreadyInPath = IsDirectoryInUserPath(exeDirectory);
        var contextMenuExists = ContextMenuExists();

        if (alreadyInPath && contextMenuExists)
        {
            Console.WriteLine("FileTree is already installed for the current user.");
            return Task.CompletedTask;
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

    public Task UninstallAsync()
    {
        var exePath = GetExecutablePath();
        var exeDirectory = Path.GetDirectoryName(exePath)!;

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
        AddContextMenuForKey(DirectoryMenuKey, exePath, "\"%1\"");
        AddContextMenuForKey(FileMenuKey, exePath, "\"%1\"");
        AddContextMenuForKey(DesktopMenuKey, exePath, "");
        AddContextMenuForKey(DirectoryBackgroundMenuKey, exePath, "");
    }

    private static void AddContextMenuForKey(string keyPath, string exePath, string argument)
    {
        using var mainKey = Registry.CurrentUser.CreateSubKey(keyPath);
        if (mainKey is null)
            throw new InvalidOperationException($"Failed to create registry key: {keyPath}");

        mainKey.SetValue(string.Empty, "FileTree");
        mainKey.SetValue("Icon", exePath);

        using var commandKey = mainKey.CreateSubKey("command");
        if (commandKey is null)
            throw new InvalidOperationException($"Failed to create command key for: {keyPath}");

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

        return removedAny;
    }

    private static bool RemoveContextMenuKey(string keyPath)
    {
        try
        {
            using var parent = Registry.CurrentUser.OpenSubKey(
                Path.GetDirectoryName(keyPath)!, writable: true);

            if (parent == null)
                return false;

            parent.DeleteSubKeyTree("FileTree", throwOnMissingSubKey: false);
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
