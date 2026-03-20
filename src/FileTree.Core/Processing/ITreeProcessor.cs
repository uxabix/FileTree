using FileTree.Core.Models;

namespace FileTree.Core.Processing;

internal interface ITreeProcessor
{
    FileNode Process(FileNode root, FileTreeOptions options);
}