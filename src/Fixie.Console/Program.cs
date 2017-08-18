namespace Fixie.Console
{
    using System;
    using Execution;

    class Program
    {
        [STAThread]
        static int Main(string[] arguments)
        {
            return AssemblyRunner.Main(arguments);
        }
    }
}