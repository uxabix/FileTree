using System.Text;
using FileTree.Core.Models;

namespace FileTree.Core.Formatting;

internal class TreeFormatter
{
    public static void Build(FileNode node, StringBuilder sb, string prefix, string lastHorizontalSepparator,
        string horizontalSepparator, string lastVerticalSepparator, string verticalSepparator,
        Func<FileNode, string> formatName)
    {
        for (var i = 0; i < node.Children.Count; i++)
        {
            var child = node.Children[i];
            var isLast = i == node.Children.Count - 1;

            var connector = isLast ? lastHorizontalSepparator : horizontalSepparator;
            var displayName = formatName(child);
            sb.AppendLine(prefix + connector + displayName);

            if (child.Children.Count > 0)
            {
                var extension = isLast ? lastVerticalSepparator : verticalSepparator;
                Build(child, sb, prefix + extension, lastHorizontalSepparator, horizontalSepparator,
                    lastVerticalSepparator, verticalSepparator, formatName);
            }
        }
    }
}
