using FileTree.Core.Models;

namespace FileTree.Core.Formatting;

internal interface ITreeFormatter
{
    string Format(FileNode root);
}