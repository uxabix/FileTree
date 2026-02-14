using FileTree.Core.Models;

namespace FileTree.Core.Filtering;

internal interface IFilterEngine
{
    void Apply(FileNode root, FilterContext context);
}