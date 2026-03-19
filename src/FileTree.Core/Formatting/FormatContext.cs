using FileTree.Core.Models;

namespace FileTree.Core.Formatting;

internal sealed class FormatContext
{
    public FormatContext(FileTreeOptions options)
    {
        Options = options ?? throw new ArgumentNullException(nameof(options));
    }

    public FileTreeOptions Options { get; }
    public OutputFormat Format => Options.Format;
}
