namespace FileTree.CLI.SystemIntegrator;

internal sealed class NoOpSystemIntegrator : ISystemIntegrator
{
    public Task InstallAsync()
    {
        Console.WriteLine("Install command is not supported on this operating system.");
        return Task.CompletedTask;
    }

    public Task UninstallAsync()
    {
        Console.WriteLine("Uninstall command is not supported on this operating system.");
        return Task.CompletedTask;
    }

    public Task UninstallDeepAsync()
    {
        Console.WriteLine("Uninstall-deep command is not supported on this operating system.");
        return Task.CompletedTask;
    }
}
