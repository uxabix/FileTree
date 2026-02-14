using FileTree.Core.Models;

namespace FileTree.Core.Scanning;

internal interface IFileScanner
{
    FileNode Scan(string rootPath, int maxDepth);
}