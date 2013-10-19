using System;
using System.Collections.Generic;

namespace Fixie.Tests
{
    public static class RedirectedConsoleExtensions
    {
        public static IEnumerable<string> Lines(this RedirectedConsole console)
        {
            return console.Output.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}