namespace Fixie.Internal;

// The 'Fixie' package includes this file in test projects so
// that their tests can be executed. Do not modify this file.

class TestProjectEntryPoint
{
    [STAThread]
    static async Task<int> Main(string[] customArguments)
        => await EntryPoint.Main(typeof(TestProjectEntryPoint).Assembly, customArguments);
}