using FileTree.Core.Models;

namespace FileTree.Core.Processing;

/// <summary>
/// Post-scan tree processor pipeline interface.
/// Transforms tree (e.g., collapse nodes, prune, etc.) based on options.
/// </summary>
internal interface ITreeProcessor
{
    /// <summary>Processes/transforms the tree root.</summary>
    /// <param name="root">Input tree.</param>
    /// <param name="options">Processing options (threshold etc.).</param>
    /// <returns>Processed tree (may mutate/replace nodes).</returns>
    FileNode Process(FileNode root, FileTreeOptions options);
}