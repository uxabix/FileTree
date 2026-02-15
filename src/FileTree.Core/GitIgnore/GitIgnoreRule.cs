using System;
using System.IO;

internal class GitIgnoreRule
{
    public string Pattern { get; }
    public bool IsDirectory { get; }
    public bool IsNegate { get; }

    public GitIgnoreRule(string pattern, bool isDirectory, bool isNegate = false)
    {
        Pattern = pattern;
        IsDirectory = isDirectory;
        IsNegate = isNegate;
    }

    // Проверка совпадения пути с правилом
    public bool Matches(string relativePath)
    {
        if (IsDirectory)
            return relativePath.StartsWith(Pattern + Path.DirectorySeparatorChar);

        return Path.GetFileName(relativePath) == Pattern;
    }
}
