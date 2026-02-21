namespace FileTree.Core.Filtering;

internal interface IFileFilter
{
    bool ShouldIncludeFile(string fileName, string fullPath);
    bool ShouldIncludeDirectory(string directoryName, string fullPath);
}
