using FileTree.Core.Models;

namespace FileTree.Core.Services;

public interface IFileTreeServise
{
    string Generate(string rootPath, FileTreeOptions options);
}
