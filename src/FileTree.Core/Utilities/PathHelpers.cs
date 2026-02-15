using System;
using System.IO;

namespace FileTree.Core.Utilities;

public class PathHelpers
{
    public static string GetRelativePath(string rootPath, string fullPath)
    {
        return Path.GetRelativePath(rootPath, fullPath);
    }

    public static string GetName(string path)
    {
        return Path.GetFileName(path);
    }
}
