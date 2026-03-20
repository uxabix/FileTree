namespace FileTree.Core.Models;

public class FileTreeOptions
{
    public int MaxDepth { get; init; } = -1;
    public int MaxWidth { get; init; } = -1;
    public int MaxNodes { get; init; } = -1;
    public bool UseGitIgnore { get; init; }
    public bool SkipHidden { get; init; }
    public bool HighlightHiddenFiles { get; init; } = true;
    public HiddenStyle HiddenStyle { get; init; } = HiddenStyle.Prefix;
    public OutputFormat Format { get; init; } = OutputFormat.Ascii;
    public FilterOptions Filter { get; init; } = new();
    public int? CollapseThreshold { get; init; }
    public int CollapseKeepStart { get; init; } = 1;
    public int CollapseKeepEnd { get; init; } = 1;
    public CollapseStyle CollapseStyle { get; init; } = CollapseStyle.Count;
}
