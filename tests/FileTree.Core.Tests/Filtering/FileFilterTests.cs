using Xunit;
using FileTree.Core.Filtering;
using FileTree.Core.Models;
using System.Collections.Generic;

namespace FileTree.Core.Tests.Filtering
{
    public class FileFilterTests
    {
        [Fact]
        public void ShouldIncludeFile_WithNoFilters_ReturnsTrue()
        {
            var options = new FilterOptions();
            var filter = new FileFilter(options);
            Assert.True(filter.ShouldIncludeFile("test.txt", "/path/to/test.txt"));
        }

        [Fact]
        public void ShouldIncludeFile_MatchesIncludeExtension_ReturnsTrue()
        {
            var options = new FilterOptions { IncludeExtensions = new List<string> { ".txt" } };
            var filter = new FileFilter(options);
            Assert.True(filter.ShouldIncludeFile("test.txt", "/path/to/test.txt"));
        }

        [Fact]
        public void ShouldIncludeFile_DoesNotMatchIncludeExtension_ReturnsFalse()
        {
            var options = new FilterOptions { IncludeExtensions = new List<string> { ".jpg" } };
            var filter = new FileFilter(options);
            Assert.False(filter.ShouldIncludeFile("test.txt", "/path/to/test.txt"));
        }

        [Fact]
        public void ShouldIncludeFile_MatchesExcludeExtension_ReturnsFalse()
        {
            var options = new FilterOptions { ExcludeExtensions = new List<string> { ".tmp" } };
            var filter = new FileFilter(options);
            Assert.False(filter.ShouldIncludeFile("test.tmp", "/path/to/test.tmp"));
        }

        [Fact]
        public void ShouldIncludeFile_MatchesExcludeName_ReturnsFalse()
        {
            var options = new FilterOptions { ExcludeNames = new List<string> { "temp.txt" } };
            var filter = new FileFilter(options);
            Assert.False(filter.ShouldIncludeFile("temp.txt", "/path/to/temp.txt"));
        }

        [Fact]
        public void ShouldIncludeFile_MatchesBothIncludeExtensionAndExcludeName_ReturnsFalse()
        {
            var options = new FilterOptions
            {
                IncludeExtensions = new List<string> { ".txt" },
                ExcludeNames = new List<string> { "temp.txt" }
            };
            var filter = new FileFilter(options);
            Assert.False(filter.ShouldIncludeFile("temp.txt", "/path/to/temp.txt"));
        }

        [Fact]
        public void ShouldIncludeDirectory_WithNoFilters_ReturnsTrue()
        {
            var options = new FilterOptions();
            var filter = new FileFilter(options);
            Assert.True(filter.ShouldIncludeDirectory("test_dir", "/path/to/test_dir"));
        }

        [Fact]
        public void ShouldIncludeDirectory_MatchesExcludeName_ReturnsFalse()
        {
            var options = new FilterOptions { ExcludeNames = new List<string> { "bin", "obj" } };
            var filter = new FileFilter(options);
            Assert.False(filter.ShouldIncludeDirectory("bin", "/path/to/bin"));
            Assert.False(filter.ShouldIncludeDirectory("obj", "/path/to/obj"));
        }

        [Fact]
        public void ShouldIncludeDirectory_DoesNotMatchExcludeName_ReturnsTrue()
        {
            var options = new FilterOptions { ExcludeNames = new List<string> { "bin", "obj" } };
            var filter = new FileFilter(options);
            Assert.True(filter.ShouldIncludeDirectory("src", "/path/to/src"));
        }
    }
}
