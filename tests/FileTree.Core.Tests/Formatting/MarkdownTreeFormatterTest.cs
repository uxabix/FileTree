using FileTree.Core.Formatting;
using FileTree.Core.Models;
using Xunit;

namespace FileTree.Core.Tests.Formatting;

public class MarkdownTreeFormatterTests
{
    private static FileNode CreateTree()
    {
        var root = new FileNode("project", "project", true);

        var src = new FileNode("src", "project/src", true);
        src.AddChild(new FileNode("main.cs", "project/src/main.cs", false));
        src.AddChild(new FileNode("utils.cs", "project/src/utils.cs", false));

        root.AddChild(src);
        root.AddChild(new FileNode("README.md", "project/README.md", false));

        return root;
    }

    [Fact]
    public void Format_ShouldStartWithRootName_WithTrailingSlash()
    {
        var formatter = new MarkdownTreeFormatter();
        var root = CreateTree();

        var output = formatter.Format(root);
        var firstLine = output.Split('\n', System.StringSplitOptions.RemoveEmptyEntries)[0];

        Assert.Equal("project/", firstLine.TrimEnd());
    }

    [Fact]
    public void Format_ShouldContainDirectorySlashes()
    {
        var formatter = new MarkdownTreeFormatter();
        var root = CreateTree();

        var output = formatter.Format(root);

        Assert.Contains("project/", output);
        Assert.Contains("src/", output);
    }

    [Fact]
    public void Format_ShouldContainAllNodeNames()
    {
        var formatter = new MarkdownTreeFormatter();
        var root = CreateTree();

        var output = formatter.Format(root);

        Assert.Contains("project/", output);
        Assert.Contains("src/", output);
        Assert.Contains("main.cs", output);
        Assert.Contains("utils.cs", output);
        Assert.Contains("README.md", output);
    }

    [Fact]
    public void Format_ShouldNotContainBoxDrawingOrAsciiBranchCharacters()
    {
        var formatter = new MarkdownTreeFormatter();
        var root = CreateTree();

        var output = formatter.Format(root);

        // No Unicode box-drawing
        Assert.DoesNotContain("└", output);
        Assert.DoesNotContain("├", output);
        Assert.DoesNotContain("│", output);

        // No ASCII branch connectors used by AsciiTreeFormatter
        Assert.DoesNotContain("|-- ", output);
        Assert.DoesNotContain("`-- ", output);
    }

    [Fact]
    public void Format_ShouldIndentChildren_ByTwoSpacesPerLevel_AndPreserveOrder()
    {
        var formatter = new MarkdownTreeFormatter();
        var root = CreateTree();

        var output = formatter.Format(root).Replace("\r\n", "\n");
        var lines = output.Split('\n', System.StringSplitOptions.RemoveEmptyEntries);

        Assert.Equal("project/", lines[0].TrimEnd());
        Assert.Equal("  src/", lines[1].TrimEnd());
        Assert.Equal("    main.cs", lines[2].TrimEnd());
        Assert.Equal("    utils.cs", lines[3].TrimEnd());
        Assert.Equal("  README.md", lines[4].TrimEnd());
    }
}
