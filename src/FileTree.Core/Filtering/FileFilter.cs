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
internal class FileFilter : IFileFilter
{
    private readonly FilterOptions _options;

    public FileFilter(FilterOptions options)
    {
        _options = options;
    }

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

    public bool ShouldIncludeDirectory(string directoryName, string fullPath)
    {
        if (_options.IncludeNames.Contains(directoryName, StringComparer.OrdinalIgnoreCase))
            return true;

        if (_options.ExcludeNames.Contains(directoryName, StringComparer.OrdinalIgnoreCase))
            return false;

        return true;
    }
}
