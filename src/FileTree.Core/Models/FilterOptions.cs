namespace FileTree.Core.Models;

public class FilterOptions
{
    public List<string> IncludeExtensions { get; set; } = new(); // Include only these extensions if list is not empty
    public List<string> ExcludeExtensions { get; set; } = new(); // Extensions to always exclude
    public List<string> IncludeNames { get; set; } = new(); // File names to include always
    public List<string> ExcludeNames { get; set; } = new(); // File names to exclude
    public bool IgnoreEmptyFolders { get; set; } // Skip folder if there's no children 
}