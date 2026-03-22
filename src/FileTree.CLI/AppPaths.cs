using System.Diagnostics;

namespace FileTree.CLI;

/// <summary>
/// Utility for determining FileTree executable and global configuration file paths.
/// Handles creation/reset/deletion/opening of FileTree.ignore.txt (filters) and FileTree.settings.txt (CLI defaults).
/// All paths relative to executable directory.
/// </summary>
internal static class AppPaths
{
    /// <summary>Filename for global gitignore-style filter rules near executable.</summary>
    internal const string GlobalIgnoreFileName = "FileTree.ignore.txt";
    /// <summary>Filename for default CLI options/settings near executable.</summary>
    internal const string GlobalSettingsFileName = "FileTree.settings.txt";
    private const string DefaultGlobalIgnoreContents =
        "# FileTree global ignore rules\n" +
        "# Add gitignore-style patterns here.\n";
    private const string DefaultGlobalSettingsContents =
        "# FileTree default settings\n" +
        "# Specify CLI options here (one per line or space-separated).\n" +
        "# Examples:\n" +
        "# --max-depth 2\n" +
        "# --format Markdown\n" +
        "# --use-gitignore false\n";

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

    /// <summary>
    /// Gets directory containing the FileTree executable.
    /// Prefers AppContext.BaseDirectory, falls back to parent of exe path.
    /// </summary>
    /// <returns>Normalized full directory path.</returns>
    internal static string GetExecutableDirectory()
    {
        if (!string.IsNullOrWhiteSpace(AppContext.BaseDirectory))
        {
            return Path.GetFullPath(AppContext.BaseDirectory);
        }

        return Path.GetDirectoryName(GetExecutablePath())!;
    }

    /// <summary>Gets path to global FileTree.ignore.txt next to executable.</summary>
    /// <returns>Full path (dir may not exist).</returns>
    internal static string GetGlobalIgnorePath()
    {
        return Path.Combine(GetExecutableDirectory(), GlobalIgnoreFileName);
    }

    /// <summary>Gets path to global FileTree.settings.txt next to executable.</summary>
    /// <returns>Full path (dir may not exist).</returns>
    internal static string GetGlobalSettingsPath()
    {
        return Path.Combine(GetExecutableDirectory(), GlobalSettingsFileName);
    }

    /// <summary>
    /// Ensures FileTree.ignore.txt exists (creates with defaults if missing).
    /// </summary>
    /// <param name="path">Output: full file path.</param>
    /// <param name="error">Output: null on success, else exception message.</param>
    /// <returns>true if exists or created, false on error.</returns>
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

            File.WriteAllText(path, DefaultGlobalIgnoreContents);
            return true;
        }
        catch (Exception ex)
        {
            error = ex.Message;
            return false;
        }
    }

    /// <summary>Overwrites FileTree.ignore.txt with default content.</summary>
    /// <param name="path">Output: full file path.</param>
    /// <param name="error">Output: null on success, else exception message.</param>
    /// <returns>true if written, false on error (file/dir perms).</returns>
    internal static bool TryResetGlobalIgnoreFile(out string path, out string? error)
    {
        path = GetGlobalIgnorePath();
        error = null;

        try
        {
            File.WriteAllText(path, DefaultGlobalIgnoreContents);
            return true;
        }
        catch (Exception ex)
        {
            error = ex.Message;
            return false;
        }
    }

    /// <summary>Ensures FileTree.settings.txt exists (creates with defaults if missing).</summary>
    /// <param name="path">Output: full file path.</param>
    /// <param name="error">Output: null on success, else exception message.</param>
    /// <returns>true if exists or created, false on error.</returns>
    internal static bool TryEnsureGlobalSettingsFileExists(out string path, out string? error)
    {
        path = GetGlobalSettingsPath();
        error = null;

        try
        {
            if (File.Exists(path))
            {
                return true;
            }

            File.WriteAllText(path, DefaultGlobalSettingsContents);
            return true;
        }
        catch (Exception ex)
        {
            error = ex.Message;
            return false;
        }
    }

    /// <summary>Overwrites FileTree.settings.txt with default content.</summary>
    /// <param name="path">Output: full file path.</param>
    /// <param name="error">Output: null on success, else exception message.</param>
    /// <returns>true if written, false on error (perms).</returns>
    internal static bool TryResetGlobalSettingsFile(out string path, out string? error)
    {
        path = GetGlobalSettingsPath();
        error = null;

        try
        {
            File.WriteAllText(path, DefaultGlobalSettingsContents);
            return true;
        }
        catch (Exception ex)
        {
            error = ex.Message;
            return false;
        }
    }

    /// <summary>Deletes FileTree.ignore.txt if exists.</summary>
    /// <param name="path">Output: attempted file path.</param>
    /// <param name="error">Output: null on success, else exception.</param>
    /// <returns>true if absent or deleted, false on error.</returns>
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

    /// <summary>Deletes FileTree.settings.txt if exists.</summary>
    /// <param name="path">Output: attempted file path.</param>
    /// <param name="error">Output: null on success, else exception.</param>
    /// <returns>true if absent or deleted, false on error.</returns>
    internal static bool TryDeleteGlobalSettingsFile(out string path, out string? error)
    {
        path = GetGlobalSettingsPath();
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

    /// <summary>Opens FileTree.settings.txt in default editor (creates if missing).</summary>
    /// <param name="path">Output: full file path.</param>
    /// <param name="error">Output: null on success/open, else exception.</param>
    /// <returns>true if opened (or created+opened), false on error.</returns>
    internal static bool TryOpenGlobalSettingsFile(out string path, out string? error)
    {
        path = GetGlobalSettingsPath();
        error = null;

        try
        {
            if (!File.Exists(path))
            {
                File.WriteAllText(path, DefaultGlobalSettingsContents);
            }

            var startInfo = new ProcessStartInfo
            {
                FileName = path,
                UseShellExecute = true,
            };

            Process.Start(startInfo);
            return true;
        }
        catch (Exception ex)
        {
            error = ex.Message;
            return false;
        }
    }

    /// <summary>Opens FileTree.ignore.txt in default editor (creates if missing).</summary>
    /// <param name="path">Output: full file path.</param>
    /// <param name="error">Output: null on success/open, else exception.</param>
    /// <returns>true if opened (or created+opened), false on error.</returns>
    internal static bool TryOpenGlobalIgnoreFile(out string path, out string? error)
    {
        path = GetGlobalIgnorePath();
        error = null;

        try
        {
            if (!File.Exists(path))
            {
                File.WriteAllText(path, DefaultGlobalIgnoreContents);
            }

            var startInfo = new ProcessStartInfo
            {
                FileName = path,
                UseShellExecute = true,
            };

            Process.Start(startInfo);
            return true;
        }
        catch (Exception ex)
        {
            error = ex.Message;
            return false;
        }
    }
}
