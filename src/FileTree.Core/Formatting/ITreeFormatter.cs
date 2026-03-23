using FileTree.Core.Models;

namespace FileTree.Core.Formatting;

/// <summary>
/// Contract for formatting a <see cref="FileNode"/> tree into string representation.
/// Supports different output styles (ASCII, Unicode, Markdown).
/// </summary>
internal interface ITreeFormatter
{
    /// <summary>
    /// Formats the tree hierarchy into a string.
    /// </summary>
    /// <param name="root">Root node of the tree.</param>
    /// <param name="context">Formatting context (options, styles).</param>
    /// <returns>Formatted tree string ready for output.</returns>
    string Format(FileNode root, FormatContext context);
}
