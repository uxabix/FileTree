using FileTree.Core.Filtering;
using FileTree.Core.Models;
using FileTree.Core.Tests.Fixtures;
using Xunit.Abstractions;

namespace FileTree.Core.Tests.Filtering;

public class FilterEngineTests : IClassFixture<TestTreeFixture>
{
    private readonly TestTreeFixture _fixture;
    private readonly FilterEngine _engine;

    private readonly ITestOutputHelper _output;

    private void PrintTree(FileNode node, string indent = "")
    {
        _output.WriteLine($"{indent}{node.Name}");
        foreach (var child in node.Children)
        {
            PrintTree(child, indent + "  ");
        }
    }
    
    public FilterEngineTests(TestTreeFixture fixture, ITestOutputHelper output)
    {
        _fixture = fixture;
        _engine = new FilterEngine();
        _output = output;
    }

    [Fact]
    public void Apply_WithEmptyOptions_ShouldReturnFullTree()
    {
        // Arrange
        var root = _fixture.CreateTestTree();
        var options = new FilterOptions
        {
            IncludeExtensions = new(),
            ExcludeExtensions = new(),
            IncludeNames = new(),
            ExcludeNames = new(),
            IgnoreEmptyFolders = false
        };
        var context = new FilterContext(options);

        // Act
        var result = _engine.Apply(root, context);

        // Assert
        Assert.Equal(4, result.Children.Count); // bin, src, temp, config.json
    }

    [Fact]
    public void Apply_ExcludeExtensions_ShouldRemoveMatchingFiles()
    {
        // Arrange
        var root = _fixture.CreateTestTree();
        var options = new FilterOptions
        {
            IncludeExtensions = new(),
            ExcludeExtensions = new() { ".exe" },
            IncludeNames = new(),
            ExcludeNames = new(),
            IgnoreEmptyFolders = false
        };
        var context = new FilterContext(options);

        // Act
        var result = _engine.Apply(root, context);

        // Assert
        var bin = result.Children.First(c => c.Name == "bin");
        Assert.Empty(bin.Children);
    }

    [Fact]
    public void Apply_IncludeExtensions_ShouldOnlyKeepMatchingFiles()
    {
        // Arrange
        var root = _fixture.CreateTestTree();
        var options = new FilterOptions
        {
            IncludeExtensions = new() { ".cs" },
            ExcludeExtensions = new(),
            IncludeNames = new(),
            ExcludeNames = new(),
            IgnoreEmptyFolders = false
        };
        var context = new FilterContext(options);

        // Act
        var result = _engine.Apply(root, context);
        PrintTree(result);
        // Assert
        // bin has app.exe, should be empty (but folder remains because IgnoreEmptyFolders=false)
        var bin = result.Children.First(c => c.Name == "bin");
        Assert.Empty(bin.Children);

        // src has main.cs, utils.cs and docs/readme.md. docs should be empty.
        var src = result.Children.First(c => c.Name == "src");
        Assert.Contains(src.Children, c => c.Name == "main.cs");
        Assert.Contains(src.Children, c => c.Name == "utils.cs");
        var docs = src.Children.First(c => c.Name == "docs");
        Assert.Empty(docs.Children);
        
        Assert.Null(result.Children.FirstOrDefault(c => c.Name == "config.json"));
    }

    [Fact]
    public void Apply_IgnoreEmptyFolders_ShouldRemoveFoldersWithNoChildren()
    {
        // Arrange
        var root = _fixture.CreateTestTree();
        var options = new FilterOptions
        {
            IncludeExtensions = new List<string> { ".cs" },
            IgnoreEmptyFolders = true
        };
        var context = new FilterContext(options);

        // Act
        var result = _engine.Apply(root, context);

        // Debug info
        PrintTree(result);

        // Assert
        Assert.DoesNotContain(result.Children, c => c.Name == "bin");
        Assert.DoesNotContain(result.Children, c => c.Name == "temp");
        Assert.DoesNotContain(result.Children, c => c.Name == "config.json");
        
        var src = result.Children.FirstOrDefault(c => c.Name == "src");
        Assert.NotNull(src);
        Assert.DoesNotContain(src.Children, c => c.Name == "docs");
    }

    [Fact]
    public void Apply_ExcludeNames_ShouldRemoveMatchingNodes()
    {
        // Arrange
        var root = _fixture.CreateTestTree();
        var options = new FilterOptions
        {
            IncludeExtensions = new(),
            ExcludeExtensions = new(),
            IncludeNames = new(),
            ExcludeNames = new() { "src", "config.json" },
            IgnoreEmptyFolders = false
        };
        var context = new FilterContext(options);

        // Act
        var result = _engine.Apply(root, context);

        // Assert
        Assert.DoesNotContain(result.Children, c => c.Name == "src");
        Assert.DoesNotContain(result.Children, c => c.Name == "config.json");
        Assert.Contains(result.Children, c => c.Name == "bin");
    }

    [Fact]
    public void Apply_IncludeNames_ShouldOverrideExclusions()
    {
        // Arrange
        var root = _fixture.CreateTestTree();
        var options = new FilterOptions
        {
            IncludeExtensions = new(),
            ExcludeExtensions = new() { ".exe" },
            IncludeNames = new() { "app.exe" },
            ExcludeNames = new(),
            IgnoreEmptyFolders = false
        };
        var context = new FilterContext(options);

        // Act
        var result = _engine.Apply(root, context);

        // Assert
        var bin = result.Children.First(c => c.Name == "bin");
        Assert.Contains(bin.Children, c => c.Name == "app.exe");
    }
}
