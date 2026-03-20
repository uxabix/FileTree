using System.IO;
using FileTree.Core.Models;

namespace FileTree.Core.Processing;

internal sealed class CollapseProcessor : ITreeProcessor
{
    public FileNode Process(FileNode root, FileTreeOptions options)
    {
        if (root == null) throw new ArgumentNullException(nameof(root));
        if (options == null) throw new ArgumentNullException(nameof(options));

        if (options.CollapseThreshold is null)
        {
            return root;
        }

        return CloneWithCollapse(root, options);
    }

    private static FileNode CloneWithCollapse(FileNode node, FileTreeOptions options)
    {
        var clone = CloneNode(node);

        if (!node.IsDirectory || node.Children.Count == 0)
        {
            return clone;
        }

        var threshold = options.CollapseThreshold.GetValueOrDefault();
        var keepStart = Math.Max(0, options.CollapseKeepStart);
        var keepEnd = Math.Max(0, options.CollapseKeepEnd);
        var childCount = node.Children.Count;

        if (childCount <= threshold || keepStart + keepEnd >= childCount)
        {
            foreach (var child in node.Children)
            {
                clone.AddChild(CloneWithCollapse(child, options));
            }

            return clone;
        }

        for (var i = 0; i < keepStart; i++)
        {
            clone.AddChild(CloneWithCollapse(node.Children[i], options));
        }

        var middleCount = childCount - keepStart - keepEnd;
        var middleItems = node.Children.Skip(keepStart).Take(middleCount).ToList();
        if (middleItems.Count > 0)
        {
            clone.AddChild(CreatePlaceholder(node, middleItems));
        }

        for (var i = childCount - keepEnd; i < childCount; i++)
        {
            clone.AddChild(CloneWithCollapse(node.Children[i], options));
        }

        return clone;
    }

    private static FileNode CloneNode(FileNode source)
    {
        return new FileNode(source.Name, source.FullPath, source.IsDirectory, source.IsHidden)
        {
            IsCollapsedPlaceholder = source.IsCollapsedPlaceholder,
            CollapsedCount = source.CollapsedCount,
            CollapsedExtensionHint = source.CollapsedExtensionHint,
        };
    }

    private static FileNode CreatePlaceholder(FileNode parent, IReadOnlyList<FileNode> collapsed)
    {
        return new FileNode("...", Path.Combine(parent.FullPath, "..."), false)
        {
            IsCollapsedPlaceholder = true,
            CollapsedCount = collapsed.Count,
            CollapsedExtensionHint = GetCollapsedExtensionHint(collapsed),
        };
    }

    private static string? GetCollapsedExtensionHint(IReadOnlyList<FileNode> collapsed)
    {
        string? extension = null;

        foreach (var node in collapsed)
        {
            if (node.IsDirectory)
            {
                return null;
            }

            var current = Path.GetExtension(node.Name);
            if (string.IsNullOrWhiteSpace(current))
            {
                return null;
            }

            if (extension == null)
            {
                extension = current;
            }
            else if (!string.Equals(extension, current, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }
        }

        return extension;
    }
}