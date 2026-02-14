using FileTree.Core.Models;

namespace FileTree.Core.Filtering;

internal class FilterEngine : IFilterEngine
{
    public FileNode Apply(FileNode root, FilterContext context)
    {
        var filtered = FilterNode(root, context);
        if (filtered == null) return new FileNode(root.Name, root.FullPath, root.IsDirectory);
        return filtered;
    }

    private FileNode? FilterNode(FileNode node, FilterContext context)
    {
        if (!ShouldKeep(node, context.Options))
            return null;

        var newNode = new FileNode(node.Name, node.FullPath, node.IsDirectory);

        if (node.IsDirectory)
            foreach (var child in node.Children)
            {
                var filteredChild = FilterNode(child, context);

                if (filteredChild != null) newNode.AddChild(filteredChild);
            }

        if (context.Options.IgnoreEmptyFolders &&
            !newNode.Children.Any() && newNode.IsDirectory)
            return null;

        return newNode;
    }

    private bool ShouldKeep(FileNode node, FilterOptions options)
    {
        if (options.IncludeNames.Contains(node.Name, StringComparer.OrdinalIgnoreCase))
            return true;
        if (options.ExcludeNames.Contains(node.Name, StringComparer.OrdinalIgnoreCase))
            return false;

        if (!node.IsDirectory)
        {
            var extension = Path.GetExtension(node.Name);

            if (options.IncludeExtensions.Any())
                return options.IncludeExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase);
            if (options.ExcludeExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
                return false;
        }

        return true;
    }
}