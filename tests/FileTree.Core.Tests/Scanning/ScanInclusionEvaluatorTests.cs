using Xunit;
using System.IO;
using FileTree.Core.Scanning;
using FileTree.Core.Models;
using FileTree.Core.Tests.Fixtures;
using FileTree.Core.GitIgnore;
using System.Collections.Generic;

namespace FileTree.Core.Tests.Scanning
{
    public class ScanInclusionEvaluatorTests : IClassFixture<TempDirectoryFixture>
    {
        private readonly TempDirectoryFixture _fixture;

        public ScanInclusionEvaluatorTests(TempDirectoryFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void ShouldInclude_WithDefaultOptions_IncludesAll()
        {
            _fixture.CreateFile("test.txt");
            _fixture.CreateDirectory("dir");

            var options = new FileTreeOptions();
            var evaluator = new ScanInclusionEvaluator(_fixture.RootPath, options, null);

            var fileInfo = new FileInfo(Path.Combine(_fixture.RootPath, "test.txt"));
            var dirInfo = new DirectoryInfo(Path.Combine(_fixture.RootPath, "dir"));

            Assert.True(evaluator.ShouldInclude(fileInfo, 0, 0));
            Assert.True(evaluator.ShouldInclude(dirInfo, 0, 0));
        }

        [Fact]
        public void ShouldInclude_SkipHidden_ExcludesHidden()
        {
            _fixture.CreateFile(".hiddenfile");
            var options = new FileTreeOptions { SkipHidden = true };
            var evaluator = new ScanInclusionEvaluator(_fixture.RootPath, options, null);
            var fileInfo = new FileInfo(Path.Combine(_fixture.RootPath, ".hiddenfile"));

            Assert.False(evaluator.ShouldInclude(fileInfo, 0, 0));
        }

        [Fact]
        public void ShouldInclude_WithGitIgnore_ExcludesIgnored()
        {
            _fixture.CreateFile("ignored.log");
            var gitignore = GitIgnoreParser.FromLines(new[] { "*.log" });
            var options = new FileTreeOptions { UseGitIgnore = true };
            var evaluator = new ScanInclusionEvaluator(_fixture.RootPath, options, gitignore);
            var fileInfo = new FileInfo(Path.Combine(_fixture.RootPath, "ignored.log"));

            Assert.False(evaluator.ShouldInclude(fileInfo, 0, 0));
        }

        [Fact]
        public void ShouldInclude_WithFileFilter_ExcludesFiltered()
        {
            _fixture.CreateFile("test.tmp");
            var options = new FileTreeOptions
            {
                Filter = new FilterOptions { ExcludeExtensions = new List<string> { ".tmp" } }
            };
            var evaluator = new ScanInclusionEvaluator(_fixture.RootPath, options, null);
            var fileInfo = new FileInfo(Path.Combine(_fixture.RootPath, "test.tmp"));

            Assert.False(evaluator.ShouldInclude(fileInfo, 0, 0));
        }

        [Fact]
        public void ShouldInclude_MaxDepth_ExcludesDeeperItems()
        {
            _fixture.CreateFile("dir1/dir2/test.txt");
            var options = new FileTreeOptions { MaxDepth = 2 };
            var evaluator = new ScanInclusionEvaluator(_fixture.RootPath, options, null);
            var fileInfo = new FileInfo(Path.Combine(_fixture.RootPath, "dir1/dir2/test.txt"));

            // Depth of dir1 is 0, dir2 is 1, test.txt is 2.
            // The check is for the *item's* depth, which is currentDepth.
            // So, for test.txt, currentDepth would be 2.
            Assert.False(evaluator.ShouldInclude(fileInfo, 2, 0));
        }
    }
}
