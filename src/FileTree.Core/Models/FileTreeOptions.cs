namespace FileTree.Core.Models;

public class FileTreeOptions
{
    public int MaxDepth { get; init; }
    public int MaxWidth { get; init; }
    public int MaxNodes { get; init; }
    public bool UseGitIgnore { get; init; }
    public bool SkipHidden { get; init; }
    public OutputFormat Format { get; init; }
    public FilterOptions Filter { get; init; }
}
