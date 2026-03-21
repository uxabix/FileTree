using FileTree.Core.Models;

namespace FileTree.Core.Formatting;

internal static class CollapsePlaceholderFormatter
{
    internal static string Format(FileNode node, FormatContext context)
    {
        var text = BuildText(node, context.Options);
        return context.Format == OutputFormat.Markdown ? $"`{text}`" : text;
    }

    internal static string BuildText(FileNode node, FileTreeOptions options)
    {
        if (options.CollapseStyle == CollapseStyle.Simple)
        {
            return "...";
        }

        if (options.CollapseStyle == CollapseStyle.ByExtension &&
            node.CollapsedFolderCount == 0 &&
            node.CollapsedFileCount > 0 &&
            !string.IsNullOrWhiteSpace(node.CollapsedExtensionHint))
        {
            return $"... ({node.CollapsedCount} more {node.CollapsedExtensionHint} files)";
        }

        var label = GetCollapsedLabel(node);
        return $"... ({node.CollapsedCount} more {label})";
    }

    private static string GetCollapsedLabel(FileNode node)
    {
        if (node.CollapsedFileCount > 0 && node.CollapsedFolderCount > 0)
        {
            return "files and folders";
        }

        if (node.CollapsedFolderCount > 0 && node.CollapsedFileCount == 0)
        {
            return "folders";
        }

        return "files";
    }
}
