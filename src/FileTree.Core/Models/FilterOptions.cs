namespace FileTree.Core.Models;

public class FilterOptions
{
    public List<string> IncludeExtensions { get; init; }  // Include only these extensions if list is not empty
    public List<string> ExcludeExtensions { get; init; }  // Extensions to always exclude
    public List<string> IncludeNames { get; init; }       // File names to include always
    public List<string> ExcludeNames { get; init; }       // File names to exclude
    public bool IgnoreEmptyFolders { get; init; }         // Skip folder if there's no children 
}