using System.Linq;
using System.Reflection;
using Should;

namespace Fixie.Tests
{
    public class MethodFilterTests
    {
        public void ConsidersOnlyPublicInstanceMethods()
        {
            var methods =
                new MethodFilter()
                    .Filter(typeof(Sample));

            methods
                .OrderBy(method => method.Name)
                .Select(method => method.Name)
                .ShouldEqual("PublicInstanceNoArgsVoid", "PublicInstanceNoArgsWithReturn", "PublicInstanceWithArgsVoid", "PublicInstanceWithArgsWithReturn");
        }

        public void ShouldFilterByAllSpecifiedConditions()
        {
            var methods =
                new MethodFilter()
                    .Where(method => method.Name.Contains("Void"))
                    .Where(method => method.Name.Contains("No"))
                    .Filter(typeof(Sample));

            methods
                .OrderBy(method => method.Name)
                .Select(method => method.Name)
                .ShouldEqual("PublicInstanceNoArgsVoid");
        }

        public void CanFilterToMethodsWithZeroParameters()
        {
            var methods =
                new MethodFilter()
                    .ZeroParameters()
                    .Filter(typeof(Sample));

            methods
                .OrderBy(method => method.Name)
                .Select(method => method.Name)
                .ShouldEqual("PublicInstanceNoArgsVoid", "PublicInstanceNoArgsWithReturn");
        }

        class Sample
        {
            public static int PublicStaticWithArgsWithReturn(int x) { return 0; }
            public static int PublicStaticNoArgsWithReturn() { return 0; }
            public static void PublicStaticWithArgsVoid(int x) { }
            public static void PublicStaticNoArgsVoid() { }

            public int PublicInstanceWithArgsWithReturn(int x) { return 0; }
            public int PublicInstanceNoArgsWithReturn() { return 0; }
            public void PublicInstanceWithArgsVoid(int x) { }
            public void PublicInstanceNoArgsVoid() { }

            private static int PrivateStaticWithArgsWithReturn(int x) { return 0; }
            private static int PrivateStaticNoArgsWithReturn() { return 0; }
            private static void PrivateStaticWithArgsVoid(int x) { }
            private static void PrivateStaticNoArgsVoid() { }

            private int PrivateInstanceWithArgsWithReturn(int x) { return 0; }
            private int PrivateInstanceNoArgsWithReturn() { return 0; }
            private void PrivateInstanceWithArgsVoid(int x) { }
            private void PrivateInstanceNoArgsVoid() { }
        }
    }
}