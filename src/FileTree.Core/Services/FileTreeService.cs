using FileTree.Core.Abstractions;
using FileTree.Core.Formatting;
using FileTree.Core.Models;
using FileTree.Core.Scanning;

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

    public string Generate(string rootPath, FileTreeOptions options)
    {
        var rootNode = _scanner.Scan(rootPath, options);

        var formatter = _formatterFactory.Create(options.Format);
        return formatter.Format(rootNode);
    }
}
