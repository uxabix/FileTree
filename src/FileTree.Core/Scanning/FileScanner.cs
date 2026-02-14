using System;
using System.IO;
using FileTree.Core.Models;

namespace FileTree.Core.Scanning
{
    internal class FileScanner : IFileScanner
    {
        public FileNode Scan(string rootPath, int maxDepth)
        {
            if (!Directory.Exists(rootPath))
                throw new DirectoryNotFoundException(rootPath);

            var rootInfo = new DirectoryInfo(rootPath);
            var rootNode = new FileNode(rootInfo.Name, rootInfo.FullName, true);

            PerformScan(rootInfo, rootNode, 0, maxDepth);
            return rootNode;
        }

        private void PerformScan(DirectoryInfo dirInfo, FileNode parentNode, int currentDepth, int maxDepth)
        {
            if (maxDepth != -1 && currentDepth >= maxDepth) return;

            try
            {
                foreach (var item in dirInfo.GetFileSystemInfos())
                {
                    if (item.Attributes.HasFlag(FileAttributes.ReparsePoint)) continue;

                    bool isDir = item is DirectoryInfo;
                    var node = new FileNode(item.Name, item.FullName, isDir);
                    parentNode.AddChild(node);

                    if (isDir)
                    {
                        PerformScan((DirectoryInfo)item, node, currentDepth + 1, maxDepth);
                    }
                }
            }
            catch (UnauthorizedAccessException) { }
        }
    }
}