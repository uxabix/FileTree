using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using FileTree.Core.Filtering;
using FileTree.Core.GitIgnore;
using FileTree.Core.Models;

namespace FileTree.Core.Scanning
{
    /// <summary>
    /// Evaluates whether file system items should be included in the scan based on limits, filters, gitignore rules, hidden status, etc.
    /// Supports hierarchical .filetreeignore files and legacy filter conversion.
    /// Uses stack for local rules scope.
    /// </summary>
    internal class ScanInclusionEvaluator
    {
        private readonly GitIgnoreRules? _gitIgnoreRules;
        private GitIgnoreRules? _filterRules;
        private readonly IFileFilter? _legacyFileFilter;
        private readonly string _rootPath;
        private readonly FileTreeOptions _options;
        private readonly List<List<string>> _localRulesStack = new();
        private readonly List<string> _baseFilterRules;
        private readonly bool _useLocalFilterFiles;
        /// <summary>Name of local ignore file for directory-specific rules.</summary>
        private const string LocalIgnoreFileName = ".filetreeignore";

        /// <summary>
        /// Creates a new instance of ScanInclusionEvaluator.
        /// </summary>
        /// <param name="rootPath">The root directory path being scanned.</param>
        /// <param name="options">File tree scanning options.</param>
        /// <param name="gitIgnoreRules">Optional .gitignore rules to apply.</param>
        /// <param name="filterRules">Optional custom filter rules to apply.</param>
        /// <summary>Initializes evaluator with root path, options, and rules sources.</summary>
        /// <remarks>Combines gitignore, custom rules, legacy filters. Builds initial filter rules.</remarks>
        public ScanInclusionEvaluator(
            string rootPath,
            FileTreeOptions options,
            GitIgnoreRules? gitIgnoreRules,
            IReadOnlyList<string>? filterRules = null)
        {
            _rootPath = Path.GetFullPath(rootPath);
            _options = options;
            _gitIgnoreRules = gitIgnoreRules;
            _baseFilterRules = filterRules?.Where(rule => !string.IsNullOrWhiteSpace(rule))
                .Select(rule => rule.Trim())
                .ToList() ?? new List<string>();
            _useLocalFilterFiles = options.Filter.UseLocalFilterFiles;

            // For backward compatibility, create legacy file filter if no new-style filter rules provided
            // and legacy filter options are present
            if (_baseFilterRules.Count == 0 && LegacyFilterConverter.HasLegacyFilters(options.Filter))
            {
                _legacyFileFilter = new FileFilter(options.Filter);
            }

            RebuildFilterRules();
        }

        /// <summary>
        /// Determines whether a file or directory should be included in the scan results.
        /// </summary>
        /// <param name="item">The file or directory to evaluate.</param>
        /// <param name="currentDepth">Current depth in the directory tree.</param>
        /// <param name="currentNodeCount">Current number of nodes scanned.</param>
        /// <returns>True if the item should be included, false otherwise.</returns>
        public bool ShouldInclude(FileSystemInfo item, int currentDepth, int currentNodeCount)
        {
            // Check depth limits
            if (_options.MaxDepth != -1 && currentDepth >= _options.MaxDepth)
                return false;

            // Check node count limits
            if (_options.MaxNodes != -1 && currentNodeCount >= _options.MaxNodes)
                return false;

            // Skip hidden files/directories if configured
            if (_options.SkipHidden && HiddenFileDetector.IsHidden(item))
                return false;

            // Skip symbolic links and reparse points
            if (item.Attributes.HasFlag(FileAttributes.ReparsePoint))
                return false;

            // Get relative path for gitignore-style matching
            var relativePath = Path.GetRelativePath(_rootPath, item.FullName).Replace('\\', '/');
            if (item is DirectoryInfo)
                relativePath += "/"; // Gitignore requires trailing slash for directories

            // Apply .gitignore rules if configured
            if (_gitIgnoreRules != null && _gitIgnoreRules.IsIgnored(relativePath))
                return false;

            // Apply custom filter rules (new gitignore-style filtering)
            if (_filterRules != null && _filterRules.IsIgnored(relativePath))
                return false;

            // Apply legacy filtering for backward compatibility
            if (_legacyFileFilter != null)
            {
                bool isDir = item is DirectoryInfo;

                if (isDir)
                {
                    if (!_legacyFileFilter.ShouldIncludeDirectory(item.Name, item.FullName))
                        return false;
                }
                else
                {
                    if (!_legacyFileFilter.ShouldIncludeFile(item.Name, item.FullName))
                        return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Loads .filetreeignore from dir if exists, pushes to stack, rebuilds rules.
        /// </summary>
        /// <param name="dirInfo">Directory to check for local ignore.</param>
        /// <returns>True if rules loaded/pushed.</returns>
        public bool EnterDirectory(DirectoryInfo dirInfo)
        {
            if (!_useLocalFilterFiles)
            {
                return false;
            }

            var ignorePath = Path.Combine(dirInfo.FullName, LocalIgnoreFileName);
            if (!File.Exists(ignorePath))
            {
                return false;
            }

            var rules = LoadLocalRules(ignorePath, dirInfo.FullName);
            if (rules.Count == 0)
            {
                return false;
            }

            _localRulesStack.Add(rules);
            RebuildFilterRules();
            return true;
        }

        /// <summary>Pops local rules from stack if previously pushed, rebuilds effective rules.</summary>
        /// <param name="hadRules">True if EnterDirectory loaded rules.</param>
        public void ExitDirectory(bool hadRules)
        {
            if (!hadRules)
            {
                return;
            }

            if (_localRulesStack.Count == 0)
            {
                return;
            }

            _localRulesStack.RemoveAt(_localRulesStack.Count - 1);
            RebuildFilterRules();
        }

        private void RebuildFilterRules()
        {
            if (_baseFilterRules.Count == 0 && _localRulesStack.Count == 0)
            {
                _filterRules = null;
                return;
            }

            var rules = new GitIgnoreRules();
            if (_baseFilterRules.Count > 0)
            {
                rules.Add(_baseFilterRules);
            }

            foreach (var stackRules in _localRulesStack)
            {
                rules.Add(stackRules);
            }

            _filterRules = rules;
        }

        private List<string> LoadLocalRules(string filePath, string directoryPath)
        {
            var rules = new List<string>();
            var relativeDir = NormalizeRelativeDirectory(Path.GetRelativePath(_rootPath, directoryPath));
            var lines = File.ReadAllLines(filePath);

            foreach (var line in lines)
            {
                var trimmed = line?.Trim();
                if (string.IsNullOrEmpty(trimmed))
                {
                    continue;
                }

                if (trimmed.StartsWith("#", StringComparison.Ordinal))
                {
                    continue;
                }

                var isNegation = trimmed.StartsWith("!", StringComparison.Ordinal);
                var pattern = isNegation ? trimmed.Substring(1) : trimmed;
                if (string.IsNullOrEmpty(pattern))
                {
                    continue;
                }

                foreach (var expanded in ExpandPattern(pattern, relativeDir))
                {
                    var finalRule = isNegation ? "!" + expanded : expanded;
                    rules.Add(finalRule);
                }
            }

            return rules;
        }

        private static string NormalizeRelativeDirectory(string relativeDir)
        {
            if (string.IsNullOrWhiteSpace(relativeDir) || relativeDir == ".")
            {
                return string.Empty;
            }

            return relativeDir.Replace('\\', '/').Trim('/');
        }

        private static IEnumerable<string> ExpandPattern(string pattern, string relativeDir)
        {
            var normalized = pattern.Replace('\\', '/');
            var hasPrefix = !string.IsNullOrEmpty(relativeDir);
            var prefix = hasPrefix ? relativeDir.TrimEnd('/') + "/" : string.Empty;

            if (!hasPrefix)
            {
                yield return normalized;
                yield break;
            }

            if (normalized.StartsWith("/", StringComparison.Ordinal))
            {
                yield return prefix + normalized.Substring(1);
                yield break;
            }

            if (normalized.Contains("/", StringComparison.Ordinal))
            {
                yield return prefix + normalized;
                yield break;
            }

            // No slash in pattern: match any level under this directory.
            yield return prefix + normalized;
            yield return prefix + "**/" + normalized;
        }
    }
}


