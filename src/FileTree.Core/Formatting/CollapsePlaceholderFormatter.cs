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
            !string.IsNullOrWhiteSpace(node.CollapsedExtensionHint))
        {
            return $"... ({node.CollapsedCount} more {node.CollapsedExtensionHint} files)";
        }

        return $"... ({node.CollapsedCount} more files)";
    }
}