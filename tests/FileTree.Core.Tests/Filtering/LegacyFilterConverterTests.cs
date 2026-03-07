using System.Collections.Generic;
using Xunit;
using FileTree.Core.Filtering;
using FileTree.Core.Models;

namespace FileTree.Core.Tests.Filtering;

/// <summary>
/// Tests for the LegacyFilterConverter class.
/// </summary>
public class LegacyFilterConverterTests
{
    [Fact]
    public void ConvertToGitIgnoreRules_WithNullOptions_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<System.ArgumentNullException>(() =>
            LegacyFilterConverter.ConvertToGitIgnoreRules(null!));
    }

    [Fact]
    public void ConvertToGitIgnoreRules_WithEmptyOptions_ReturnsEmptyList()
    {
        // Arrange
        var options = new FilterOptions();

        // Act
        #pragma warning disable CS0618 // Type or member is obsolete
        var rules = LegacyFilterConverter.ConvertToGitIgnoreRules(options);
        #pragma warning restore CS0618

        // Assert
        Assert.NotNull(rules);
        Assert.Empty(rules);
    }

    [Fact]
    public void ConvertToGitIgnoreRules_WithExcludeExtensions_GeneratesCorrectPatterns()
    {
        // Arrange
        var options = new FilterOptions();
        #pragma warning disable CS0618 // Type or member is obsolete
        options.ExcludeExtensions = new List<string> { ".log", ".tmp" };
        #pragma warning restore CS0618

        // Act
        var rules = LegacyFilterConverter.ConvertToGitIgnoreRules(options);

        // Assert
        Assert.Contains("**/*.log", rules);
        Assert.Contains("**/*.tmp", rules);
    }

    [Fact]
    public void ConvertToGitIgnoreRules_WithExcludeExtensionsWithoutDot_AddsDotPrefix()
    {
        // Arrange
        var options = new FilterOptions();
        #pragma warning disable CS0618 // Type or member is obsolete
        options.ExcludeExtensions = new List<string> { "log", "tmp" };
        #pragma warning restore CS0618

        // Act
        var rules = LegacyFilterConverter.ConvertToGitIgnoreRules(options);

        // Assert
        Assert.Contains("**/*.log", rules);
        Assert.Contains("**/*.tmp", rules);
    }

    [Fact]
    public void ConvertToGitIgnoreRules_WithExcludeNames_GeneratesCorrectPatterns()
    {
        // Arrange
        var options = new FilterOptions();
        #pragma warning disable CS0618 // Type or member is obsolete
        options.ExcludeNames = new List<string> { "bin", "obj", "node_modules" };
        #pragma warning restore CS0618

        // Act
        var rules = LegacyFilterConverter.ConvertToGitIgnoreRules(options);

        // Assert
        Assert.Contains("**/bin", rules);
        Assert.Contains("**/obj", rules);
        Assert.Contains("**/node_modules", rules);
    }

    [Fact]
    public void ConvertToGitIgnoreRules_WithIncludeExtensions_GeneratesNegationPatterns()
    {
        // Arrange
        var options = new FilterOptions();
        #pragma warning disable CS0618 // Type or member is obsolete
        options.IncludeExtensions = new List<string> { ".cs", ".txt" };
        #pragma warning restore CS0618

        // Act
        var rules = LegacyFilterConverter.ConvertToGitIgnoreRules(options);

        // Assert
        // Should exclude all, then include specific extensions
        Assert.Contains("*", rules);
        Assert.Contains("!**/*.cs", rules);
        Assert.Contains("!**/*.txt", rules);
        // Exclude all should come before inclusions
        Assert.True(rules.IndexOf("*") < rules.IndexOf("!**/*.cs"));
    }

    [Fact]
    public void ConvertToGitIgnoreRules_WithIncludeNames_GeneratesNegationPatterns()
    {
        // Arrange
        var options = new FilterOptions();
        #pragma warning disable CS0618 // Type or member is obsolete
        options.IncludeNames = new List<string> { "important.log", "README.md" };
        #pragma warning restore CS0618

        // Act
        var rules = LegacyFilterConverter.ConvertToGitIgnoreRules(options);

        // Assert
        Assert.Contains("!**/important.log", rules);
        Assert.Contains("!**/README.md", rules);
    }

    [Fact]
    public void ConvertToGitIgnoreRules_WithMixedOptions_GeneratesCorrectOrderAndPatterns()
    {
        // Arrange
        var options = new FilterOptions();
        #pragma warning disable CS0618 // Type or member is obsolete
        options.IncludeExtensions = new List<string> { ".cs" };
        options.ExcludeExtensions = new List<string> { ".tmp" };
        options.ExcludeNames = new List<string> { "bin", "obj" };
        options.IncludeNames = new List<string> { "important.tmp" };
        #pragma warning restore CS0618

        // Act
        var rules = LegacyFilterConverter.ConvertToGitIgnoreRules(options);

        // Assert
        Assert.NotEmpty(rules);

        // Should include all expected patterns
        Assert.Contains("*", rules); // Exclude all for IncludeExtensions
        Assert.Contains("!**/*.cs", rules); // Include .cs files
        Assert.Contains("**/*.tmp", rules); // Exclude .tmp files
        Assert.Contains("**/bin", rules); // Exclude bin
        Assert.Contains("**/obj", rules); // Exclude obj
        Assert.Contains("!**/important.tmp", rules); // Include important.tmp
    }

    [Fact]
    public void HasLegacyFilters_WithNullOptions_ReturnsFalse()
    {
        // Act
        var result = LegacyFilterConverter.HasLegacyFilters(null!);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void HasLegacyFilters_WithEmptyOptions_ReturnsFalse()
    {
        // Arrange
        var options = new FilterOptions();

        // Act
        var result = LegacyFilterConverter.HasLegacyFilters(options);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void HasLegacyFilters_WithIncludeExtensions_ReturnsTrue()
    {
        // Arrange
        var options = new FilterOptions();
        #pragma warning disable CS0618 // Type or member is obsolete
        options.IncludeExtensions = new List<string> { ".cs" };
        #pragma warning restore CS0618

        // Act
        var result = LegacyFilterConverter.HasLegacyFilters(options);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void HasLegacyFilters_WithExcludeExtensions_ReturnsTrue()
    {
        // Arrange
        var options = new FilterOptions();
        #pragma warning disable CS0618 // Type or member is obsolete
        options.ExcludeExtensions = new List<string> { ".log" };
        #pragma warning restore CS0618

        // Act
        var result = LegacyFilterConverter.HasLegacyFilters(options);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void HasLegacyFilters_WithIncludeNames_ReturnsTrue()
    {
        // Arrange
        var options = new FilterOptions();
        #pragma warning disable CS0618 // Type or member is obsolete
        options.IncludeNames = new List<string> { "README.md" };
        #pragma warning restore CS0618

        // Act
        var result = LegacyFilterConverter.HasLegacyFilters(options);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void HasLegacyFilters_WithExcludeNames_ReturnsTrue()
    {
        // Arrange
        var options = new FilterOptions();
        #pragma warning disable CS0618 // Type or member is obsolete
        options.ExcludeNames = new List<string> { "bin" };
        #pragma warning restore CS0618

        // Act
        var result = LegacyFilterConverter.HasLegacyFilters(options);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ConvertToGitIgnoreRules_WithWhitespaceInNames_IgnoresEmptyEntries()
    {
        // Arrange
        var options = new FilterOptions();
        #pragma warning disable CS0618 // Type or member is obsolete
        options.ExcludeNames = new List<string> { "bin", "", "  ", "obj" };
        #pragma warning restore CS0618

        // Act
        var rules = LegacyFilterConverter.ConvertToGitIgnoreRules(options);

        // Assert
        Assert.Contains("**/bin", rules);
        Assert.Contains("**/obj", rules);
        // Should not contain patterns for empty or whitespace-only names
        Assert.DoesNotContain("**/", rules.Where(r => r == "**/" || r == "**/ ").ToList());
    }
}
