using System.Diagnostics;

namespace FileTree.CLI;

internal static class AppPaths
{
    internal const string GlobalIgnoreFileName = "FileTree.ignore";

    internal static string GetExecutablePath()
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

    internal static string GetExecutableDirectory()
    {
        if (!string.IsNullOrWhiteSpace(AppContext.BaseDirectory))
        {
            return Path.GetFullPath(AppContext.BaseDirectory);
        }

        return Path.GetDirectoryName(GetExecutablePath())!;
    }

    internal static string GetGlobalIgnorePath()
    {
        return Path.Combine(GetExecutableDirectory(), GlobalIgnoreFileName);
    }

    internal static bool TryEnsureGlobalIgnoreFileExists(out string path, out string? error)
    {
        path = GetGlobalIgnorePath();
        error = null;

        try
        {
            if (File.Exists(path))
            {
                return true;
            }

            var contents =
                "# FileTree global ignore rules\n" +
                "# Add gitignore-style patterns here.\n";

            File.WriteAllText(path, contents);
            return true;
        }
        catch (Exception ex)
        {
            error = ex.Message;
            return false;
        }
    }

    internal static bool TryDeleteGlobalIgnoreFile(out string path, out string? error)
    {
        path = GetGlobalIgnorePath();
        error = null;

        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            return true;
        }
        catch (Exception ex)
        {
            error = ex.Message;
            return false;
        }
    }
}
