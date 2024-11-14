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

        if (parameter is bool b)
            return b ? "true" : "false";

        var displayString = Convert.ToString(parameter, CultureInfo.InvariantCulture);

        if (displayString == null)
            return parameter.GetType().ToString();

        return displayString;
    }

    static string CharacterLiteral(char ch)
    {
        return "'" + Escape(ch, Literal.Character) + "'";
    }

    static string ShortStringLiteral(string s)
    {
        const int trimLength = 15;

        if (s.Length > trimLength)
            s = s.Substring(0, trimLength) + "...";

        var sb = new StringBuilder();
        
        sb.Append('"');

        foreach (var ch in s)
            sb.Append(Escape(ch, Literal.String));

        sb.Append('"');

        return  sb.ToString();
    }

    static string Escape(char ch, Literal literal) =>
        ch switch
        {
            '\0' => @"\0",
            '\a' => @"\a",
            '\b' => @"\b",
            '\t' => @"\t",
            '\n' => @"\n",
            '\v' => @"\v",
            '\f' => @"\f",
            '\r' => @"\r",

            #if NET9_0_OR_GREATER
            '\e' => @"\e",
            #endif

            ' ' => " ",
            '\"' => literal == Literal.String ? @"\""" : char.ToString(ch),
            '\'' => literal == Literal.Character ? @"\'" : char.ToString(ch),
            '\\' => @"\\",
            _ when (char.IsControl(ch) || char.IsWhiteSpace(ch)) => $"\\u{(int)ch:X4}",
            _ => char.ToString(ch)
        };

    enum Literal { Character, String }
}