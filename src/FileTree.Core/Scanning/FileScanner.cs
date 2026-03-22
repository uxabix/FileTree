using FileTree.Core.Abstractions;
using System;
using System.IO;
using System.Linq;
using FileTree.Core.Models;
using FileTree.Core.GitIgnore;
using FileTree.Core.Filtering;


namespace FileTree.Core.Scanning
{
/// <summary>
/// Implements tree scanning with limits, filtering, gitignore.
/// Recursive, depth/node aware.
/// </summary>
/// <summary>
/// Default implementation of <see cref="IFileScanner"/>. Handles recursive directory scanning with support for
/// depth/width/node limits, .gitignore rules, custom filters (legacy/new), hidden files, empty folder pruning.
/// Uses stack-based Enter/Exit for local .filetreeignore inheritance.
/// </summary>
internal class FileScanner : IFileScanner
    {
        /// <summary>Total nodes scanned (for MaxNodes limit).</summary>
        private int _nodeCount;
        /// <summary>Whether to prune empty folders post-scan.</summary>
        private bool _ignoreEmptyFolders;
        /// <summary>Evaluator for inclusion rules (limits, filters, gitignore).</summary>
        private ScanInclusionEvaluator? _inclusionEvaluator;

    /// <summary>
    /// Scans the directory tree starting from the specified root path.
    /// </summary>
    /// <param name="rootPath">Validated full path to the root directory (caller must validate existence).</param>
    /// <param name="options">Scanning options.</param>
    /// <returns>FileNode representing the scanned tree.</returns>
    public FileNode Scan(string rootPath, FileTreeOptions options)
    {
        string fullRootPath = Path.GetFullPath(rootPath);
        _nodeCount = 0;

        // Load .gitignore rules if configured
        GitIgnoreRules? gitIgnore = null;
        if (options.UseGitIgnore)
        {
            string gitIgnorePath = Path.Combine(fullRootPath, ".gitignore");
            if (File.Exists(gitIgnorePath))
            {
                gitIgnore = GitIgnoreParser.FromFile(gitIgnorePath);
            }
        }

            // Load custom filter rules
            List<string> filterRules = new();

            // Load new-style filtering if configured
            if (options.Filter.RulesSource != null)
            {
                var loader = new FilterRulesLoader();
                filterRules.AddRange(loader.LoadFilterRules(options.Filter.RulesSource));
            }

            // Merge legacy filters (if any) into gitignore-style rules for backward compatibility
            if (LegacyFilterConverter.HasLegacyFilters(options.Filter))
            {
                var legacyRules = LegacyFilterConverter.ConvertToGitIgnoreRules(options.Filter);
                filterRules.AddRange(legacyRules);
            }

            _inclusionEvaluator = new ScanInclusionEvaluator(fullRootPath, options, gitIgnore, filterRules);
            _ignoreEmptyFolders = options.Filter.IgnoreEmptyFolders;

            var rootInfo = new DirectoryInfo(fullRootPath);
            var rootNode = new FileNode(rootInfo.Name, rootInfo.FullName, true, HiddenFileDetector.IsHidden(rootInfo));

            var rootPushed = _inclusionEvaluator.EnterDirectory(rootInfo);
            PerformScan(rootInfo, rootNode, 0, options);
            _inclusionEvaluator.ExitDirectory(rootPushed);
            return rootNode;
        }

        /// <summary>
        /// Recursively scans directory contents, applying inclusion rules and limits.
        /// Builds child FileNode, recurses dirs, prunes empty if configured.
        /// </summary>
        /// <param name="dirInfo">Current directory.</param>
        /// <param name="parentNode">Parent node to add children to.</param>
        /// <param name="currentDepth">Recursion depth.</param>
        /// <param name="options">Scan options.</param>
        private void PerformScan(DirectoryInfo dirInfo, FileNode parentNode, int currentDepth, FileTreeOptions options)
        {
            // MaxDepth and MaxNodes checks are now handled by ScanInclusionEvaluator,
            // but we keep the MaxNodes check here to break the loop early if needed.
            if (options.MaxNodes != -1 && _nodeCount >= options.MaxNodes)
                return;

            FileSystemInfo[] items;

            try
            {
                items = dirInfo.GetFileSystemInfos();
            }
            catch (UnauthorizedAccessException)
            {
                return;
            }

            int visibleCount = 0;

            foreach (var item in items)
            {
                if (options.MaxNodes != -1 && _nodeCount >= options.MaxNodes)
                    break;

                if (_inclusionEvaluator == null)
                    throw new InvalidOperationException("ScanInclusionEvaluator was not initialized.");

                if (!_inclusionEvaluator.ShouldInclude(item, currentDepth, _nodeCount))
                    continue;

                if (options.MaxWidth != -1 && visibleCount >= options.MaxWidth)
                    break;

                bool isDir = item is DirectoryInfo;

                var node = new FileNode(item.Name, item.FullName, isDir, HiddenFileDetector.IsHidden(item));

                parentNode.AddChild(node);
                _nodeCount++;

                if (isDir)
                {
                    dirInfo = (DirectoryInfo)item;
                    var pushed = _inclusionEvaluator.EnterDirectory(dirInfo);
                    PerformScan(dirInfo, node, currentDepth + 1, options);
                    _inclusionEvaluator.ExitDirectory(pushed);

                    // Remove empty folders if IgnoreEmptyFolders is enabled
                    if (_ignoreEmptyFolders && !node.Children.Any())
                    {
                        parentNode.RemoveChild(node);
                        _nodeCount--;
                        continue;
                    }
                }

                visibleCount++;
                if (options.MaxWidth != -1 && visibleCount >= options.MaxWidth)
                    break;
            }
        }
    }
}
