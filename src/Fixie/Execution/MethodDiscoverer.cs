namespace Fixie.Execution
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    class MethodDiscoverer
    {
        readonly Filter filter;
        readonly IReadOnlyList<Func<MethodInfo, bool>> testMethodConditions;

        public MethodDiscoverer(Filter filter, Convention convention)
        {
            this.filter = filter;
            testMethodConditions = convention.Config.TestMethodConditions;
        }

        public IReadOnlyList<MethodInfo> TestMethods(Type testClass)
        {
            try
            {
                bool testClassIsDisposable = IsDisposable(testClass);

                return testClass
                    .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                    .Where(method => method.DeclaringType != typeof(object))
                    .Where(method => !(testClassIsDisposable && HasDisposeSignature(method)))
                    .Where(IsMatch)
                    .Where(method => filter.IsSatisfiedBy(new MethodGroup(testClass, method)))
                    .ToArray();
            }
            catch (Exception exception)
            {
                throw new Exception(
                    "Exception thrown while attempting to run a custom method-discovery predicate. " +
                    "Check the inner exception for more details.", exception);
            }
        }

        bool IsMatch(MethodInfo candidate)
            => testMethodConditions.All(condition => condition(candidate));

        static bool IsDisposable(Type type)
            => type.GetInterfaces().Any(interfaceType => interfaceType == typeof(IDisposable));

        static bool HasDisposeSignature(MethodInfo method)
            => method.Name == "Dispose" && method.IsVoid() && method.GetParameters().Length == 0;
    }
}