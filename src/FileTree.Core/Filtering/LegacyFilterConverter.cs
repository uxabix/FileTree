using System;
using System.Collections.Generic;
using System.Linq;
using FileTree.Core.Models;

namespace FileTree.Core.Filtering;

/// <summary>
/// Converts legacy FilterOptions properties to gitignore-style filtering rules.
/// This ensures backward compatibility with code using the old filtering API.
/// </summary>
internal static class LegacyFilterConverter
{
    /// <summary>
    /// Converts legacy filter options to gitignore-style rules.
    /// </summary>
    /// <param name="options">The FilterOptions containing legacy filter properties.</param>
    /// <returns>A list of gitignore-style rule strings.</returns>
    /// <remarks>
    /// Conversion rules:
    /// - ExcludeExtensions: Converts to "**/*{ext}" patterns
    /// - ExcludeNames: Converts to "{name}" patterns
    /// - IncludeExtensions: Converts to "*" (exclude all) followed by "!**/*{ext}" (except these)
    /// - IncludeNames: Converts to "!{name}" (negation patterns)
    ///
    /// Note: The order matters in gitignore. Include rules must come after exclude rules
    /// to properly negate them.
    /// </remarks>
    public static List<string> ConvertToGitIgnoreRules(FilterOptions options)
    {
        if (options == null)
            throw new ArgumentNullException(nameof(options));

        var rules = new List<string>();

        // Step 1: Handle IncludeExtensions
        // If IncludeExtensions is specified, we need to exclude everything first,
        // then include only the specified extensions
        if (options.IncludeExtensions.Any())
        {
            // Exclude all files
            rules.Add("*");

            // Include specified extensions using negation patterns
            foreach (var ext in options.IncludeExtensions)
            {
                string normalizedExt = NormalizeExtension(ext);
                rules.Add($"!**/*{normalizedExt}");
            }
        }

        // Step 2: Add exclude extensions
        // These exclude specific extensions (can override IncludeExtensions if needed)
        foreach (var ext in options.ExcludeExtensions)
        {
            string normalizedExt = NormalizeExtension(ext);
            rules.Add($"**/*{normalizedExt}");
        }

        // Step 3: Add exclude names
        // These patterns match both files and directories with the specified names
        foreach (var name in options.ExcludeNames)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                // Use ** pattern to match at any directory level
                rules.Add($"**/{name}");
            }
        }

        // Step 4: Add include names (negation patterns)
        // These override any previous exclusions for specific names
        foreach (var name in options.IncludeNames)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                // Negation pattern to force inclusion
                rules.Add($"!**/{name}");
            }
        }

        return rules;
    }

    /// <summary>
    /// Checks if the legacy filter properties contain any filtering rules.
    /// </summary>
    /// <param name="options">The FilterOptions to check.</param>
    /// <returns>True if any legacy filter properties are configured, false otherwise.</returns>
    public static bool HasLegacyFilters(FilterOptions options)
    {
        if (options == null)
            return false;

        return options.IncludeExtensions.Any() ||
               options.ExcludeExtensions.Any() ||
               options.IncludeNames.Any() ||
               options.ExcludeNames.Any();
    }

    /// <summary>
    /// Normalizes a file extension by ensuring it starts with a dot.
    /// </summary>
    /// <param name="extension">The extension to normalize (e.g., "txt" or ".txt").</param>
    /// <returns>Normalized extension starting with a dot (e.g., ".txt").</returns>
    private static string NormalizeExtension(string extension)
    {
        if (string.IsNullOrWhiteSpace(extension))
            return extension;

        extension = extension.Trim();

        // Add leading dot if missing
        if (!extension.StartsWith(".", StringComparison.Ordinal))
            return "." + extension;

        return extension;
    }
}
