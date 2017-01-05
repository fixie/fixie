namespace Fixie.Tests
{
    using System.Runtime.CompilerServices;

    public class Utility
    {
        public static string FullName<T>()
           => typeof(T).FullName;

        public static string At<T>(string method, [CallerFilePath] string path = null)
        {
            if (typeof(T) == typeof(SampleTestClass))
                path = SampleTestClass.FilePath();

            return $"   at {FullName<T>().Replace("+", ".")}.{method} in {path}:line #";
        }

        public static string PathToThisFile([CallerFilePath] string path = null)
            => path;
    }
}
