using System.IO;
using System.Linq;
using Xunit;
using FileTree.Core.GitIgnore;

namespace FileTree.Core.GitIgnore.Tests
{
    public class GitIgnoreTests
    {
        [Fact]
        public void AddSingleRule_IsIgnored()
        {
            var rules = new GitIgnoreRules();
            rules.Add(".vs/");
            Assert.True(rules.IsIgnored(".vs/a.txt"));
        }

        [Fact]
        public void AddMultipleRules_Fluent_IsIgnored()
        {
            var rules = new GitIgnoreRules()
                .Add(".vs/")
                .Add(new[] { "*.user", "obj/*" });

            Assert.True(rules.IsIgnored("x.user"));
            Assert.True(rules.IsIgnored("obj/a.dll"));
            Assert.True(rules.IsIgnored(".vs/a.txt"));
        }

        [Fact]
        public void Filter_ReturnsOnlyNonIgnored()
        {
            var rules = new GitIgnoreRules()
                .Add(".vs/")
                .Add(new[] { "*.user", "obj/*" });

            var inputs = new[] { ".vs/a.txt", "x.user", "obj/a.dll", "src/main.cs" };
            var visible = rules.Filter(inputs).ToArray();

            Assert.Single(visible);
            Assert.Equal("src/main.cs", visible[0]);
        }

        [Fact]
        public void FromLines_ShouldCreateRules()
        {
            var lines = new[] { "# comment", ".vs/", "", " *.user " };
            var rules = GitIgnoreParser.FromLines(lines);

            Assert.True(rules.IsIgnored("x.user"));
            Assert.True(rules.IsIgnored(".vs/a.txt"));
        }

        [Fact]
        public void FromFile_ShouldCreateRules()
        {
            var temp = Path.GetTempFileName();
            File.WriteAllLines(temp, new[] { ".vs/", "*.user" });

            try
            {
                var rules = GitIgnoreParser.FromFile(temp);
                Assert.True(rules.IsIgnored(".vs/a.txt"));
                Assert.True(rules.IsIgnored("x.user"));
            }
            finally
            {
                File.Delete(temp);
            }
        }
    }
}
