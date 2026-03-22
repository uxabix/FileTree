using FileTree.Core.Models;

namespace FileTree.Core.Formatting;

/// <summary>
/// Formats placeholder strings for collapsed nodes based on <see cref="CollapseStyle"/>: Simple "...", Count, ByExtension.
/// Called by formatters for IsCollapsedPlaceholder nodes.
/// </summary>
internal static class CollapsePlaceholderFormatter
{
    /// <summary>Main entry: Builds text, adds Markdown backticks if needed.</summary>
    /// <param name="node">Collapsed node with counts.</param>
    /// <param name="context">Context for format-specific wrapping.</param>
    /// <returns>Placeholder string.</returns>
    internal static string Format(FileNode node, FormatContext context)
    {
        var text = BuildText(node, context.Options);
        return context.Format == OutputFormat.Markdown ? $"`{text}`" : text;
    }

    /// <summary>Builds core placeholder text based on style/counts/extension hint.</summary>
    /// <param name="node">Node stats.</param>
    /// <param name="options">Collapse style/options.</param>
    /// <returns>Text like "... (3 files)" or "... (12 .cs)".</returns>
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
