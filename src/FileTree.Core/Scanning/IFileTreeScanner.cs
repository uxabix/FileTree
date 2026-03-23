using FileTree.Core.Models;

namespace FileTree.Core.Scanning;

/// <summary>
/// Defines the scanning contract for building file tree structures from directories.
/// Implementations handle recursion, limits, filtering during scan.
/// Note: Named IFileScanner internally, but file is IFileTreeScanner.cs.
/// </summary>
internal interface IFileScanner
{
    /// <summary>
    /// Scans the directory tree starting from <paramref name="rootPath"/>, building a <see cref="FileNode"/> hierarchy.
    /// Applies scanning limits, hidden/gitignore checks based on <paramref name="options"/>.
    /// </summary>
    /// <param name="rootPath">Absolute path to root directory.</param>
    /// <param name="options">Scan configuration (depth, width, filters).</param>
    /// <returns>Root <see cref="FileNode"/> representing the tree.</returns>
    /// <exception cref="UnauthorizedAccessException">Access denied during scan.</exception>
    FileNode Scan(string rootPath, FileTreeOptions options);
}
