using System.Collections.Generic;
using System.Linq;

internal class GitIgnoreRules
{
    private readonly List<GitIgnoreRule> _rules;

    public GitIgnoreRules(IEnumerable<GitIgnoreRule> rules)
    {
        _rules = rules.ToList();
    }

    // Проверка, нужно ли игнорировать путь
    public bool IsIgnored(string relativePath)
    {
        bool ignored = false;

        foreach (var rule in _rules)
        {
            if (rule.Matches(relativePath))
            {
                ignored = !rule.IsNegate; // если negate = true, отменяет игнор
            }
        }

        return ignored;
    }
}
