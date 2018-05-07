namespace Fixie.EntryPoint
{
    using System;
    using Internal;

    class Program
    {
        [STAThread]
        static int Main(string[] arguments) => AssemblyRunner.Main(arguments);
    }
}