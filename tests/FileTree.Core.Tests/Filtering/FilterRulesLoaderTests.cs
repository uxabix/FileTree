using System;
using System.IO;
using Xunit;
using FileTree.Core.Filtering;
using FileTree.Core.Models;
using FileTree.Core.GitIgnore;

namespace FileTree.Core.Tests.Filtering;

/// <summary>
/// Tests for the FilterRulesLoader class.
/// </summary>
public class FilterRulesLoaderTests
{
    [Fact]
    public void LoadFilterRules_WithNullSource_ThrowsArgumentNullException()
    {
        // Arrange
        var loader = new FilterRulesLoader();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => loader.LoadFilterRules(null!));
    }

    [Fact]
    public void LoadFilterRules_WithEmptySource_ReturnsEmptyRules()
    {
        // Arrange
        var loader = new FilterRulesLoader();
        var source = new FilterRulesSource
        {
            UseDefaultGlobalConfig = false
        };

        // Act
        var rules = loader.LoadFilterRules(source);

        // Assert
        Assert.NotNull(rules);
    }

    [Fact]
    public void LoadFilterRules_WithInlineRules_AppliesRulesCorrectly()
    {
        // Arrange
        var loader = new FilterRulesLoader();
        var source = new FilterRulesSource
        {
            InlineRules = new List<string> { "*.log", "bin/" },
            UseDefaultGlobalConfig = false
        };

        // Act
        var rules = loader.LoadFilterRules(source);
        var ignore = GitIgnoreParser.FromLines(rules);

        // Assert
        Assert.NotNull(rules);
        Assert.True(ignore.IsIgnored("test.log"));
        Assert.True(ignore.IsIgnored("bin/"));
    }

    [Fact]
    public void LoadFilterRules_WithNonExistentFile_ThrowsFileNotFoundException()
    {
        // Arrange
        var loader = new FilterRulesLoader();
        var source = new FilterRulesSource
        {
            LocalConfigPath = "/path/to/nonexistent/file.txt",
            UseDefaultGlobalConfig = false
        };

        // Act & Assert
        Assert.Throws<FileNotFoundException>(() => loader.LoadFilterRules(source));
    }

    [Fact]
    public void LoadFilterRules_WithValidFile_LoadsRulesFromFile()
    {
        // Arrange
        var loader = new FilterRulesLoader();
        var tempFile = Path.GetTempFileName();

        try
        {
            // Write test rules to temporary file
            File.WriteAllText(tempFile, "*.tmp\n# Comment\nnode_modules/");

            var source = new FilterRulesSource
            {
                LocalConfigPath = tempFile,
                UseDefaultGlobalConfig = false
            };

            // Act
            var rules = loader.LoadFilterRules(source);
            var ignore = GitIgnoreParser.FromLines(rules);

            // Assert
            Assert.NotNull(rules);
            Assert.True(ignore.IsIgnored("file.tmp"));
            Assert.True(ignore.IsIgnored("node_modules/"));
            Assert.False(ignore.IsIgnored("file.txt"));
        }
        finally
        {
            // Cleanup
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public void LoadFilterRules_WithMultipleSources_AppliesInCorrectOrder()
    {
        // Arrange
        var loader = new FilterRulesLoader();
        var globalFile = Path.GetTempFileName();
        var localFile = Path.GetTempFileName();

        try
        {
            // Global config excludes *.log
            File.WriteAllText(globalFile, "*.log");

            // Local config includes important.log (negation)
            File.WriteAllText(localFile, "!important.log");

            var source = new FilterRulesSource
            {
                GlobalConfigPath = globalFile,
                LocalConfigPath = localFile,
                UseDefaultGlobalConfig = false
            };

            // Act
            var rules = loader.LoadFilterRules(source);
            var ignore = GitIgnoreParser.FromLines(rules);

            // Assert
            Assert.NotNull(rules);
            // Regular log files should be ignored
            Assert.True(ignore.IsIgnored("test.log"));
            // But important.log should be included (negation has higher precedence)
            Assert.False(ignore.IsIgnored("important.log"));
        }
        finally
        {
            // Cleanup
            if (File.Exists(globalFile))
                File.Delete(globalFile);
            if (File.Exists(localFile))
                File.Delete(localFile);
        }
    }

    [Fact]
    public void LoadFilterRules_WithInlineRulesOverridingFileRules_InlineRulesHaveHighestPrecedence()
    {
        // Arrange
        var loader = new FilterRulesLoader();
        var localFile = Path.GetTempFileName();

        try
        {
            // Local config excludes *.log
            File.WriteAllText(localFile, "*.log");

            var source = new FilterRulesSource
            {
                LocalConfigPath = localFile,
                InlineRules = new List<string> { "!critical.log" }, // Override with inline rule
                UseDefaultGlobalConfig = false
            };

            // Act
            var rules = loader.LoadFilterRules(source);
            var ignore = GitIgnoreParser.FromLines(rules);

            // Assert
            Assert.NotNull(rules);
            Assert.True(ignore.IsIgnored("test.log"));
            Assert.False(ignore.IsIgnored("critical.log")); // Inline negation overrides file rule
        }
        finally
        {
            // Cleanup
            if (File.Exists(localFile))
                File.Delete(localFile);
        }
    }

    [Fact]
    public void LoadFilterRules_WithCommentsAndEmptyLines_IgnoresCommentsAndEmptyLines()
    {
        // Arrange
        var loader = new FilterRulesLoader();
        var tempFile = Path.GetTempFileName();

        try
        {
            // Write test rules with comments and empty lines
            File.WriteAllText(tempFile, @"
# This is a comment
*.log

# Another comment
bin/

");

            var source = new FilterRulesSource
            {
                LocalConfigPath = tempFile,
                UseDefaultGlobalConfig = false
            };

            // Act
            var rules = loader.LoadFilterRules(source);
            var ignore = GitIgnoreParser.FromLines(rules);

            // Assert
            Assert.NotNull(rules);
            Assert.True(ignore.IsIgnored("test.log"));
            Assert.True(ignore.IsIgnored("bin/"));
        }
        finally
        {
            // Cleanup
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }
}
