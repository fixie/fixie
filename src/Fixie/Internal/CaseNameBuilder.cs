using System.Globalization;
using System.Reflection;
using System.Text;

namespace Fixie.Internal;

static class CaseNameBuilder
{
    public static string GetName(MethodInfo method, object?[] parameters)
    {
        var name = method.ReflectedType!.FullName + "." + method.Name;

        if (method.IsGenericMethod)
            name += $"<{string.Join(", ", method.GetGenericArguments().Select(x => x.IsGenericParameter ? x.Name : x.FullName))}>";

        if (parameters.Length > 0)
            name += $"({string.Join(", ", parameters.Select(x => x.ToDisplayString()))})";

        return name;
    }

    static string ToDisplayString(this object? parameter)
    {
        if (parameter == null)
            return "null";

        if (parameter is char ch)
            return CharacterLiteral(ch);

        if (parameter is string s)
            return ShortStringLiteral(s);

        var displayString = Convert.ToString(parameter, CultureInfo.InvariantCulture);

        if (displayString == null)
            return parameter.GetType().ToString();

        return displayString;
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
            case '\"': return literal == Literal.String ? @"\""" : char.ToString(ch);
            case '\'': return literal == Literal.Character ? @"\'" : char.ToString(ch);

            case '\\': return @"\\";
            case '\0': return @"\0";
            case '\a': return @"\a";
            case '\b': return @"\b";
            case '\f': return @"\f";
            case '\n': return @"\n";
            case '\r': return @"\r";
            case '\t': return @"\t";
            case '\v': return @"\v";

            case '\u0085': //Next Line
            case '\u2028': //Line Separator
            case '\u2029': //Paragraph Separator
                var digits = (int)ch;
                return $"\\u{digits:X4}";

            default:
                return char.ToString(ch);
        }
    }

    enum Literal { Character, String }
}