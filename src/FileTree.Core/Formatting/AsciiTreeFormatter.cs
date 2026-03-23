using System.Text;
using FileTree.Core.Models;

namespace FileTree.Core.Formatting;

/// <summary>
/// ASCII art tree formatter using | / \ - characters.
/// Compatible with all terminals.
/// </summary>
internal class AsciiTreeFormatter : ITreeFormatter
{
    /// <summary>Formats tree using ASCII characters. Root printed first, then children with connectors.</summary>
    public string Format(FileNode root, FormatContext context)
    {
        var sb = new StringBuilder();
        sb.AppendLine(FormatNode(root, context));
        TreeFormatter.Build(
            root,
            sb,
            "",
            "`-- ",
            "|-- ",
            " ",
            "|   ",
            node => FormatNode(node, context));
        return sb.ToString();
    }

    /// <summary>Formats single node name, handling collapsed placeholders and hidden styling.</summary>
    private static string FormatNode(FileNode node, FormatContext context)
    {
        if (node.IsCollapsedPlaceholder)
        {
            return CollapsePlaceholderFormatter.Format(node, context);
        }

        return NodeNameStyler.Apply(node.Name, node, context);
    }
}
