using System;
using System.IO;
using FileTree.Core.Models;

namespace FileTree.Core.Scanning
{
    internal class FileScanner : IFileScanner
    {
        public int MaxDepth { get; init; } = -1;
        public int MaxWidth { get; init; } = -1;
        public int MaxNodes { get; init; } = -1;

        private int _nodeCount;
        private bool _stopScan;

        public FileNode Scan(string rootPath)
        {
            if (!Directory.Exists(rootPath))
                throw new DirectoryNotFoundException(rootPath);

            var rootInfo = new DirectoryInfo(rootPath);
            var rootNode = new FileNode(rootInfo.Name, rootInfo.FullName, true);

            _nodeCount = 1; // корневой узел
            _stopScan = false;

            PerformScan(rootInfo, rootNode, 0);
            return rootNode;
        }

        private void PerformScan(DirectoryInfo dirInfo, FileNode parentNode, int currentDepth)
        {
            if (_stopScan) return;
            if (MaxDepth != -1 && currentDepth >= MaxDepth) return;

            try
            {
                int childrenAdded = 0;
                foreach (var item in dirInfo.GetFileSystemInfos())
                {
                    if (_stopScan) break;

                    if (MaxWidth != -1 && childrenAdded >= MaxWidth)
                        break;

                    if (MaxNodes != -1 && _nodeCount >= MaxNodes)
                    {
                        _stopScan = true;
                        break;
                    }

                    if (item.Attributes.HasFlag(FileAttributes.ReparsePoint))
                        continue;

                    bool isDir = item is DirectoryInfo;
                    var node = new FileNode(item.Name, item.FullName, isDir);
                    parentNode.AddChild(node);
                    _nodeCount++;
                    childrenAdded++;

                    if (isDir)
                    {
                        PerformScan((DirectoryInfo)item, node, currentDepth + 1);
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {

            }
        }
    }
}
