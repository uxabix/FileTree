using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using FileTree.Core.Models;

namespace FileTree.Core.Filtering;

/// <summary>
/// Loads filtering rules from multiple sources and combines them into a single ordered list.
/// Rules are applied in order of precedence: App global -> Default global -> Custom global -> Local config -> Inline rules.
/// </summary>
/// <summary>
/// Loads filter rules from various sources (app/global/local configs, inline) in precedence order.
/// Parses .filetreeignore files, combines into ordered list for gitignore matching.
/// Cross-platform home dir detection.
/// </summary>
internal class FilterRulesLoader
{
    private const string DefaultGlobalConfigFileName = ".filetreeignore";

    /// <summary>
    /// Loads and combines filtering rules from all configured sources.
    /// </summary>
    /// <param name="source">Configuration specifying which rule sources to load.</param>
    /// <returns>A list of combined rules in the correct order.</returns>
    /// <summary>Loads rules from all sources in order, adding to list.</summary>
    public List<string> LoadFilterRules(FilterRulesSource source)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));

        var rules = new List<string>();

        // Load rules in order of precedence (lowest to highest)
        // Later rules can override earlier ones using negation patterns (!)

        // 1. Load app-level global configuration (e.g., FileTree.ignore near the executable)
        if (source.UseAppGlobalConfig && !string.IsNullOrWhiteSpace(source.AppGlobalConfigPath))
        {
            if (File.Exists(source.AppGlobalConfigPath))
            {
                LoadRulesFromFile(rules, source.AppGlobalConfigPath);
            }
        }

        // 2. Load default global configuration from user's home directory
        if (source.UseDefaultGlobalConfig)
        {
            string? defaultGlobalPath = GetDefaultGlobalConfigPath();
            if (defaultGlobalPath != null && File.Exists(defaultGlobalPath))
            {
                LoadRulesFromFile(rules, defaultGlobalPath);
            }
        }

        // 3. Load custom global configuration if specified
        if (!string.IsNullOrWhiteSpace(source.GlobalConfigPath))
        {
            LoadRulesFromFile(rules, source.GlobalConfigPath);
        }

        // 4. Load local configuration file if specified
        if (!string.IsNullOrWhiteSpace(source.LocalConfigPath))
        {
            LoadRulesFromFile(rules, source.LocalConfigPath);
        }

        // 5. Add inline rules (highest precedence)
        if (source.InlineRules.Any())
        {
            var inline = source.InlineRules
                .Where(rule => !string.IsNullOrWhiteSpace(rule))
                .Select(rule => rule.Trim())
                .ToList();
            if (inline.Count > 0)
            {
                rules.AddRange(inline);
            }
        }

        return rules;
    }

    /// <summary>
    /// Gets the default global configuration file path based on the operating system.
    /// </summary>
    /// <returns>Path to default global config, or null if home directory cannot be determined.</returns>
    private string? GetDefaultGlobalConfigPath()
    {
        string? homeDirectory = GetUserHomeDirectory();
        if (homeDirectory == null)
            return null;

        return Path.Combine(homeDirectory, DefaultGlobalConfigFileName);
    }

    /// <summary>
    /// Gets the user's home directory path in a cross-platform manner.
    /// </summary>
    /// <returns>User's home directory path, or null if it cannot be determined.</returns>
    private string? GetUserHomeDirectory()
    {
        // Try environment variables first (more reliable)
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return Environment.GetEnvironmentVariable("USERPROFILE");
        }
        else
        {
            // Linux/Mac
            return Environment.GetEnvironmentVariable("HOME");
        }
    }

    /// <summary>
    /// Loads rules from a file and adds them to the provided GitIgnoreRules instance.
    /// </summary>
    /// <param name="rules">The rules list to add rules to.</param>
    /// <param name="filePath">Path to the file containing rules.</param>
    /// <exception cref="FileNotFoundException">Thrown if the specified file does not exist.</exception>
    /// <exception cref="IOException">Thrown if an error occurs while reading the file.</exception>
    private void LoadRulesFromFile(List<string> rules, string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Filter rules file not found: {filePath}", filePath);

        try
        {
            var lines = File.ReadAllLines(filePath);
            var cleanedLines = lines
                .Select(l => l?.Trim())
                .Where(l => !string.IsNullOrEmpty(l) && !l!.StartsWith("#", StringComparison.Ordinal))
                .ToArray();

            if (cleanedLines.Length > 0)
                rules.AddRange(cleanedLines);
        }
        catch (Exception ex) when (ex is not FileNotFoundException)
        {
            throw new IOException($"Error reading filter rules from file: {filePath}", ex);
        }
    }
}
