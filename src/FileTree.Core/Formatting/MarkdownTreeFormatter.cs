using System.Text;
using FileTree.Core.Models;

namespace FileTree.Core.Formatting;

internal class MarkdownTreeFormatter : ITreeFormatter
{
    public string Format(FileNode root, FormatContext context)
    {
        var sb = new StringBuilder();
        WriteNode(root, sb, 0, context);
        return sb.ToString();
    }

    private void WriteNode(FileNode node, StringBuilder sb, int depth, FormatContext context)
    {
        var indent = new string(' ', depth * 2);

        var name = node.IsDirectory ? $"{node.Name}/" : node.Name;

        var displayName = NodeNameStyler.Apply(name, node, context);
        sb.AppendLine(indent + displayName);

        foreach (var child in node.Children) WriteNode(child, sb, depth + 1, context);
    }
}
