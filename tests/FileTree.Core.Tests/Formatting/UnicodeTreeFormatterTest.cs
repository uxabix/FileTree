using FileTree.Core.Formatting;
using FileTree.Core.Models;
using Xunit;

namespace FileTree.Core.Tests.Formatting;

public class UnicodeTreeFormatterTests
{
    private static FormatContext CreateContext()
    {
        var options = new FileTreeOptions
        {
            Format = OutputFormat.Unicode
        };

        return new FormatContext(options);
    }

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
    public void Format_ShouldStartWithRootName()
    {
        var formatter = new UnicodeTreeFormatter();
        var root = CreateTree();

        var output = formatter.Format(root, CreateContext());
        var firstLine = output.Split('\n', System.StringSplitOptions.RemoveEmptyEntries)[0];

        Assert.Equal("project", firstLine.TrimEnd());
    }

    [Fact]
    public void Format_ShouldContainUnicodeBranches()
    {
        var formatter = new UnicodeTreeFormatter();
        var root = CreateTree();

        var output = formatter.Format(root, CreateContext());

        Assert.Contains("└─ ", output);
        Assert.Contains("├─ ", output);
    }

    [Fact]
    public void Format_ShouldContainVerticalLines()
    {
        var formatter = new UnicodeTreeFormatter();
        var root = CreateTree();

        var output = formatter.Format(root, CreateContext());

        Assert.Contains("│", output);
    }

    [Fact]
    public void Format_ShouldContainAllNodeNames()
    {
        var formatter = new UnicodeTreeFormatter();
        var root = CreateTree();

        var output = formatter.Format(root, CreateContext());

        Assert.Contains("project", output);
        Assert.Contains("src", output);
        Assert.Contains("main.cs", output);
        Assert.Contains("utils.cs", output);
        Assert.Contains("README.md", output);
    }

    [Fact]
    public void Format_ShouldNotContainAsciiBranches()
    {
        var formatter = new UnicodeTreeFormatter();
        var root = CreateTree();

        var output = formatter.Format(root, CreateContext());

        Assert.DoesNotContain("|--", output);
        Assert.DoesNotContain("`--", output);
    }

    [Fact]
    public void Format_ShouldPreserveHierarchyStructure()
    {
        var formatter = new UnicodeTreeFormatter();
        var root = CreateTree();

        var output = formatter.Format(root, CreateContext());
        var lines = output.Split('\n', System.StringSplitOptions.RemoveEmptyEntries);

        // Проверяем вложенность: src должен быть дочерним project
        Assert.Contains(lines, l => l.Contains("├─ src") || l.Contains("└─ src"));

        // Проверяем, что файлы находятся глубже
        Assert.Contains(lines, l => l.Contains("main.cs"));
        Assert.Contains(lines, l => l.Contains("utils.cs"));
    }
}
