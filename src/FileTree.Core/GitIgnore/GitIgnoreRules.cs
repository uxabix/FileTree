using System;
using System.Collections.Generic;
using System.Linq;
using Ignore;

namespace FileTree.Core.GitIgnore
{
    public class GitIgnoreRules
    {
        private readonly Ignore.Ignore _ignore;

        public GitIgnoreRules()
        {
            _ignore = new Ignore.Ignore();
        }

        public GitIgnoreRules Add(string rule)
        {
            if (rule == null) throw new ArgumentNullException(nameof(rule));
            _ignore.Add(rule);
            return this;
        }

        public GitIgnoreRules Add(IEnumerable<string> rules)
        {
            if (rules == null) throw new ArgumentNullException(nameof(rules));
            var arr = rules.Where(r => r != null).Select(r => r!.Trim()).Where(r => r.Length > 0).ToArray();
            if (arr.Length > 0)
                _ignore.Add(arr);
            return this;
        }

        public IEnumerable<string> Filter(IEnumerable<string> paths)
        {
            if (paths == null) throw new ArgumentNullException(nameof(paths));
            return _ignore.Filter(paths);
        }

        public bool IsIgnored(string path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            return _ignore.IsIgnored(path);
        }
    }
}
