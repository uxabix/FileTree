using FileTree.Core.GitIgnore;
using FileTree.Core.Models;

namespace FileTree.Core.Filtering;

internal class FilterContext
{
    public FilterContext(FilterOptions options, GitIgnoreRules? gitIgnoreRules = null)
    {
        Options = options;
        GitIgnoreRules = gitIgnoreRules;
    }

    public FilterOptions Options { get; }
    public GitIgnoreRules? GitIgnoreRules { get; }
}