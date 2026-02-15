using System.Collections.Generic;

internal class GitIgnoreParser
{
    private readonly string[] _lines;

    public GitIgnoreParser(string[] lines)
    {
        _lines = lines;
    }

    public GitIgnoreRules Parse()
    {
        var rules = new List<GitIgnoreRule>();

        foreach (var line in _lines)
        {
            var trimmed = line.Trim();
            if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith("#"))
                continue;

            bool isNegate = trimmed.StartsWith("!");
            if (isNegate)
                trimmed = trimmed.Substring(1);

            bool isDirectory = trimmed.EndsWith("/");
            string pattern = trimmed.TrimEnd('/');

            rules.Add(new GitIgnoreRule(pattern, isDirectory, isNegate));
        }

        return new GitIgnoreRules(rules);
    }
}

/*using System; Пример использования

class Program
{
    static void Main()
    {
        string[] gitignoreLines =
        {
            "src/",
            "README.md",
            "!src/utils.cs"  // пример negate
        };

        var parser = new GitIgnoreParser(gitignoreLines);
        var rules = parser.Parse();

        Console.WriteLine(rules.IsIgnored("src/main.cs"));   // true
        Console.WriteLine(rules.IsIgnored("README.md"));     // true
        Console.WriteLine(rules.IsIgnored("src/utils.cs"));  // false (negate)
        Console.WriteLine(rules.IsIgnored("docs/manual.txt"));// false
    }
}*/
