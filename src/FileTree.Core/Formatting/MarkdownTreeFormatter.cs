using System.Text;
using FileTree.Core.Models;

namespace FileTree.Core.Formatting;

/// <summary>
/// Markdown list-style tree formatter.
/// Indented bullet-less list with / for dirs.
/// Ideal for GitHub README.
/// </summary>
internal class MarkdownTreeFormatter : ITreeFormatter
{
    /// <summary>Formats tree as indented Markdown list. Recursive write.</summary>
    public string Format(FileNode root, FormatContext context)
    {
        var sb = new StringBuilder();
        WriteNode(root, sb, 0, context);
        return sb.ToString();
    }

    /// <summary>Recursively writes node at depth, with 2-space indent.</summary>
    /// <param name="node">Current node.</param>
    /// <param name="sb">Builder to append to.</param>
    /// <param name="depth">Indent level.</param>
    /// <param name="context">Formatting context.</param>
    private void WriteNode(FileNode node, StringBuilder sb, int depth, FormatContext context)
    {
        var indent = new string(' ', depth * 2);

        var displayName = FormatNode(node, context);
        sb.AppendLine(indent + displayName);

        foreach (var child in node.Children) WriteNode(child, sb, depth + 1, context);
    }

    /// <summary>Formats node name: dir / suffix, hidden styling, collapsed placeholders.</summary>
    private static string FormatNode(FileNode node, FormatContext context)
    {
        if (node.IsCollapsedPlaceholder)
        {
            return CollapsePlaceholderFormatter.Format(node, context);
        }

        var name = node.IsDirectory ? $"{node.Name}/" : node.Name;
        return NodeNameStyler.Apply(name, node, context);
    }
}
