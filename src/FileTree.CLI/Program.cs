using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FileTree.Core.Formatting;
using FileTree.Core.Models;

class Program
{
    static void Main()
    {
        var tree = new Dictionary<string, object>
        {
            ["project"] = new Dictionary<string, object>
            {
                ["src"] = new Dictionary<string, object>
                {
                    ["main.cs"] = new Dictionary<string, object>(),
                    ["utils.cs"] = new Dictionary<string, object>()
                },
                ["README.md"] = new Dictionary<string, object>()
            }
        };
        UnicodeTreeFormatter formatter = new ();
        FileNode root = BuildTree(tree);
        Console.WriteLine(formatter.Format(root));
        Console.ReadKey();
    }

    static FileNode BuildTree(Dictionary<string, object> dict, string parentPath = "")
    {
        // Берём первый (и единственный) корневой элемент словаря
        var rootEntry = dict.First();
        return BuildNode(rootEntry.Key, rootEntry.Value, parentPath);
    }

    static FileNode BuildNode(string name, object value, string parentPath)
    {
        string fullPath = string.IsNullOrEmpty(parentPath) ? name : Path.Combine(parentPath, name);
        bool isDirectory = value is Dictionary<string, object>;

        var node = new FileNode(name, fullPath, isDirectory);

        if (isDirectory)
        {
            var children = (Dictionary<string, object>)value;
            foreach (var child in children)
            {
                node.AddChild(BuildNode(child.Key, child.Value, fullPath));
            }
        }

        return node;
    }

    static void PrintTree(FileNode node, string indent = "")
    {
        Console.WriteLine($"{indent}{node.Name}");

        foreach (var child in node.Children)
        {
            PrintTree(child, indent + "  ");
        }
    }
}
