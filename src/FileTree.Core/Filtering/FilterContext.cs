using FileTree.Core.GitIgnore;
using FileTree.Core.Models;

namespace FileTree.Core.Filtering;

/// <summary>
/// Context for filter operations: combines <see cref="FilterOptions"/> with optional gitignore rules.
/// Passed to rule loaders/evaluators.
/// </summary>
internal class FilterContext
{
    /// <summary>Initializes with options and optional gitignore.</summary>
    public FilterContext(FilterOptions options, GitIgnoreRules? gitIgnoreRules = null)
    {
        Options = options;
        GitIgnoreRules = gitIgnoreRules;
    }

    /// <summary>Filter options from user input.</summary>
    public FilterOptions Options { get; }
    /// <summary>Loaded .gitignore rules if applicable.</summary>
    public GitIgnoreRules? GitIgnoreRules { get; }
}