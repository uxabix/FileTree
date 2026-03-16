using FileTree.Core.Abstractions;
using FileTree.Core.Formatting;
using FileTree.Core.Models;
using FileTree.Core.Scanning;
using FileTree.Core.Utilities;
using System.IO;

namespace FileTree.Core.Services;

public class FileTreeService : IFileTreeService
{
    private readonly IFileScanner _scanner;
    private readonly TreeFormatterFactory _formatterFactory;

    internal FileTreeService(
        IFileScanner scanner,
        TreeFormatterFactory formatterFactory)
    {
        _scanner = scanner;
        _formatterFactory = formatterFactory;
    }

    public FileTreeService() : this(new FileScanner(), new TreeFormatterFactory())
    {

    }

    /// <summary>
    /// Generates a formatted file tree representation for the specified directory.
    /// </summary>
    /// <param name="rootPath">Path to the root directory to scan. Must be a valid, accessible directory.</param>
    /// <param name="options">Scanning and formatting options.</param>
    /// <returns>Formatted tree string.</returns>
    /// <exception cref="PathValidationException">Thrown if rootPath is invalid, not a directory, or inaccessible.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown if scanning encounters permission issues inside dirs.</exception>
    public string Generate(string rootPath, FileTreeOptions options)
    {
        ValidateRootPath(rootPath);

        var rootNode = _scanner.Scan(rootPath, options);

        var formatter = _formatterFactory.Create(options.Format);
        return formatter.Format(rootNode);
    }

    private void ValidateRootPath(string rootPath)
    {
        if (string.IsNullOrWhiteSpace(rootPath))
            throw new PathValidationException(
                "Directory path cannot be empty or whitespace.", 
                PathErrorType.InvalidFormat, 
                string.Empty);

        string fullPath;
        try
        {
            fullPath = Path.GetFullPath(rootPath);
        }
        catch (ArgumentException ex)
        {
            throw new PathValidationException(
                $"Invalid path format '{rootPath}': {ex.Message}. Use a valid directory path.",
                PathErrorType.InvalidFormat,
                rootPath, ex);
        }

        if (!Directory.Exists(fullPath))
        {
            if (File.Exists(fullPath))
            {
                throw new PathValidationException(
                    $"'{fullPath}' is a file, not a directory. Specify an existing directory path.",
                    PathErrorType.NotDirectory,
                    fullPath);
            }
            else
            {
                throw new PathValidationException(
                    $"Directory not found: '{fullPath}'. Verify the path exists and is accessible.",
                    PathErrorType.NotFound,
                    fullPath);
            }
        }

        // Probe access
        try
        {
        var di = new DirectoryInfo(fullPath);
        di.GetFileSystemInfos(); // Dispose not needed for probe
            _ = di.GetFileSystemInfos(); // Throws if no read access
        }
        catch (UnauthorizedAccessException ex)
        {
            throw new PathValidationException(
                $"Access denied to directory '{fullPath}'. Check permissions or run as administrator.",
                PathErrorType.AccessDenied,
                fullPath, ex);
        }
    }
}
