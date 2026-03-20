using System.Text;
using FileTree.Core.Models;

namespace FileTree.Core.Formatting;

internal class UnicodeTreeFormatter : ITreeFormatter
{
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

    private static string FormatNode(FileNode node, FormatContext context)
    {
        if (node.IsCollapsedPlaceholder)
        {
            return CollapsePlaceholderFormatter.Format(node, context);
        }

        return NodeNameStyler.Apply(node.Name, node, context);
    }
}
