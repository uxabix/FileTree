using System.Runtime.InteropServices;

namespace FileTree.CLI;

internal static class SystemIntegratorFactory
{
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

