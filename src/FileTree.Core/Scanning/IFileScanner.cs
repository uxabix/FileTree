using FileTree.Core.Models;

namespace FileTree.Core.Scanning;

internal interface IFileScanner
{
    int MaxDepth { get; init; }
    int MaxWidth { get; init; }
    int MaxNodes { get; init; }
    FileNode Scan(string rootPath);
}