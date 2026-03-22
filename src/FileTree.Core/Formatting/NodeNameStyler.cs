using FileTree.Core.Models;

namespace FileTree.Core.Formatting;

/// <summary>
/// Utility for styling node names: applies hidden file markers (prefix/suffix/minimal) based on options.
/// Used by all tree formatters.
/// </summary>
internal static class NodeNameStyler
{
    /// <summary>Applies hidden styling if enabled and node is hidden.</summary>
    /// <param name="name">Base name.</param>
    /// <param name="node">Node metadata.</param>
    /// <param name="context">Context for style.</param>
    /// <returns>Styled name.</returns>
    internal static string Apply(string name, FileNode node, FormatContext context)
    {
        if (string.IsNullOrEmpty(name))
        {
            return name;
        }

        if (!context.Options.HighlightHiddenFiles || !node.IsHidden)
        {
            return name;
        }

        return ApplyHiddenStyle(name, context);
    }

    /// <summary>Applies the configured hidden style to name.</summary>
    private static string ApplyHiddenStyle(string name, FormatContext context)
    {
        return context.Options.HiddenStyle switch
        {
            HiddenStyle.Prefix => $"{GetHiddenPrefix(context)} {name}",
            HiddenStyle.Suffix => $"{name}{GetHiddenSuffix(context)}",
            HiddenStyle.Minimal => $"{GetHiddenMinimalPrefix(context)} {name}",
            _ => name
        };
    }

    private static string GetHiddenPrefix(FormatContext context)
    {
        return "[hidden]";
    }

    private static string GetHiddenMinimalPrefix(FormatContext context)
    {
        return "[h]";
    }

    private static string GetHiddenSuffix(FormatContext context)
    {
        return context.Format == OutputFormat.Markdown
            ? " _(hidden)_"
            : " (hidden)";
    }
}
