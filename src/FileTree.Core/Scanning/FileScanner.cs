using FileTree.Core.Abstractions;
using System;
using System.IO;
using System.Linq;
using FileTree.Core.Models;
using FileTree.Core.GitIgnore;


namespace FileTree.Core.Scanning
{
    internal class FileScanner : IFileScanner
    {
        private int _nodeCount;
        private GitIgnoreRules? _gitIgnore;

        public FileNode Scan(string rootPath, FileTreeOptions options)
        {
            if (!Directory.Exists(rootPath))
                throw new DirectoryNotFoundException(rootPath);

            _nodeCount = 0;

            _gitIgnore = null;
            if (options.UseGitIgnore)
            {
                string gitIgnorePath = Path.Combine(rootPath, ".gitignore");
                if (File.Exists(gitIgnorePath))
                {
                    _gitIgnore = GitIgnoreParser.FromFile(gitIgnorePath);
                }
            }

            var rootInfo = new DirectoryInfo(rootPath);
            var rootNode = new FileNode(rootInfo.Name, rootInfo.FullName, true);

            PerformScan(rootInfo, rootNode, 0, options);
            return rootNode;
        }

        private bool IsHidden(FileSystemInfo item)
        {
            if (item.Attributes.HasFlag(FileAttributes.Hidden))
                return true;

            //To do linux hiden files

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

                if (options.Hidden && IsHidden(item)) 
                    continue;

                if (item.Attributes.HasFlag(FileAttributes.ReparsePoint))
                    continue;

                if (_gitIgnore != null && _gitIgnore.IsIgnored(item.FullName))
                    continue;

                bool isDir = item is DirectoryInfo;
                var node = new FileNode(item.Name, item.FullName, isDir);

                parentNode.AddChild(node);
                _nodeCount++;

                if (isDir)
                {
                    PerformScan((DirectoryInfo)item, node, currentDepth + 1, options);
                }
            }
        }
    }
}
