using FileTree.Core.Models;

namespace FileTree.Core.Tests.Fixtures;

/// <summary>
/// Fixture providing a standard test tree structure for formatter/scanner tests.
/// Creates in-memory FileNode hierarchy with root/bin/src/docs/temp/config.
/// </summary>
public class TestTreeFixture
{
    /// <summary>Creates the test tree with known structure for assertions.</summary>
    /// <returns>Root FileNode.</returns>
    public FileNode CreateTestTree()
    {
        // root/
        //   bin/
        //     app.exe
        //   src/
        //     main.cs
        //     utils.cs
        //     docs/
        //       readme.md
        //   temp/
        //     (empty)
        //   config.json

        var root = new FileNode("root", "C:\\root", true);

        var bin = new FileNode("bin", "C:\\root\\bin", true);
        bin.AddChild(new FileNode("app.exe", "C:\\root\\bin\\app.exe", false));
        root.AddChild(bin);

        var src = new FileNode("src", "C:\\root\\src", true);
        src.AddChild(new FileNode("main.cs", "C:\\root\\src\\main.cs", false));
        src.AddChild(new FileNode("utils.cs", "C:\\root\\src\\utils.cs", false));

        var docs = new FileNode("docs", "C:\\root\\src\\docs", true);
        docs.AddChild(new FileNode("readme.md", "C:\\root\\src\\docs\\readme.md", false));
        src.AddChild(docs);
        root.AddChild(src);

        var temp = new FileNode("temp", "C:\\root\\temp", true);
        root.AddChild(temp);

        root.AddChild(new FileNode("config.json", "C:\\root\\config.json", false));

        return root;
    }
}
