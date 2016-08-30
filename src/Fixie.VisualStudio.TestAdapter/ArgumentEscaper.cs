namespace Fixie.VisualStudio.TestAdapter
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public static class ArgumentEscaper
    {
        // See http://blogs.msdn.com/b/twistylittlepassagesallalike/archive/2011/04/23/everyone-quotes-arguments-the-wrong-way.aspx

        public static string EscapeForProcessStartInfo(IEnumerable<string> args)
            => string.Join(" ", args.Select(Escape));

        static string Escape(string argument)
        {
            var shouldSurroundWithQuotes = ShouldSurroundWithQuotes(argument);
            var resultWillBeSurroundedWithQuotes = shouldSurroundWithQuotes || IsSurroundedWithQuotes(argument);

            var result = new StringBuilder();
            for (int i = 0; i < argument.Length; ++i)
            {
                var backslashCount = 0;

                // Skip and count leading backslashes.
                while (i < argument.Length && argument[i] == '\\')
                {
                    backslashCount++;
                    i++;
                }

                if (i == argument.Length)
                {
                    // We're at the end of the argument.

                    if (resultWillBeSurroundedWithQuotes)
                    {
                        // Escape any backslashes to ensure the outside quote is interpreted as an argument delimiter.

                        result.Append('\\', backslashCount*2);
                    }
                    else
                    {
                        // Just add the backslashes without escaping them.

                        result.Append('\\', backslashCount);
                    }
                }

                else if (argument[i] == '"')
                {
                    // Escape any preceding backslashes and the quote.

                    result.Append('\\', backslashCount*2 + 1);
                    result.Append('"');
                }

                else
                {
                    // Output any consumed backslashes and the character.

                    result.Append('\\', backslashCount);
                    result.Append(argument[i]);
                }
            }

            return shouldSurroundWithQuotes ? $"\"{result}\"" : result.ToString();
        }

        static bool ShouldSurroundWithQuotes(string argument)
            => !IsSurroundedWithQuotes(argument) && ArgumentContainsWhitespace(argument);

        static bool IsSurroundedWithQuotes(string argument)
            => argument.StartsWith("\"", StringComparison.Ordinal) &&
               argument.EndsWith("\"", StringComparison.Ordinal);

        static bool ArgumentContainsWhitespace(string argument)
            => argument.Contains(" ") || argument.Contains("\t") || argument.Contains("\n");
    }
}