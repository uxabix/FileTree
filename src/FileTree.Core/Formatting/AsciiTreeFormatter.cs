using System.Text;
using FileTree.Core.Models;
namespace FileTree.Core.Formatting;

internal class AsciiTreeFormatter : ITreeFormatter
{

    public string Format(FileNode root)
    {
        var sb = new StringBuilder();
        sb.AppendLine(root.Name);
        TreeFormatter.Build(root, sb, "", "`-- ", "|-- ", " ", "|   ");
        return sb.ToString();
    }
}