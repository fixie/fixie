namespace Fixie.Internal
{
    using System;

    class Program
    {
        [STAThread]
        static int Main(string[] customArguments) => EntryPoint.Main(typeof(Program).Assembly, customArguments).GetAwaiter().GetResult();
    }
}