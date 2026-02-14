using FileTree.Core.Models;

namespace FileTree.Core.Formatting;

internal class TreeFormatterFactory
{
    public ITreeFormatter Create(OutputFormat format)
    {
        return format switch
        {
            OutputFormat.Ascii => new AsciiTreeFormatter(),
            OutputFormat.Markdown => new MarkdownTreeFormatter(),
            _ => throw new NotSupportedException($"Unsupported format: {format}")
        };
    }
}