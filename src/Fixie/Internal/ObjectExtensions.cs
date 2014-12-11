using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace Fixie.Internal
{
    public static class ObjectExtensions
    {
        static readonly Dictionary<string, string> EscapeMapping = new Dictionary<string, string>()
        {
            { "\"", "\\\"" },
            { "\\\\", @"\\" },
            { "\0", @"\0" },
            { "\a", @"\a" },
            { "\b", @"\b" },
            { "\f", @"\f" },
            { "\n", @"\n" },
            { "\r", @"\r" },
            { "\t", @"\t" },
            { "\v", @"\v" }
        };

        static readonly Regex SpecialCharacters = new Regex(string.Join("|", EscapeMapping.Keys.ToArray()));

        public static string ToDisplayString(this object parameter)
        {
            if (parameter == null)
                return "null";

            if (parameter is char)
                return "'" + parameter + "'";

            var s = parameter as string;
            if (s != null)
            {
                const int trimLength = 15;

                if (s.Length > trimLength)
                    return "\"" + s.Substring(0, trimLength).Escape() + "\"...";

                return "\"" + s.Escape() + "\"";
            }

            return Convert.ToString(parameter, CultureInfo.InvariantCulture);
        }

        static string Escape(this string s)
        {
            return SpecialCharacters.Replace(s, Replacement);
        }

        static string Replacement(Match m)
        {
            return EscapeMapping.ContainsKey(m.Value) ? EscapeMapping[m.Value] : EscapeMapping[Regex.Escape(m.Value)];
        }
    }
}