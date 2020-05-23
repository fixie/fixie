namespace Fixie.Internal
{
    using System;

    class Program
    {
        [STAThread]
        static int Main(string[] arguments) => EntryPoint.Main(typeof(Program).Assembly, arguments);
    }
}