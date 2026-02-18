using FileTree.Core.Abstractions;
using System;
using System.IO;
using System.Linq;
using FileTree.Core.Models;
using FileTree.Core.GitIgnore;
using System.Runtime.InteropServices;
using FileTree.Core.Filtering;


namespace FileTree.Core.Scanning
{
    internal class FileScanner : IFileScanner
    {
        private int _nodeCount;
        private GitIgnoreRules? _gitIgnore;
        private IFileFilter? _fileFilter;
        private bool _ignoreEmptyFolders;
        private string _rootPath = string.Empty;

        public FileNode Scan(string rootPath, FileTreeOptions options)
        {
            if (!Directory.Exists(rootPath))
                throw new DirectoryNotFoundException(rootPath);

            _rootPath = Path.GetFullPath(rootPath);
            _nodeCount = 0;

            _gitIgnore = null;
            if (options.UseGitIgnore)
            {
                string gitIgnorePath = Path.Combine(_rootPath, ".gitignore");
                if (File.Exists(gitIgnorePath))
                {
                    _gitIgnore = GitIgnoreParser.FromFile(gitIgnorePath);
                }
            }

            _fileFilter = new FileFilter(options.Filter);
            _ignoreEmptyFolders = options.Filter.IgnoreEmptyFolders;

            var rootInfo = new DirectoryInfo(_rootPath);
            var rootNode = new FileNode(rootInfo.Name, rootInfo.FullName, true);

            PerformScan(rootInfo, rootNode, 0, options);
            return rootNode;
        }

        private bool IsHidden(FileSystemInfo item)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && item.Attributes.HasFlag(FileAttributes.Hidden))
                return true;

            if (item.Name.StartsWith("."))
                return true;

            return false;
        }


        private void PerformScan(DirectoryInfo dirInfo, FileNode parentNode, int currentDepth, FileTreeOptions options)
        {
            if (options.MaxDepth != -1 && currentDepth >= options.MaxDepth)
                return;

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

                if (options.SkipHidden && IsHidden(item))
                    continue;

                if (item.Attributes.HasFlag(FileAttributes.ReparsePoint))
                    continue;

                if (_gitIgnore != null)
                {
                    var relativePath = Path.GetRelativePath(_rootPath, item.FullName).Replace('\\', '/');
                    if (item is DirectoryInfo)
                        relativePath += "/";

                    if (_gitIgnore.IsIgnored(relativePath))
                        continue;
                }

                bool isDir = item is DirectoryInfo;

                // Apply filter
                if (_fileFilter != null)
                {
                    if (isDir)
                    {
                        if (!_fileFilter.ShouldIncludeDirectory(item.Name, item.FullName))
                            continue;
                    }
                    else
                    {
                        if (!_fileFilter.ShouldIncludeFile(item.Name, item.FullName))
                            continue;
                    }
                }

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
