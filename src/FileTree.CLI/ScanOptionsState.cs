using System.IO;
using FileTree.Core.Models;

namespace FileTree.CLI;

internal sealed class ScanOptionsState
{
    public string? Path { get; set; }
    public string? PathOption { get; set; }
    public int? MaxDepth { get; set; }
    public int? MaxWidth { get; set; }
    public int? MaxNodes { get; set; }
    public bool? UseGitIgnore { get; set; }
    public OutputFormat? Format { get; set; }
    public IEnumerable<string>? FilterRules { get; set; }
    public string? FilterFile { get; set; }
    public string? GlobalFilterFile { get; set; }
    public bool? NoDefaultFilters { get; set; }
    public bool? NoAppGlobalIgnore { get; set; }
    public bool? NoLocalFilters { get; set; }
    public bool? NoDefaultSettings { get; set; }
    public IEnumerable<string>? IncludeExtensions { get; set; }
    public IEnumerable<string>? ExcludeExtensions { get; set; }
    public IEnumerable<string>? IncludeNames { get; set; }
    public IEnumerable<string>? ExcludeNames { get; set; }
    public bool? IgnoreEmptyFolders { get; set; }
    public bool? SkipHidden { get; set; }
    public bool? HighlightHiddenFiles { get; set; }
    public HiddenStyle? HiddenStyle { get; set; }
    public bool? Wait { get; set; }

    public static ScanOptionsState FromCli(ScanCommandOptions cli)
    {
        if (cli == null)
        {
            throw new ArgumentNullException(nameof(cli));
        }

        var state = new ScanOptionsState
        {
            Path = NormalizePath(cli.Path),
            PathOption = NormalizePath(cli.PathOption),
            MaxDepth = cli.MaxDepth,
            MaxWidth = cli.MaxWidth,
            MaxNodes = cli.MaxNodes,
            UseGitIgnore = cli.UseGitIgnore,
            Format = cli.Format,
            FilterRules = cli.FilterRules,
            FilterFile = NormalizePath(cli.FilterFile),
            GlobalFilterFile = NormalizePath(cli.GlobalFilterFile),
            NoDefaultFilters = cli.NoDefaultFilters,
            NoAppGlobalIgnore = cli.NoAppGlobalIgnore,
            NoLocalFilters = cli.NoLocalFilters,
            NoDefaultSettings = cli.NoDefaultSettings,
            IncludeExtensions = cli.IncludeExtensions,
            ExcludeExtensions = cli.ExcludeExtensions,
            IncludeNames = cli.IncludeNames,
            ExcludeNames = cli.ExcludeNames,
            IgnoreEmptyFolders = cli.IgnoreEmptyFolders,
            SkipHidden = cli.SkipHidden,
            HighlightHiddenFiles = cli.HighlightHiddenFiles,
            HiddenStyle = cli.HiddenStyle,
            Wait = cli.Wait,
        };

        if (!string.IsNullOrWhiteSpace(state.PathOption))
        {
            state.Path = state.PathOption;
        }

        return state;
    }

    public void MergeFrom(ScanOptionsState source)
    {
        if (source == null)
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(source.PathOption))
        {
            PathOption = source.PathOption;
            Path = source.PathOption;
        }
        else if (!string.IsNullOrWhiteSpace(source.Path))
        {
            Path = source.Path;
            PathOption = null;
        }

        if (source.MaxDepth.HasValue) MaxDepth = source.MaxDepth;
        if (source.MaxWidth.HasValue) MaxWidth = source.MaxWidth;
        if (source.MaxNodes.HasValue) MaxNodes = source.MaxNodes;
        if (source.UseGitIgnore.HasValue) UseGitIgnore = source.UseGitIgnore;
        if (source.Format.HasValue) Format = source.Format;
        if (source.FilterRules != null) FilterRules = source.FilterRules;
        if (source.FilterFile != null) FilterFile = source.FilterFile;
        if (source.GlobalFilterFile != null) GlobalFilterFile = source.GlobalFilterFile;
        if (source.NoDefaultFilters.HasValue) NoDefaultFilters = source.NoDefaultFilters;
        if (source.NoAppGlobalIgnore.HasValue) NoAppGlobalIgnore = source.NoAppGlobalIgnore;
        if (source.NoLocalFilters.HasValue) NoLocalFilters = source.NoLocalFilters;
        if (source.NoDefaultSettings.HasValue) NoDefaultSettings = source.NoDefaultSettings;
        if (source.IncludeExtensions != null) IncludeExtensions = source.IncludeExtensions;
        if (source.ExcludeExtensions != null) ExcludeExtensions = source.ExcludeExtensions;
        if (source.IncludeNames != null) IncludeNames = source.IncludeNames;
        if (source.ExcludeNames != null) ExcludeNames = source.ExcludeNames;
        if (source.IgnoreEmptyFolders.HasValue) IgnoreEmptyFolders = source.IgnoreEmptyFolders;
        if (source.SkipHidden.HasValue) SkipHidden = source.SkipHidden;
        if (source.HighlightHiddenFiles.HasValue) HighlightHiddenFiles = source.HighlightHiddenFiles;
        if (source.HiddenStyle.HasValue) HiddenStyle = source.HiddenStyle;
        if (source.Wait.HasValue) Wait = source.Wait;
    }

    public void ApplyDefaults(ScanOptionsState defaults)
    {
        if (defaults == null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(Path) && !string.IsNullOrWhiteSpace(defaults.PathOption))
        {
            PathOption = defaults.PathOption;
            Path = defaults.PathOption;
        }
        else if (string.IsNullOrWhiteSpace(Path) && !string.IsNullOrWhiteSpace(defaults.Path))
        {
            Path = defaults.Path;
        }

        if (!MaxDepth.HasValue && defaults.MaxDepth.HasValue) MaxDepth = defaults.MaxDepth;
        if (!MaxWidth.HasValue && defaults.MaxWidth.HasValue) MaxWidth = defaults.MaxWidth;
        if (!MaxNodes.HasValue && defaults.MaxNodes.HasValue) MaxNodes = defaults.MaxNodes;
        if (!UseGitIgnore.HasValue && defaults.UseGitIgnore.HasValue) UseGitIgnore = defaults.UseGitIgnore;
        if (!Format.HasValue && defaults.Format.HasValue) Format = defaults.Format;
        if (FilterRules == null && defaults.FilterRules != null) FilterRules = defaults.FilterRules;
        if (FilterFile == null && defaults.FilterFile != null) FilterFile = defaults.FilterFile;
        if (GlobalFilterFile == null && defaults.GlobalFilterFile != null) GlobalFilterFile = defaults.GlobalFilterFile;
        if (!NoDefaultFilters.HasValue && defaults.NoDefaultFilters.HasValue) NoDefaultFilters = defaults.NoDefaultFilters;
        if (!NoAppGlobalIgnore.HasValue && defaults.NoAppGlobalIgnore.HasValue) NoAppGlobalIgnore = defaults.NoAppGlobalIgnore;
        if (!NoLocalFilters.HasValue && defaults.NoLocalFilters.HasValue) NoLocalFilters = defaults.NoLocalFilters;
        if (!NoDefaultSettings.HasValue && defaults.NoDefaultSettings.HasValue) NoDefaultSettings = defaults.NoDefaultSettings;
        if (IncludeExtensions == null && defaults.IncludeExtensions != null) IncludeExtensions = defaults.IncludeExtensions;
        if (ExcludeExtensions == null && defaults.ExcludeExtensions != null) ExcludeExtensions = defaults.ExcludeExtensions;
        if (IncludeNames == null && defaults.IncludeNames != null) IncludeNames = defaults.IncludeNames;
        if (ExcludeNames == null && defaults.ExcludeNames != null) ExcludeNames = defaults.ExcludeNames;
        if (!IgnoreEmptyFolders.HasValue && defaults.IgnoreEmptyFolders.HasValue) IgnoreEmptyFolders = defaults.IgnoreEmptyFolders;
        if (!SkipHidden.HasValue && defaults.SkipHidden.HasValue) SkipHidden = defaults.SkipHidden;
        if (!HighlightHiddenFiles.HasValue && defaults.HighlightHiddenFiles.HasValue) HighlightHiddenFiles = defaults.HighlightHiddenFiles;
        if (!HiddenStyle.HasValue && defaults.HiddenStyle.HasValue) HiddenStyle = defaults.HiddenStyle;
        if (!Wait.HasValue && defaults.Wait.HasValue) Wait = defaults.Wait;
    }

    public FileTreeOptions ToFileTreeOptions()
    {
        return new FileTreeOptions
        {
            MaxDepth = MaxDepth ?? -1,
            MaxWidth = MaxWidth ?? -1,
            MaxNodes = MaxNodes ?? -1,
            UseGitIgnore = UseGitIgnore ?? true,
            SkipHidden = SkipHidden ?? true,
            HighlightHiddenFiles = HighlightHiddenFiles ?? true,
            HiddenStyle = HiddenStyle ?? global::FileTree.Core.Models.HiddenStyle.Suffix,
            Format = Format ?? OutputFormat.Ascii,
            Filter = new FilterOptions
            {
                RulesSource = BuildFilterRulesSource(),
                IncludeExtensions = IncludeExtensions?.ToList() ?? new List<string>(),
                ExcludeExtensions = ExcludeExtensions?.ToList() ?? new List<string>(),
                IncludeNames = IncludeNames?.ToList() ?? new List<string>(),
                ExcludeNames = ExcludeNames?.ToList() ?? new List<string>(),
                IgnoreEmptyFolders = IgnoreEmptyFolders ?? false,
                UseLocalFilterFiles = !(NoLocalFilters ?? false),
            }
        };
    }

    public string GetTargetPath()
    {
        if (!string.IsNullOrWhiteSpace(PathOption))
        {
            return PathOption;
        }

        if (!string.IsNullOrWhiteSpace(Path))
        {
            return Path;
        }

        return Directory.GetCurrentDirectory();
    }

    private FilterRulesSource BuildFilterRulesSource()
    {
        var inlineRules = FilterRules?
            .Where(rule => !string.IsNullOrWhiteSpace(rule))
            .Select(rule => rule.Trim())
            .ToList() ?? new List<string>();

        return new FilterRulesSource
        {
            AppGlobalConfigPath = AppPaths.GetGlobalIgnorePath(),
            UseAppGlobalConfig = !(NoAppGlobalIgnore ?? false),
            UseDefaultGlobalConfig = !(NoDefaultFilters ?? false),
            GlobalConfigPath = GlobalFilterFile,
            LocalConfigPath = FilterFile,
            InlineRules = inlineRules,
        };
    }

    private static string? NormalizePath(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim();

        if (string.Equals(trimmed, "true", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(trimmed, "false", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return trimmed;
    }
}
