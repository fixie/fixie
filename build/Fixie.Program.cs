namespace Fixie.Internal
{
    using System;
    using System.Threading.Tasks;

    class Program
    {
        [STAThread]
        static async Task<int> Main(string[] customArguments)
            => await EntryPoint.Main(typeof(Program).Assembly, customArguments);
    }
}