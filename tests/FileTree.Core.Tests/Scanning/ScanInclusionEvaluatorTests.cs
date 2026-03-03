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
        public void ShouldInclude_WithLegacyFileFilter_ExcludesFiltered()
        {
            _fixture.CreateFile("test.tmp");
            var options = new FileTreeOptions
            {
                #pragma warning disable CS0618 // Type or member is obsolete
                Filter = new FilterOptions { ExcludeExtensions = new List<string> { ".tmp" } }
                #pragma warning restore CS0618
            };
            var evaluator = new ScanInclusionEvaluator(_fixture.RootPath, options, null);
            var fileInfo = new FileInfo(Path.Combine(_fixture.RootPath, "test.tmp"));

            Assert.False(evaluator.ShouldInclude(fileInfo, 0, 0));
        }

        [Fact]
        public void ShouldInclude_WithFilterRules_ExcludesFiltered()
        {
            _fixture.CreateFile("test.log");
            var filterRules = GitIgnoreParser.FromLines(new[] { "*.log" });
            var options = new FileTreeOptions();
            var evaluator = new ScanInclusionEvaluator(_fixture.RootPath, options, null, filterRules);
            var fileInfo = new FileInfo(Path.Combine(_fixture.RootPath, "test.log"));

            Assert.False(evaluator.ShouldInclude(fileInfo, 0, 0));
        }

        [Fact]
        public void ShouldInclude_WithFilterRulesAndGitIgnore_BothApplied()
        {
            _fixture.CreateFile("test.log");
            _fixture.CreateFile("test.tmp");

            var gitIgnoreRules = GitIgnoreParser.FromLines(new[] { "*.log" });
            var filterRules = GitIgnoreParser.FromLines(new[] { "*.tmp" });
            var options = new FileTreeOptions { UseGitIgnore = true };
            var evaluator = new ScanInclusionEvaluator(_fixture.RootPath, options, gitIgnoreRules, filterRules);

            var logFile = new FileInfo(Path.Combine(_fixture.RootPath, "test.log"));
            var tmpFile = new FileInfo(Path.Combine(_fixture.RootPath, "test.tmp"));
            var txtFile = new FileInfo(Path.Combine(_fixture.RootPath, "test.txt"));
            _fixture.CreateFile("test.txt");

            Assert.False(evaluator.ShouldInclude(logFile, 0, 0)); // Excluded by gitignore
            Assert.False(evaluator.ShouldInclude(tmpFile, 0, 0)); // Excluded by filter rules
            Assert.True(evaluator.ShouldInclude(txtFile, 0, 0));  // Not excluded
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
