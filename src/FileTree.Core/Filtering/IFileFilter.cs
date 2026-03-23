namespace FileTree.Core.Filtering;

/// <summary>
/// Legacy filtering interface for file/dir inclusion decisions.
/// Used for backward compatibility with list-based filters.
/// New code should use gitignore-style FilterRulesSource.
/// </summary>
internal interface IFileFilter
{
    /// <summary>Decides if file should be included based on name/path lists.</summary>
    bool ShouldIncludeFile(string fileName, string fullPath);
    /// <summary>Decides if directory should be included based on name/path lists.</summary>
    bool ShouldIncludeDirectory(string directoryName, string fullPath);
}
