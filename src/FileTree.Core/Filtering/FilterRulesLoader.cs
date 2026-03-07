using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using FileTree.Core.GitIgnore;
using FileTree.Core.Models;

namespace FileTree.Core.Filtering;

/// <summary>
/// Loads filtering rules from multiple sources and combines them into a single GitIgnoreRules instance.
/// Rules are applied in order of precedence: Global config -> Local config -> Inline rules.
/// </summary>
internal class FilterRulesLoader
{
    private const string DefaultGlobalConfigFileName = ".filetreeignore";

    /// <summary>
    /// Loads and combines filtering rules from all configured sources.
    /// </summary>
    /// <param name="source">Configuration specifying which rule sources to load.</param>
    /// <returns>A GitIgnoreRules instance containing all loaded rules.</returns>
    public GitIgnoreRules LoadFilterRules(FilterRulesSource source)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));

        var rules = new GitIgnoreRules();

        // Load rules in order of precedence (lowest to highest)
        // Later rules can override earlier ones using negation patterns (!)

        // 1. Load default global configuration from user's home directory
        if (source.UseDefaultGlobalConfig)
        {
            string? defaultGlobalPath = GetDefaultGlobalConfigPath();
            if (defaultGlobalPath != null && File.Exists(defaultGlobalPath))
            {
                LoadRulesFromFile(rules, defaultGlobalPath);
            }
        }

        // 2. Load custom global configuration if specified
        if (!string.IsNullOrWhiteSpace(source.GlobalConfigPath))
        {
            LoadRulesFromFile(rules, source.GlobalConfigPath);
        }

        // 3. Load local configuration file if specified
        if (!string.IsNullOrWhiteSpace(source.LocalConfigPath))
        {
            LoadRulesFromFile(rules, source.LocalConfigPath);
        }

        // 4. Add inline rules (highest precedence)
        if (source.InlineRules.Any())
        {
            rules.Add(source.InlineRules);
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
    /// <param name="rules">The GitIgnoreRules instance to add rules to.</param>
    /// <param name="filePath">Path to the file containing rules.</param>
    /// <exception cref="FileNotFoundException">Thrown if the specified file does not exist.</exception>
    /// <exception cref="IOException">Thrown if an error occurs while reading the file.</exception>
    private void LoadRulesFromFile(GitIgnoreRules rules, string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Filter rules file not found: {filePath}", filePath);

        try
        {
            var fileRules = GitIgnoreParser.FromFile(filePath);

            // GitIgnoreParser already creates a new GitIgnoreRules instance,
            // so we need to extract and add the rules to our existing instance.
            // We do this by reading the file again and adding the lines directly.
            var lines = File.ReadAllLines(filePath);
            var cleanedLines = lines
                .Select(l => l?.Trim())
                .Where(l => !string.IsNullOrEmpty(l) && !l!.StartsWith("#", StringComparison.Ordinal))
                .ToArray();

            if (cleanedLines.Length > 0)
                rules.Add(cleanedLines);
        }
        catch (Exception ex) when (ex is not FileNotFoundException)
        {
            throw new IOException($"Error reading filter rules from file: {filePath}", ex);
        }
    }
}
