namespace FileTree.Core.Models;

/// <summary>
/// Core configuration for tree generation.
/// Controls scanning limits, filtering, formatting, collapse.
/// Defaults favor reasonable output; -1/-1 unlimited.
/// </summary>
public class FileTreeOptions
{
    /// <summary>Max recursion depth. -1 unlimited (default).</summary>
    public int MaxDepth { get; init; } = -1;
    /// <summary>Max siblings per dir. -1 unlimited (default).</summary>
    public int MaxWidth { get; init; } = -1;
    /// <summary>Global node cap. -1 unlimited (default).</summary>
    public int MaxNodes { get; init; } = -1;
    /// <summary>Load/apply .gitignore rules from root. Default: false.</summary>
    public bool UseGitIgnore { get; init; }
    /// <summary>Exclude hidden files/dirs. Default: false.</summary>
    public bool SkipHidden { get; init; }
    /// <summary>Special style for hidden (if not skipped). Default: true.</summary>
    public bool HighlightHiddenFiles { get; init; } = true;
    /// <summary>Hidden marker style. Default: Prefix.</summary>
    public HiddenStyle HiddenStyle { get; init; } = HiddenStyle.Prefix;
    /// <summary>Tree render style (Ascii/Markdown/Unicode). Default: Ascii.</summary>
    public OutputFormat Format { get; init; } = OutputFormat.Ascii;
    /// <summary>Filtering config (rules, legacy).</summary>
    public FilterOptions Filter { get; init; } = new();
    /// <summary>Collapse dirs > N children. Null=auto.</summary>
    public int? CollapseThreshold { get; init; }
    /// <summary>First N children to show in collapsed. Default: 1.</summary>
    public int CollapseKeepStart { get; init; } = 1;
    /// <summary>Last N children to show in collapsed. Default: 1.</summary>
    public int CollapseKeepEnd { get; init; } = 1;
    /// <summary>Placeholder style. Default: Count.</summary>
    public CollapseStyle CollapseStyle { get; init; } = CollapseStyle.Count;
    /// <summary>Start collapsing from this depth (1=root children). Default: 1.</summary>
    public int CollapseFrom { get; init; } = 1;
}
