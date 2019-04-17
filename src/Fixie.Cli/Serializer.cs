namespace Fixie.Cli
{
    using System;
    using System.Linq;
    using System.Text.RegularExpressions;

    static class Serializer
    {
        /// <summary>
        /// Serialize the given string[] to a single string, so that when used as a ProcessStartInfo.Arguments
        /// value, the process's Main method will receive the original string[].
        /// 
        /// See https://blogs.msdn.microsoft.com/twistylittlepassagesallalike/2011/04/23/everyone-quotes-command-line-arguments-the-wrong-way/
        /// See https://stackoverflow.com/a/6040946 for the regex approach used here.
        /// </summary>
        public static string Serialize(string[] arguments)
            => string.Join(" ", arguments.Select(Quote));

        static string Quote(string argument)
        {
            //For each substring of zero or more \ followed by "
            //replace it with twice as many \ followed by \"
            var s = Regex.Replace(argument, @"(\\*)" + '"', @"$1$1\" + '"');

            //When an argument ends in \ double the number of \ at the end.
            s = Regex.Replace(s, @"(\\+)$", @"$1$1");

            //Now that the content has been escaped, surround the value in quotes.
            return '"' + s + '"';
        }
    }
}