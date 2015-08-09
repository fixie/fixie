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
            var sb = new StringBuilder();

            foreach (var ch in s)
                sb.Append(Escape(ch));

            return sb.ToString();
        }

        static string Escape(char ch)
        {
            switch (ch)
            {
                case '\"': return "\\\"";
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
                   return string.Format("\\u{0:X4}", (int)ch);

                default:
                    return Char.ToString(ch);
            }
        }
    }
}