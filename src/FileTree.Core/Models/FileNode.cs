using System.Collections.Generic;

namespace FileTree.Core.Models;

public class FileNode
{
    private readonly List<FileNode> _children = new();

    public FileNode(string name, string fullPath, bool isDirectory, bool isHidden = false)
    {
        Name = name;
        FullPath = fullPath;
        IsDirectory = isDirectory;
        IsHidden = isHidden;
    }

    public string Name { get; }
    public string FullPath { get; }
    public bool IsDirectory { get; }
    public bool IsHidden { get; }
    public IReadOnlyList<FileNode> Children => _children;
    public bool IsCollapsedPlaceholder { get; set; }
    public int CollapsedCount { get; set; }
    public string? CollapsedExtensionHint { get; set; }

    public void AddChild(FileNode child)
    {
        _children.Add(child);
    }

    public void RemoveChild(FileNode child)
    {
        _children.Remove(child);
    }
}
