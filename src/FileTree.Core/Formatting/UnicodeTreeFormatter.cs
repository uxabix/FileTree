using System.Text;
using FileTree.Core.Models;

namespace FileTree.Core.Formatting;

internal class UnicodeTreeFormatter : ITreeFormatter
{
    public string Format(FileNode root, FormatContext context)
    {
        var sb = new StringBuilder();
        sb.AppendLine(NodeNameStyler.Apply(root.Name, root, context));
        TreeFormatter.Build(
            root,
            sb,
            "",
            "└─ ",
            "├─ ",
            " ",
            "│   ",
            node => NodeNameStyler.Apply(node.Name, node, context));
        return sb.ToString();
    }
}
