namespace Fixie
{
    using System.Reflection;
    using System.Text.RegularExpressions;

    public class Test
    {
        public string Class { get; }
        public string Method { get; }
        public string Name { get; }

        public Test(MethodInfo method)
        {
            Class = method.ReflectedType!.FullName!;
            Method = method.Name;
            Name = Class + "." + Method;
        }

        public Test(string @class, string method)
        {
            Class = @class;
            Method = method;
            Name = Class + "." + Method;
        }

        public Test(string name)
        {
            var indexOfMemberSeparator = name.LastIndexOf(".");
            var className = name.Substring(0, indexOfMemberSeparator);
            var methodName = name.Substring(indexOfMemberSeparator + 1);

            Class = className;
            Method = methodName;
            Name = name;
        }

        public bool Matches(string pattern)
        {
            var previousWasUpperCase = false;
            var patternWithWildcards = "";

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
            
            return Regex.IsMatch(Name, patternWithWildcards);
        }
    }
}