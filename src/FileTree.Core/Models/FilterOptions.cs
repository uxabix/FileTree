using System;

namespace FileTree.Core.Models;

public class FilterOptions
{
    /// <summary>
    /// Source configuration for gitignore-style filtering rules.
    /// This is the recommended way to configure filtering.
    /// </summary>
    public FilterRulesSource? RulesSource { get; set; }

    /// <summary>
    /// Skip folders that have no children after filtering is applied.
    /// </summary>
    public bool IgnoreEmptyFolders { get; set; }

    // ============================================================================
    // Legacy filtering properties (deprecated)
    // These properties are maintained for backward compatibility and will be
    // removed in a future major version. Use RulesSource instead.
    // ============================================================================

    /// <summary>
    /// Include only these extensions if list is not empty.
    /// </summary>
    [Obsolete("Use RulesSource with gitignore-style rules instead. This property will be removed in v2.0.0")]
    public List<string> IncludeExtensions { get; set; } = new();

    /// <summary>
    /// Extensions to always exclude.
    /// </summary>
    [Obsolete("Use RulesSource with gitignore-style rules instead. This property will be removed in v2.0.0")]
    public List<string> ExcludeExtensions { get; set; } = new();

    /// <summary>
    /// File/directory names to include always.
    /// </summary>
    [Obsolete("Use RulesSource with gitignore-style rules instead. This property will be removed in v2.0.0")]
    public List<string> IncludeNames { get; set; } = new();

    /// <summary>
    /// File/directory names to exclude.
    /// </summary>
    [Obsolete("Use RulesSource with gitignore-style rules instead. This property will be removed in v2.0.0")]
    public List<string> ExcludeNames { get; set; } = new();
}