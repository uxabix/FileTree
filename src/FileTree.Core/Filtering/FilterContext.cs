using FileTree.Core.Models;
using FileTree.Core.GitIgnore;

namespace FileTree.Core.Filtering;

internal class FilterContext
{
    public FilterOptions Options { get; }
    public GitIgnoreRules? GitIgnoreRules { get; }
}