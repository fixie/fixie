namespace Fixie.Tests
{
    using System;
    using Fixie.Execution;

    class Program
    {
        [STAThread]
        static int Main(string[] arguments)
        {
            return AssemblyRunner.Main(arguments);
        }
    }
}