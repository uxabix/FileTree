using System;
using System.IO;
using System.Linq;
using Xunit;
using FileTree.Core.Scanning;
using FileTree.Core.Models;

namespace FileTree.Core.Tests.Scanning
{
    public class FileScannerTests : IDisposable
    {
        private readonly string _tempRoot;

        public FileScannerTests()
        {
            _tempRoot = Path.Combine(Path.GetTempPath(), "FileTreeTest_" + Guid.NewGuid());
            Directory.CreateDirectory(_tempRoot);
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
            // Arrange
            File.WriteAllText(Path.Combine(_tempRoot, "file1.txt"), "dummy content");
            var subFolder = Path.Combine(_tempRoot, "folder1");
            Directory.CreateDirectory(subFolder);
            File.WriteAllText(Path.Combine(subFolder, "file2.txt"), "dummy content");

            var scanner = new FileScanner(); // без ограничений

            // Act
            var rootNode = scanner.Scan(_tempRoot);

            // Assert
            Assert.NotNull(rootNode);
            Assert.Equal(Path.GetFileName(_tempRoot), rootNode.Name);
            Assert.Equal(2, rootNode.Children.Count);

            var fileNode = rootNode.Children.FirstOrDefault(c => c.Name == "file1.txt");
            Assert.NotNull(fileNode);
            Assert.False(fileNode.IsDirectory);

            var folderNode = rootNode.Children.FirstOrDefault(c => c.Name == "folder1");
            Assert.NotNull(folderNode);
            Assert.True(folderNode.IsDirectory);

            Assert.Single(folderNode.Children);
            Assert.Equal("file2.txt", folderNode.Children.First().Name);
        }

        [Fact]
        public void Scan_ShouldRespectMaxDepth()
        {
            // Arrange
            var level1 = Path.Combine(_tempRoot, "level1");
            var level2 = Path.Combine(level1, "level2");
            Directory.CreateDirectory(level2);
            File.WriteAllText(Path.Combine(level2, "file.txt"), "hi");

            var scanner = new FileScanner { MaxDepth = 1 };

            // Act
            var rootNode = scanner.Scan(_tempRoot);

            // Assert
            var level1Node = rootNode.Children.FirstOrDefault(c => c.Name == "level1");
            Assert.NotNull(level1Node);
            Assert.Empty(level1Node.Children);
        }

        [Fact]
        public void Scan_ShouldThrow_WhenDirectoryNotFound()
        {
            // Arrange
            string nonExistentPath = Path.Combine(_tempRoot, "GHOST_FOLDER");
            var scanner = new FileScanner();

            // Act & Assert
            Assert.Throws<DirectoryNotFoundException>(() => scanner.Scan(nonExistentPath));
        }
    }
}
