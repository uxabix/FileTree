using FileTree.Core.Models;

namespace FileTree.Core.Filtering;

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
