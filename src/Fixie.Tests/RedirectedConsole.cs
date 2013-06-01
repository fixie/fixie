using System;
using System.Collections.Generic;
using System.IO;

namespace Fixie.Tests
{
    public class RedirectedConsole : IDisposable
    {
        readonly TextWriter before;
        readonly StringWriter log;

        public RedirectedConsole()
        {
            log = new StringWriter();
            before = Console.Out;
            Console.SetOut(log);
        }

        public IEnumerable<string> Lines
        {
            get { return log.ToString().Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries); }
        }

        public void Dispose()
        {
            Console.SetOut(before);
            log.Dispose();
        }

        public override string ToString()
        {
            return log.ToString();
        }
    }
}