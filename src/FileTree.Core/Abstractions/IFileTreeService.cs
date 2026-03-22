using FileTree.Core.Models;

namespace FileTree.Core.Abstractions;

/// <summary>
/// Core tree generation service interface.
/// </summary>
public interface IFileTreeService
{
    /// <summary>Main entrypoint: Generates formatted tree string.</summary>
    /// <param name="rootPath">Root dir path (validated).</param>
    /// <param name="options">Scan/format config.</param>
    /// <returns>Formatted tree.</returns>
    /// <exception cref="PathValidationException">Invalid/inaccessible path.</exception>
    /// <exception cref="UnauthorizedAccessException">Scan access denied.</exception>
    string Generate(string rootPath, FileTreeOptions options);
}
