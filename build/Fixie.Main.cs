// The 'Fixie' package includes this file in test projects so
// that their tests can be executed. Do not modify this file.

namespace Fixie;

file static class Program
{
    static async Task<int> Main(string[] args)
        => await Fixie.Internal.EntryPoint.Main(typeof(Program).Assembly, customArguments: args);
}