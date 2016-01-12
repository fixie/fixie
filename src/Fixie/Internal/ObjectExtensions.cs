using System;
using System.Globalization;
using System.Text;

namespace Fixie.Internal
{
    public static class ObjectExtensions
    {
        public static string ToDisplayString(this object parameter)
        {
            if (parameter == null)
                return "null";

            if (parameter is char)
                return CharacterLiteral((char)parameter);

            var s = parameter as string;
            if (s != null)
                return ShortStringLiteral(s);

            return Convert.ToString(parameter, CultureInfo.InvariantCulture);
        }

        static string CharacterLiteral(char ch)
        {
            return "'" + ch.Escape(Literal.Character) + "'";
        }

        static string ShortStringLiteral(string s)
        {
            const int trimLength = 15;

            if (s.Length > trimLength)
                s = s.Substring(0, trimLength) + "...";

            var sb = new StringBuilder();

            foreach (var ch in s)
                sb.Append(ch.Escape(Literal.String));

            return "\"" + sb + "\"";
        }

        static string Escape(this char ch, Literal literal)
        {
            switch (ch)
            {
                case '\"': return literal == Literal.String ? "\\\"" : Char.ToString(ch);
                case '\'': return literal == Literal.Character ? "\\\'" : Char.ToString(ch);

                case '\\': return "\\\\";
                case '\0': return "\\0";
                case '\a': return "\\a";
                case '\b': return "\\b";
                case '\f': return "\\f";
                case '\n': return "\\n";
                case '\r': return "\\r";
                case '\t': return "\\t";
                case '\v': return "\\v";

                case '\u0085': //Next Line
                case '\u2028': //Line Separator
                case '\u2029': //Paragraph Separator
                    var digits = (int)ch;
                    return $"\\u{digits:X4}";

                default:
                    return Char.ToString(ch);
            }
        }

        enum Literal { Character, String }
    }
}