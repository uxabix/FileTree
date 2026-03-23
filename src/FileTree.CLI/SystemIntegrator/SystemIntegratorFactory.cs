using System.Runtime.InteropServices;

namespace FileTree.CLI.SystemIntegrator;

/// <summary>
/// Creates platform-appropriate <see cref="ISystemIntegrator"/>.
/// Windows/Linux/NoOp based on RuntimeInformation.IsOSPlatform.
/// </summary>
internal static class SystemIntegratorFactory
{
    /// <summary>Detects OS and instantiates integrator.</summary>
    /// <returns>WindowsSystemIntegrator on Windows, Linux on Linux, NoOp otherwise.</returns>
    public static ISystemIntegrator Create()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return new WindowsSystemIntegrator();
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return new LinuxSystemIntegrator();
        }

        return new NoOpSystemIntegrator();
    }
}

