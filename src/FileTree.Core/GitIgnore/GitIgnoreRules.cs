using System;
using System.Collections.Generic;
using System.Linq;
using Ignore;

namespace FileTree.Core.GitIgnore
{
    /// <summary>
    /// Wrapper around Ignore library for gitignore-style path matching.
    /// Fluent Add rules, query IsIgnored/Filter.
    /// </summary>
    public class GitIgnoreRules
    {
        private readonly Ignore.Ignore _ignore;

        /// <summary>Initializes empty matcher.</summary>
        public GitIgnoreRules()
        {
            _ignore = new Ignore.Ignore();
        }

        /// <summary>Adds single rule, fluent.</summary>
        /// <param name="rule">Pattern.</param>
        public GitIgnoreRules Add(string rule)
        {
            if (rule == null) throw new ArgumentNullException(nameof(rule));
            _ignore.Add(rule);
            return this;
        }

        /// <summary>Adds rules enumerable, cleans empty/comments, fluent.</summary>
        public GitIgnoreRules Add(IEnumerable<string> rules)
        {
            if (rules == null) throw new ArgumentNullException(nameof(rules));
            var arr = rules.Where(r => r != null).Select(r => r!.Trim()).Where(r => r.Length > 0).ToArray();
            if (arr.Length > 0)
                _ignore.Add(arr);
            return this;
        }

        /// <summary>Filters paths not ignored.</summary>
        public IEnumerable<string> Filter(IEnumerable<string> paths)
        {
            if (paths == null) throw new ArgumentNullException(nameof(paths));
            return _ignore.Filter(paths);
        }

        /// <summary>Tests if path ignored.</summary>
        public bool IsIgnored(string path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            return _ignore.IsIgnored(path);
        }
    }
}
