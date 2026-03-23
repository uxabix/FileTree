using FileTree.Core.Models;

namespace FileTree.Core.Formatting;

/// <summary>
/// Immutable context passed to formatters, providing access to <see cref="FileTreeOptions"/>.
/// Enables format-specific options without passing options directly.
/// </summary>
internal sealed class FormatContext
{
    /// <summary>Creates context from options.</summary>
    /// <param name="options">Formatting options.</param>
    public FormatContext(FileTreeOptions options)
    {
        Options = options ?? throw new ArgumentNullException(nameof(options));
    }

    /// <summary>Original options.</summary>
    public FileTreeOptions Options { get; }
    /// <summary>Shortcut to <see cref="Options.Format"/>.</summary>
    public OutputFormat Format => Options.Format;
}
