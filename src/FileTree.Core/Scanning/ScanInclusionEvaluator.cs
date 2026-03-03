using System;
using System.IO;
using System.Runtime.InteropServices;
using FileTree.Core.Filtering;
using FileTree.Core.GitIgnore;
using FileTree.Core.Models;

namespace FileTree.Core.Scanning
{
    internal class ScanInclusionEvaluator
    {
        private readonly GitIgnoreRules? _gitIgnoreRules;
        private readonly GitIgnoreRules? _filterRules;
        private readonly IFileFilter? _legacyFileFilter;
        private readonly string _rootPath;
        private readonly FileTreeOptions _options;

        /// <summary>
        /// Creates a new instance of ScanInclusionEvaluator.
        /// </summary>
        /// <param name="rootPath">The root directory path being scanned.</param>
        /// <param name="options">File tree scanning options.</param>
        /// <param name="gitIgnoreRules">Optional .gitignore rules to apply.</param>
        /// <param name="filterRules">Optional custom filter rules to apply.</param>
        public ScanInclusionEvaluator(
            string rootPath,
            FileTreeOptions options,
            GitIgnoreRules? gitIgnoreRules,
            GitIgnoreRules? filterRules = null)
        {
            _rootPath = Path.GetFullPath(rootPath);
            _options = options;
            _gitIgnoreRules = gitIgnoreRules;
            _filterRules = filterRules;

            // For backward compatibility, create legacy file filter if no new-style filter rules provided
            // and legacy filter options are present
            if (_filterRules == null && LegacyFilterConverter.HasLegacyFilters(options.Filter))
            {
                _legacyFileFilter = new FileFilter(options.Filter);
            }
        }

        private bool IsHidden(FileSystemInfo item)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && item.Attributes.HasFlag(FileAttributes.Hidden))
                return true;

            if (item.Name.StartsWith("."))
                return true;

            return false;
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
            if (_options.SkipHidden && IsHidden(item))
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
    }
}
