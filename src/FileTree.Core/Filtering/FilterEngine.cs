using FileTree.Core.Models;

namespace FileTree.Core.Filtering;

internal class FilterEngine : IFilterEngine
{
    public void Apply(FileNode root, FilterContext context)
    {
        ApplyFilterOptions(root, context.Options);
    }

    private void ApplyFilterOptions(FileNode root, FilterOptions filterOptions)
    {
        FilterIncludedExtensions(root, filterOptions.IncludeExtensions);
        FilterExcludedExtensions(root, filterOptions.ExcludeExtensions);
        FilterNames(root, filterOptions.GlobalIgnoreNames);
    }
    
    private void FilterIncludedExtensions(FileNode root, List<string> include)
    {
        
    }
    
    private void FilterExcludedExtensions(FileNode root, List<string> exclude)
    {
        
    }
    
    private void FilterNames(FileNode root, List<string> include)
    {
        
    }
}