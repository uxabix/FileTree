using System;
using System.IO;
using System.Linq;
using Xunit;
using FileTree.Core.Scanning;
using FileTree.Core.Models;

namespace FileTree.Tests.Scanning
{
    public class FileScannerTests : IDisposable
    {
        private readonly string _tempRoot;
        private readonly FileScanner _scanner;

        public FileScannerTests()
        {
            _tempRoot = Path.Combine(Path.GetTempPath(), "FileTreeTest_" + Guid.NewGuid());
            Directory.CreateDirectory(_tempRoot);
            
            _scanner = new FileScanner();
        }

        public void Dispose()
        {
            if (Directory.Exists(_tempRoot))
            {
                try { Directory.Delete(_tempRoot, true); } catch { }
            }
        }

        [Fact]
        public void Scan_ShouldFindFilesAndFolders()
        {
            File.WriteAllText(Path.Combine(_tempRoot, "file1.txt"), "dummy content");
            var subFolder = Path.Combine(_tempRoot, "folder1");
            Directory.CreateDirectory(subFolder);
            File.WriteAllText(Path.Combine(subFolder, "file2.txt"), "dummy content");

            var rootNode = _scanner.Scan(_tempRoot, 10);

            Assert.NotNull(rootNode);
            Assert.Equal(Path.GetFileName(_tempRoot), rootNode.Name);
            
            Assert.Equal(2, rootNode.Children.Count);

            var fileNode = rootNode.Children.FirstOrDefault(c => c.Name == "file1.txt");
            Assert.NotNull(fileNode);
            Assert.False(fileNode.IsDirectory);

            var folderNode = rootNode.Children.FirstOrDefault(c => c.Name == "folder1");
            Assert.NotNull(folderNode);
            Assert.True(folderNode.IsDirectory);

            Assert.Single(folderNode.Children); // В folder1 только 1 файл
            Assert.Equal("file2.txt", folderNode.Children.First().Name);
        }

        [Fact]
        public void Scan_ShouldRespectMaxDepth()
        {

            var level1 = Path.Combine(_tempRoot, "level1");
            var level2 = Path.Combine(level1, "level2");
            Directory.CreateDirectory(level2);
            File.WriteAllText(Path.Combine(level2, "file.txt"), "hi");

            var rootNode = _scanner.Scan(_tempRoot, 1);

            var level1Node = rootNode.Children.FirstOrDefault(c => c.Name == "level1");
            Assert.NotNull(level1Node);
            

            Assert.Empty(level1Node.Children); 
        }

        [Fact]
        public void Scan_ShouldThrow_WhenDirectoryNotFound()
        {
            string nonExistentPath = Path.Combine(_tempRoot, "GHOST_FOLDER");

            Assert.Throws<DirectoryNotFoundException>(() => 
            {
                _scanner.Scan(nonExistentPath, 5);
            });
        }
    }
}