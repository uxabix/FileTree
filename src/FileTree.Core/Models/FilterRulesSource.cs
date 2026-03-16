namespace FileTree.Core.Models;

/// <summary>
/// Defines the sources from which filtering rules should be loaded.
/// Rules are applied in order: Global config -> Local config -> Inline rules,
/// with later rules taking precedence over earlier ones.
/// </summary>
public class FilterRulesSource
{
    /// <summary>
    /// Path to the app-level global filter configuration file (e.g., FileTree.ignore in the app directory).
    /// If null, no app-level global config will be loaded.
    /// </summary>
    public string? AppGlobalConfigPath { get; set; }

    /// <summary>
    /// Whether to load the app-level global configuration file.
    /// </summary>
    public bool UseAppGlobalConfig { get; set; } = true;

    /// <summary>
    /// Filtering rules provided directly (e.g., from CLI arguments).
    /// These rules use gitignore-style syntax.
    /// </summary>
    public List<string> InlineRules { get; set; } = new();

    /// <summary>
    /// Path to a global filter configuration file.
    /// If null, only the default global config will be used (if enabled).
    /// </summary>
    public string? GlobalConfigPath { get; set; }

    /// <summary>
    /// Path to a local filter configuration file (e.g., .filetreeignore in the working directory).
    /// If null, no local config file will be loaded.
    /// </summary>
    public string? LocalConfigPath { get; set; }

    /// <summary>
    /// Whether to load the default global configuration file from the user's home directory.
    /// Default location: ~/.filetreeignore (Linux/Mac) or %USERPROFILE%\.filetreeignore (Windows)
    /// </summary>
    public bool UseDefaultGlobalConfig { get; set; } = true;
}
