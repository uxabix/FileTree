namespace FileTree.CLI.SystemIntegrator;

/// <summary>
/// Platform-specific system integration (PATH, shims, aliases, context menus).
/// Windows: full auto. Linux: prints manual instructions. Other: NoOp.
/// </summary>
public interface ISystemIntegrator
{
    /// <summary>Adds FileTree to user PATH/context menu/shims/configs.</summary>
    Task InstallAsync();
    /// <summary>Removes user install (known locations).</summary>
    Task UninstallAsync();
    /// <summary>Scans/prompts removal of all FileTree entries (unknown installs).</summary>
    Task UninstallDeepAsync();
}
