namespace Fixie.Execution
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;

    class Filter
    {
        readonly List<Func<MethodGroup, bool>> filterConditions = new List<Func<MethodGroup, bool>>();

        public void ByPatterns(params string[] patterns)
        {
            foreach (var pattern in patterns)
            {
                var regex = PatternToRegex(pattern);

                filterConditions.Add(methodGroup => regex.IsMatch(methodGroup.FullName));
            }
        }

        public bool IsSatisfiedBy(MethodGroup methodGroup)
            => filterConditions.Count == 0 ||
               filterConditions.Any(condition => condition(methodGroup));

        static Regex PatternToRegex(string pattern)
        {
            var parts = pattern.Split(new[] { '*' }, StringSplitOptions.None);

            var wildCards = string.Join(".*", parts.Select(Regex.Escape));

            return new Regex($".*{wildCards}.*");
        }
    }
}