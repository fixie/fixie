namespace Fixie.Internal
{
    using System;

    class Program
    {
        [STAThread]
        static int Main(string[] arguments) => AssemblyRunner.Main(typeof(Program).Assembly, arguments);
    }
}