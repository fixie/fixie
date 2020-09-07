namespace Fixie.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    class MethodDiscoverer
    {
        readonly Discovery discovery;

        public MethodDiscoverer(Discovery discovery)
            => this.discovery = discovery;

        public IReadOnlyList<MethodInfo> TestMethods(Type testClass)
        {
            try
            {
                bool testClassIsDisposable = IsDisposable(testClass);

                return discovery.TestMethods(
                        testClass
                            .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
                            .Where(method => method.DeclaringType != typeof(object))
                            .Where(method => !(testClassIsDisposable && HasDisposeSignature(method))))
                    .ToList();
            }
            catch (Exception exception)
            {
                throw new Exception(
                    "Exception thrown during test method discovery. " +
                    "Check the inner exception for more details.", exception);
            }
        }

        static bool IsDisposable(Type type)
            => type.GetInterfaces().Contains(typeof(IDisposable));

        static bool HasDisposeSignature(MethodInfo method)
            => method.Name == "Dispose" && method.IsVoid() && method.GetParameters().Length == 0;
    }
}