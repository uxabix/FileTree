namespace FileTree.CLI;

public interface ISystemIntegrator
{
    Task InstallAsync();
    Task UninstallAsync();
}

