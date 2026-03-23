using System;
using FileTree.Core.Models;

namespace FileTree.Core.Filtering;

/// <summary>
/// Legacy file filter implementation using simple list-based filtering.
/// </summary>
/// <remarks>
/// This class is obsolete and maintained only for backward compatibility.
/// Use FilterRulesSource with gitignore-style rules instead.
/// This class will be removed in v2.0.0.
/// </remarks>
[Obsolete("Use FilterRulesSource with gitignore-style rules instead. This class will be removed in v2.0.0")]
/// <summary>
/// Default implementation of <see cref="IFileFilter"/>. Matches against include/exclude lists for names/extensions.
/// Backward-compatible legacy filter.
/// </summary>
internal class FileFilter : IFileFilter
{
    private readonly FilterOptions _options;

    /// <summary>Initializes with filter options lists.</summary>
    /// <param name="options">Lists of include/exclude.</param>
    public FileFilter(FilterOptions options)
    {
        _options = options;
    }

    /// <summary>Include if in includeNames or not in excludes; checks extension if IncludeExtensions set.</summary>
    public bool ShouldIncludeFile(string fileName, string fullPath)
    {
        if (_options.IncludeNames.Contains(fileName, StringComparer.OrdinalIgnoreCase))
            return true;

        if (_options.ExcludeNames.Contains(fileName, StringComparer.OrdinalIgnoreCase))
            return false;

        var extension = Path.GetExtension(fileName);

        if (_options.IncludeExtensions.Any())
            return _options.IncludeExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase);

        if (_options.ExcludeExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
            return false;

        return true;
    }

    /// <summary>Include if in includeNames or not in excludeNames (no extension logic).</summary>
    public bool ShouldIncludeDirectory(string directoryName, string fullPath)
    {
        if (_options.IncludeNames.Contains(directoryName, StringComparer.OrdinalIgnoreCase))
            return true;

        if (_options.ExcludeNames.Contains(directoryName, StringComparer.OrdinalIgnoreCase))
            return false;

        return true;
    }
}
