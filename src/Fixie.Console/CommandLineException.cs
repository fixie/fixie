using System;

namespace Fixie.Console;

public class CommandLineException : Exception
{
    public CommandLineException(string message, Exception innerException)
        : base(message, innerException) { }

    public CommandLineException(string message)
        : base(message) { }
}