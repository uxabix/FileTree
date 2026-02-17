using System.Collections.Generic;

namespace FileTree.Core.Models;

public class FileNode
{
    private readonly List<FileNode> _children = new();

    public FileNode(string name, string fullPath, bool isDirectory)
    {
        Name = name;
        FullPath = fullPath;
        IsDirectory = isDirectory;
    }

    public string Name { get; }
    public string FullPath { get; }
    public bool IsDirectory { get; }
    public IReadOnlyList<FileNode> Children => _children;

    public void AddChild(FileNode child)
    {
        _children.Add(child);
    }

    public void RemoveChild(FileNode child)
    {
        _children.Remove(child);
    }
}
