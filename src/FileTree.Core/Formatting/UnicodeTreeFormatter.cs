using System.Text;
using FileTree.Core.Models;

namespace FileTree.Core.Formatting;

/// <summary>
/// Unicode box-drawing tree formatter (│ ├ └ ─). Pretty terminal output.
/// Similar structure to Ascii, different chars.
/// </summary>
internal class UnicodeTreeFormatter : ITreeFormatter
{
    /// <summary>Formats tree using Unicode box characters. Root first, then children.</summary>
    public string Format(FileNode root, FormatContext context)
    {
        var sb = new StringBuilder();
        sb.AppendLine(FormatNode(root, context));
        TreeFormatter.Build(
            root,
            sb,
            "",
            "└─ ",
            "├─ ",
            " ",
            "│   ",
            node => FormatNode(node, context));
        return sb.ToString();
    }

    /// <summary>Formats node name, delegating to placeholder/styler.</summary>
    private static string FormatNode(FileNode node, FormatContext context)
    {
        if (node.IsCollapsedPlaceholder)
        {
            return CollapsePlaceholderFormatter.Format(node, context);
        }

        return NodeNameStyler.Apply(node.Name, node, context);
    }
}
