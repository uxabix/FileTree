namespace FileTree.Core.Models;

/// <summary>
/// Tree rendering format.
/// </summary>
public enum OutputFormat
{
    /// <summary>ASCII box chars (| / \ -).</summary>
    Ascii,
    /// <summary>Markdown tree list.</summary>
    Markdown,
    /// <summary>Unicode tree chars (│ ├ └ ─).</summary>
    Unicode

}