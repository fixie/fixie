using System;
using System.Threading.Tasks;

// The 'Fixie' package includes this file in test projects so
// that their tests can be executed. Do not modify this file.

file class TestProjectEntryPoint
{
    static async Task<int> Main(string[] customArguments)
        => await Fixie.Internal.EntryPoint.Main(typeof(TestProjectEntryPoint).Assembly, customArguments);
}