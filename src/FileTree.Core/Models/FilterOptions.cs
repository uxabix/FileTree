namespace FileTree.Core.Models;

public class FilterOptions
{
    public List<string> IncludeExtensions { get; set; } = new List<string>();  // Include only these extensions if list is not empty
    public List<string> ExcludeExtensions { get; set; } = new List<string>();  // Extensions to always exclude
    public List<string> IncludeNames { get; set; } = new List<string>();       // File names to include always
    public List<string> ExcludeNames { get; set; } = new List<string>();       // File names to exclude
    public bool IgnoreEmptyFolders { get; set; }         // Skip folder if there's no children 
}