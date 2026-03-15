namespace FileTree.CLI.SystemIntegrator;

public interface ISystemIntegrator
{
    Task InstallAsync();
    Task UninstallAsync();
    Task UninstallDeepAsync();
}
