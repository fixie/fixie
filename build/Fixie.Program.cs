namespace Fixie.EntryPoint
{
    using System;
    using Execution;

    class Program
    {
        [STAThread]
        static int Main(string[] arguments) => AssemblyRunner.Main(arguments);
    }
}