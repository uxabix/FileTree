using System;
using System.IO;

namespace FileTree.Core.Scanning
{
    internal static class HiddenFileDetector
    {
        internal static bool IsHidden(FileSystemInfo item)
        {
            if (OperatingSystem.IsWindows())
                return item.Attributes.HasFlag(FileAttributes.Hidden);

            return item.Name.StartsWith(".", StringComparison.Ordinal);
        }
    }
}
