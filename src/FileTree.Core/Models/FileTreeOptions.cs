namespace FileTree.Core.Models;

public class FileTreeOptions
{
    public int MaxDepth { get; init; } = -1;
    public int MaxWidth { get; init; } = -1;
    public int MaxNodes { get; init; } = -1;
    public bool UseGitIgnore { get; init; }
    public bool SkipHidden { get; init; }
    public OutputFormat Format { get; init; } = OutputFormat.Ascii;
    public FilterOptions Filter { get; init; } = new();
}
