namespace FileTree.Core.Models;

internal class FileNode
{
    public string Name { get; }
    public string FullPath { get; }
    public bool IsDirectory { get; }
    public IReadOnlyList<FileNode> Children => _children; 

    private readonly List<FileNode> _children = new();
}