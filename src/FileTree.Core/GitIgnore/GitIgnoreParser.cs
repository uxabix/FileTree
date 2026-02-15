using System;
using System.IO;
using System.Linq;

namespace FileTree.Core.GitIgnore
{
    public static class GitIgnoreParser
    {
        public static GitIgnoreRules Create() => new GitIgnoreRules();

        public static GitIgnoreRules FromLines(IEnumerable<string> lines)
        {
            if (lines == null) throw new ArgumentNullException(nameof(lines));
            var cleaned = lines
                .Select(l => l?.Trim())
                .Where(l => !string.IsNullOrEmpty(l) && !l!.StartsWith("#", StringComparison.Ordinal))
                .ToArray();

            var rules = new GitIgnoreRules();
            if (cleaned.Length > 0)
                rules.Add(cleaned);

            return rules;
        }

        public static GitIgnoreRules FromFile(string gitIgnoreFilePath)
        {
            if (string.IsNullOrWhiteSpace(gitIgnoreFilePath)) throw new ArgumentNullException(nameof(gitIgnoreFilePath));
            if (!File.Exists(gitIgnoreFilePath)) throw new FileNotFoundException("GitIgnore file not found", gitIgnoreFilePath);

            var lines = File.ReadAllLines(gitIgnoreFilePath);
            return FromLines(lines);
        }
    }
}
