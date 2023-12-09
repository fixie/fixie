using System.Text.RegularExpressions;

namespace Fixie.Internal;

class TestPattern
{
    readonly Regex regex;

    public TestPattern(string pattern)
    {
        var patternWithWildcards = "";
            
        var previousWasUpperCase = false;

        foreach (var c in pattern)
        {
            if (c == '*')
            {
                patternWithWildcards += ".*";
                previousWasUpperCase = false;
            }
            else
            {
                if (previousWasUpperCase && !char.IsLower(c))
                    patternWithWildcards += "[a-z]*";

                patternWithWildcards += Regex.Escape(c.ToString());
                previousWasUpperCase = char.IsUpper(c);
            }
        }

        if (previousWasUpperCase)
            patternWithWildcards += "[a-z]*";

        regex = new Regex(patternWithWildcards + "$");
    }

    public bool Matches(string test)
    {
        return regex.IsMatch(test);
    }
}