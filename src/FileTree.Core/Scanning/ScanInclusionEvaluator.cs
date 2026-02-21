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
        private readonly GitIgnoreRules? _gitIgnore;
        private readonly IFileFilter _fileFilter;
        private readonly string _rootPath;
        private readonly FileTreeOptions _options;

        public ScanInclusionEvaluator(string rootPath, FileTreeOptions options, GitIgnoreRules? gitIgnore)
        {
            _rootPath = Path.GetFullPath(rootPath);
            _options = options;
            _gitIgnore = gitIgnore;
            _fileFilter = new FileFilter(options.Filter);
        }

        private bool IsHidden(FileSystemInfo item)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && item.Attributes.HasFlag(FileAttributes.Hidden))
                return true;

            if (item.Name.StartsWith("."))
                return true;

            return false;
        }

        public bool ShouldInclude(FileSystemInfo item, int currentDepth, int currentNodeCount)
        {
            if (_options.MaxDepth != -1 && currentDepth >= _options.MaxDepth)
                return false;

            if (_options.MaxNodes != -1 && currentNodeCount >= _options.MaxNodes)
                return false;

            if (_options.SkipHidden && IsHidden(item))
                return false;

            if (item.Attributes.HasFlag(FileAttributes.ReparsePoint))
                return false;

            if (_gitIgnore != null)
            {
                var relativePath = Path.GetRelativePath(_rootPath, item.FullName).Replace('\\', '/');
                if (item is DirectoryInfo)
                    relativePath += "/";

                if (_gitIgnore.IsIgnored(relativePath))
                    return false;
            }

            bool isDir = item is DirectoryInfo;

            // Apply filter
            if (isDir)
            {
                if (!_fileFilter.ShouldIncludeDirectory(item.Name, item.FullName))
                    return false;
            }
            else
            {
                if (!_fileFilter.ShouldIncludeFile(item.Name, item.FullName))
                    return false;
            }

            return true;
        }
    }
}
