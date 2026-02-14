namespace FileTree.Core.Models;

public class FilterOptions
{
    public List<string> IncludeExtensions { get; init; }
    public List<string> ExcludeExtensions { get; init; }
    public List<string> GlobalIgnoreNames { get; init; }
}