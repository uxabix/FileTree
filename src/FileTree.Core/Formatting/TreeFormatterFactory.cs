using FileTree.Core.Models;

namespace FileTree.Core.Formatting;

/// <summary>
/// Factory for selecting concrete <see cref="ITreeFormatter"/> implementations based on <see cref="OutputFormat"/>.
/// Simple switch, extensible for new formats.
/// </summary>
internal class TreeFormatterFactory
{
    /// <summary>Creates formatter for the given format.</summary>
    /// <param name="format">Desired output format.</param>
    /// <returns>Appropriate formatter instance.</returns>
    /// <exception cref="NotSupportedException">Unknown format.</exception>
    public ITreeFormatter Create(OutputFormat format)
    {
        return format switch
        {
            OutputFormat.Ascii => new AsciiTreeFormatter(),
            OutputFormat.Markdown => new MarkdownTreeFormatter(),
            OutputFormat.Unicode => new UnicodeTreeFormatter(),

            _ => throw new NotSupportedException($"Unsupported format: {format}")
        };
    }
}