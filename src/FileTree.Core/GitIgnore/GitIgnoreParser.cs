using System;
using System.IO;
using System.Linq;

namespace FileTree.Core.GitIgnore
{
    /// <summary>
    /// Factory methods for <see cref="GitIgnoreRules"/> from empty, lines, or .gitignore file.
    /// Handles parsing, comment stripping, trimming.
    /// </summary>
    public static class GitIgnoreParser
    {
        /// <summary>Creates empty rules instance.</summary>
        public static GitIgnoreRules Create() => new GitIgnoreRules();

        /// <summary>Parses lines, ignores empty/comments, adds to new rules.</summary>
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

        /// <summary>Loads from file path, reads lines then FromLines.</summary>
        public static GitIgnoreRules FromFile(string gitIgnoreFilePath)
        {
            if (string.IsNullOrWhiteSpace(gitIgnoreFilePath)) throw new ArgumentNullException(nameof(gitIgnoreFilePath));
            if (!File.Exists(gitIgnoreFilePath)) throw new FileNotFoundException("GitIgnore file not found", gitIgnoreFilePath);

            var lines = File.ReadAllLines(gitIgnoreFilePath);
            return FromLines(lines);
        }
    }
}
