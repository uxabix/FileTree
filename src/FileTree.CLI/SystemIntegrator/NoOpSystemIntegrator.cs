namespace FileTree.CLI.SystemIntegrator;

/// <summary>
/// No-operation integrator for unsupported OS.
/// Prints "unsupported" message and completes.
/// </summary>
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
