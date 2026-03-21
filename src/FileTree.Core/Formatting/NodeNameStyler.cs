using FileTree.Core.Models;

namespace FileTree.Core.Formatting;

internal static class NodeNameStyler
{
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
