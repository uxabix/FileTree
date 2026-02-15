using System;
using System.IO;
using System.Linq;
using FileTree.Core.Models;

namespace FileTree.Core.Scanning
{
    internal class FileScanner : IFileScanner
    {
        public int MaxDepth { get; init; }
        public int MaxWidth { get; init; }
        public int MaxNodes { get; init; }

        private int _nodeCount = 0;

        public FileScanner(FileTreeOptions options)
        {
            MaxDepth = options.MaxDepth;
            MaxWidth = options.MaxWidth;
            MaxNodes = options.MaxNodes;
        }

        public FileScanner()
        {
            MaxDepth = -1;
            MaxWidth = -1;
            MaxNodes = -1;
        }

        public FileNode Scan(string rootPath)
        {
            if (!Directory.Exists(rootPath))
                throw new DirectoryNotFoundException(rootPath);

            _nodeCount = 0;

            var rootInfo = new DirectoryInfo(rootPath);
            var rootNode = new FileNode(rootInfo.Name, rootInfo.FullName, true);

            PerformScan(rootInfo, rootNode, 0);

            return rootNode;
        }

        private void PerformScan(DirectoryInfo dirInfo, FileNode parentNode, int currentDepth)
        {
            if (MaxDepth != -1 && currentDepth >= MaxDepth)
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

            if (MaxWidth != -1)
                items = items.Take(MaxWidth).ToArray();

            foreach (var item in items)
            {
                if (item.Attributes.HasFlag(FileAttributes.ReparsePoint))
                    continue;

                if (MaxNodes != -1 && _nodeCount >= MaxNodes)
                    return;

                bool isDir = item is DirectoryInfo;
                var node = new FileNode(item.Name, item.FullName, isDir);
                parentNode.AddChild(node);

                _nodeCount++;

                if (isDir)
                {
                    PerformScan((DirectoryInfo)item, node, currentDepth + 1);
                }
            }
        }
    }
}
