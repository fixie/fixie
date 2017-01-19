namespace Fixie.Cli
{
    using System;

    public class CommandLineException : Exception
    {
        public CommandLineException(string message)
            : base(message) { }
    }
}
