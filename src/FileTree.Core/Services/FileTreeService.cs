using FileTree.Core.Abstractions;
using FileTree.Core.Filtering;
using FileTree.Core.Formatting;
using FileTree.Core.GitIgnore;
using FileTree.Core.Models;
using FileTree.Core.Scanning;

namespace FileTree.Core.Services;

public class FileTreeService : IFileTreeService
{
    private readonly IFileScanner _scanner;
    private readonly IFilterEngine _filterEngine;
    private readonly TreeFormatterFactory _formatterFactory;

    public FileTreeService(
        IFileScanner scanner,
        IFilterEngine filterEngine,
        TreeFormatterFactory formatterFactory)
    {
        _scanner = scanner;
        _filterEngine = filterEngine;
        _formatterFactory = formatterFactory;
    }

    public string Generate(string rootPath, FileTreeOptions options)
    {
        var rootNode = _scanner.Scan(rootPath, options);

        GitIgnoreRules? gitIgnore = null;
        if (options.UseGitIgnore)
        {
            var gitIgnorePath = Path.Combine(rootPath, ".gitignore");
            if (File.Exists(gitIgnorePath))
                gitIgnore = GitIgnoreParser.Parse(File.ReadAllLines(gitIgnorePath));
        }

        var context = new FilterContext(options.Filter, gitIgnore);
        var filteredRoot = _filterEngine.Apply(rootNode, context);

        var formatter = _formatterFactory.Create(options.Format);
        return formatter.Format(filteredRoot);
    }
}
