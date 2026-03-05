using System.Diagnostics;

namespace FileTree.CLI.SystemIntegrator;

internal sealed class LinuxSystemIntegrator : ISystemIntegrator
{
    public Task InstallAsync()
    {
        var exePath = GetExecutablePath();
        var exeDirectory = Path.GetDirectoryName(exePath)!;

        var path = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
        var segments = path.Split(':', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var inPath = segments.Any(s => string.Equals(NormalizePath(s), NormalizePath(exeDirectory), StringComparison.Ordinal));

        if (inPath)
        {
            Console.WriteLine("FileTree CLI is already available in your PATH.");
            return Task.CompletedTask;
        }

        Console.WriteLine("FileTree CLI is not in your PATH.");
        Console.WriteLine();
        Console.WriteLine("You can add it for the current shell session with:");
        Console.WriteLine($"  export PATH=\"$PATH:{exeDirectory}\"");
        Console.WriteLine();
        Console.WriteLine("Or copy the binary to ~/.local/bin (recommended for user-level installs):");
        Console.WriteLine($"  mkdir -p ~/.local/bin");
        Console.WriteLine($"  cp \"{exePath}\" ~/.local/bin/filetree");
        Console.WriteLine("  chmod +x ~/.local/bin/filetree");
        Console.WriteLine();
        Console.WriteLine("No system-wide changes were made automatically.");

        return Task.CompletedTask;
    }

    public Task UninstallAsync()
    {
        Console.WriteLine("Uninstall on Linux does not modify your system automatically.");
        Console.WriteLine("If you added FileTree to PATH or copied it to ~/.local/bin, please remove those changes manually.");
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

    private static string NormalizePath(string path)
    {
        return Path.GetFullPath(path.Trim());
    }
}

