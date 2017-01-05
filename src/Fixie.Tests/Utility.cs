namespace Fixie.Tests
{
    using System.Runtime.CompilerServices;

    public class Utility
    {
        public static string PathToThisFile([CallerFilePath] string path = null) => path;
    }
}
