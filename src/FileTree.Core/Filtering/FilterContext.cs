using FileTree.Core.Models;
using FileTree.Core.GitIgnore;

namespace FileTree.Core.Filtering;

internal class FilterContext
{
    public FilterOptions Options { get; }
    public GitIgnoreRules? GitIgnoreRules { get; }

    public FilterContext(FilterOptions options, GitIgnoreRules? gitIgnoreRules = null)
    {
        Options = options;
        GitIgnoreRules = gitIgnoreRules;
    }
}