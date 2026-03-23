using System.Collections.Generic;

namespace FileTree.Core.Models;

/// <summary>
/// Represents a node in the file tree.
/// Leaf for files, container for dirs. Supports collapse placeholders.
/// Immutable except children/collapsed props.
/// </summary>
public class FileNode
{
    private readonly List<FileNode> _children = new();

    /// <summary>Constructor.</summary>
    /// <param name="name">Basename.</param>
    /// <param name="fullPath">Absolute path.</param>
    /// <param name="isDirectory">Dir or file.</param>
    /// <param name="isHidden">Hidden flag.</param>
    public FileNode(string name, string fullPath, bool isDirectory, bool isHidden = false)
    {
        Name = name;
        FullPath = fullPath;
        IsDirectory = isDirectory;
        IsHidden = isHidden;
    }

    /// <summary>Basename (no path).</summary>
    public string Name { get; }
    /// <summary>Absolute path.</summary>
    public string FullPath { get; }
    /// <summary>True if directory.</summary>
    public bool IsDirectory { get; }
    /// <summary>True if hidden.</summary>
    public bool IsHidden { get; }
    /// <summary>Child nodes (read-only).</summary>
    public IReadOnlyList<FileNode> Children => _children;
    /// <summary>True if placeholder for collapsed children.</summary>
    public bool IsCollapsedPlaceholder { get; set; }
    /// <summary>Total hidden children count.</summary>
    public int CollapsedCount { get; set; }
    /// <summary>Hidden file children count.</summary>
    public int CollapsedFileCount { get; set; }
    /// <summary>Hidden folder children count.</summary>
    public int CollapsedFolderCount { get; set; }
    /// <summary>Extension hint for ByExtension collapse.</summary>
    public string? CollapsedExtensionHint { get; set; }

    /// <summary>Adds child (during scan).</summary>
    /// <param name="child">Child node.</param>
    public void AddChild(FileNode child)
    {
        _children.Add(child);
    }

    /// <summary>Removes child (post-filter).</summary>
    /// <param name="child">Child to remove.</param>
    public void RemoveChild(FileNode child)
    {
        _children.Remove(child);
    }
}
