using System.Text;
using FileTree.Core.Models;
namespace FileTree.Core.Formatting;

public class TreeFormatter
{
    static public void Build(FileNode node, StringBuilder sb, string prefix, string lastHorizontalSepparator, string horizontalSepparator, string lastVerticalSepparator, string verticalSepparator)
    {
        for (int i = 0; i < node.Children.Count; i++)
        {
            var child = node.Children[i];
            bool isLast = i == node.Children.Count - 1;

            var connector = isLast ? lastHorizontalSepparator : horizontalSepparator;
            sb.AppendLine(prefix + connector + child.Name);

            if (child.Children.Count > 0)
            {
                var extension = isLast ? lastVerticalSepparator : verticalSepparator;
                Build(child, sb, prefix + extension, lastHorizontalSepparator, horizontalSepparator, lastVerticalSepparator, verticalSepparator);
            }
        }
    }
}