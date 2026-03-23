using System;
using System.IO;

namespace FileTree.Core.Scanning
{
    /// <summary>
    /// Cross-platform detector for hidden files and directories.
    /// On Windows, checks the Hidden file attribute.
    /// On Unix-like systems, checks for dot-prefix in name (e.g., .git).
    /// </summary>
    internal static class HiddenFileDetector
    {
        /// <summary>
        /// Determines if the specified file system item is hidden.
        /// </summary>
        /// <param name="item">The file or directory information to check.</param>
        /// <returns>
        /// <c>true</c> if the item is hidden; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// Platform-specific logic:
        /// - Windows: Checks <see cref="FileAttributes.Hidden"/>.
        /// - Other OS: Checks if name starts with &apos;.&apos;.
        /// </remarks>
        internal static bool IsHidden(FileSystemInfo item)
        {
            if (OperatingSystem.IsWindows())
                return item.Attributes.HasFlag(FileAttributes.Hidden);

            return item.Name.StartsWith(".", StringComparison.Ordinal);
        }
    }
}
