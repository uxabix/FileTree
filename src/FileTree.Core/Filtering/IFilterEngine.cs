using FileTree.Core.Models;

namespace FileTree.Core.Filtering;

internal interface IFilterEngine
{
    FileNode Apply(FileNode root, FilterContext context);
}