using FileTree.Core.Abstractions;
using System;
using System.IO;
using System.Linq;
using FileTree.Core.Models;
using FileTree.Core.GitIgnore;
using FileTree.Core.Filtering;


namespace FileTree.Core.Scanning
{
    internal class FileScanner : IFileScanner
    {
        private int _nodeCount;
        private bool _ignoreEmptyFolders;
        private ScanInclusionEvaluator? _inclusionEvaluator;

        public FileNode Scan(string rootPath, FileTreeOptions options)
        {
            if (!Directory.Exists(rootPath))
                throw new DirectoryNotFoundException(rootPath);

            string fullRootPath = Path.GetFullPath(rootPath);
            _nodeCount = 0;

            GitIgnoreRules? gitIgnore = null;
            if (options.UseGitIgnore)
            {
                string gitIgnorePath = Path.Combine(fullRootPath, ".gitignore");
                if (File.Exists(gitIgnorePath))
                {
                    gitIgnore = GitIgnoreParser.FromFile(gitIgnorePath);
                }
            }

            _inclusionEvaluator = new ScanInclusionEvaluator(fullRootPath, options, gitIgnore);
            _ignoreEmptyFolders = options.Filter.IgnoreEmptyFolders;

            var rootInfo = new DirectoryInfo(fullRootPath);
            var rootNode = new FileNode(rootInfo.Name, rootInfo.FullName, true);

            PerformScan(rootInfo, rootNode, 0, options);
            return rootNode;
        }

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

            if (options.MaxWidth != -1)
                items = items.Take(options.MaxWidth).ToArray();

            foreach (var item in items)
            {
                if (options.MaxNodes != -1 && _nodeCount >= options.MaxNodes)
                    break;

                if (_inclusionEvaluator == null)
                    throw new InvalidOperationException("ScanInclusionEvaluator was not initialized.");

                if (!_inclusionEvaluator.ShouldInclude(item, currentDepth, _nodeCount))
                    continue;

                bool isDir = item is DirectoryInfo;

                var node = new FileNode(item.Name, item.FullName, isDir);

                parentNode.AddChild(node);
                _nodeCount++;

                if (isDir)
                {
                    PerformScan((DirectoryInfo)item, node, currentDepth + 1, options);

                    // Remove empty folders if IgnoreEmptyFolders is enabled
                    if (_ignoreEmptyFolders && !node.Children.Any())
                    {
                        parentNode.RemoveChild(node);
                        _nodeCount--;
                    }
                }
            }
        }
    }
}
