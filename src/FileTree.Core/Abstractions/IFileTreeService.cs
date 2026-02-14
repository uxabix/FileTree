using FileTree.Core.Models;

namespace FileTree.Core.Abstractions;

public interface IFileTreeService
{
    string Generate(string rootPath, FileTreeOptions options);
}