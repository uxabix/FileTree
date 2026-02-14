using System.Text;
using FileTree.Core.Models;

namespace FileTree.Core.Formatting;

public class MarkdownTreeFormatter : ITreeFormatter
{
    public string Format(FileNode root)
    {
        var sb = new StringBuilder();
        WriteNode(root, sb, 0);
        return sb.ToString();
    }

    private void WriteNode(FileNode node, StringBuilder sb, int depth)
    {
        var indent = new string(' ', depth * 2);

        var name = node.IsDirectory ? $"{node.Name}/" : node.Name;

        sb.AppendLine(indent + name);

        foreach (var child in node.Children)
        {
            WriteNode(child, sb, depth + 1);
        }
    }
}