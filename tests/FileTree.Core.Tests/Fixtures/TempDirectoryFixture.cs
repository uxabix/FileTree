using System;
using System.IO;

namespace FileTree.Core.Tests.Fixtures
{
    /// <summary>
    /// IDisposable fixture for creating temporary directories/files for integration tests.
    /// Cleans up on Dispose.
/// </summary>
    public class TempDirectoryFixture : IDisposable
    {
        public string RootPath { get; }

        public TempDirectoryFixture()
        {
            RootPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(RootPath);
        }

        public void CreateFile(string relativePath, string content = "")
        {
            var fullPath = Path.Combine(RootPath, relativePath);
            var dir = Path.GetDirectoryName(fullPath);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir!);
            }
            File.WriteAllText(fullPath, content);
        }

        public void CreateDirectory(string relativePath)
        {
            var fullPath = Path.Combine(RootPath, relativePath);
            Directory.CreateDirectory(fullPath);
        }

        public void Dispose()
        {
            if (Directory.Exists(RootPath))
            {
                try
                {
                    Directory.Delete(RootPath, true);
                }
                catch
                {
                    // Ignore cleanup errors
                }
            }
        }
    }
}
