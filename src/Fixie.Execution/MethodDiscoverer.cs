namespace Fixie.Execution
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    public class MethodDiscoverer
    {
        readonly IReadOnlyList<Func<MethodInfo, bool>> testMethodConditions;

        public MethodDiscoverer(Convention convention)
        {
            var conditions = new List<Func<MethodInfo, bool>>
            {
                ExcludeMethodsDefinedOnObject,
                ExcludeDispose
            };

            conditions.AddRange(convention.Config.TestMethodConditions);

            testMethodConditions = conditions;
        }

        public IReadOnlyList<MethodInfo> TestMethods(Type testClass)
        {
            try
            {
                return testClass.GetMethods(BindingFlags.Public | BindingFlags.Instance).Where(IsMatch).ToArray();
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

        static bool ExcludeMethodsDefinedOnObject(MethodInfo method)
            => method.DeclaringType != typeof(object);

        static bool ExcludeDispose(MethodInfo method)
            => !IsDispose(method);

        static bool IsDispose(MethodInfo method)
            => method.ReflectedType.IsDisposable() && method.HasDisposeSignature();
    }
}