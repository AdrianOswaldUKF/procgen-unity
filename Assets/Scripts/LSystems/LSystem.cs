using System.Collections.Generic;
using System.Text;

public class LSystem
{
    private readonly Dictionary<char, string> _rules;

    public LSystem(string[] rules)
    {
        _rules = new Dictionary<char, string>();

        foreach (string rule in rules)
        {
            string[] parts = rule.Split('=');
            if (parts.Length == 2)
            {
                _rules[parts[0][0]] = parts[1];
            }
        }
    }

    public string Expand(string axiom, int iterations)
    {
        StringBuilder current = new StringBuilder(axiom);

        for (int i = 0; i < iterations; i++)
        {
            StringBuilder next = new StringBuilder();

            foreach (char c in current.ToString())
            {
                next.Append(_rules.TryGetValue(c, out string r) ? r : c);
            }

            current = next;
        }

        return current.ToString();
    }
}