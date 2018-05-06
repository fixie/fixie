namespace Fixie.Execution
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;

    class ClassDiscoverer
    {
        readonly IReadOnlyList<Func<Type, bool>> testClassConditions;

        public ClassDiscoverer(Convention convention)
        {
            var conditions = new List<Func<Type, bool>>
            {
                ConcreteClasses,
                NonCustomizationClasses,
                NonCompilerGeneratedClasses
            };

            conditions.AddRange(convention.Config.TestClassConditions);

            testClassConditions = conditions;
        }

        public IReadOnlyList<Type> TestClasses(IEnumerable<Type> candidates)
        {
            try
            {
                return candidates.Where(IsMatch).ToArray();
            }
            catch (Exception exception)
            {
                throw new Exception(
                    "Exception thrown while attempting to run a custom class-discovery predicate. " +
                    "Check the inner exception for more details.", exception);
            }
        }

        bool IsMatch(Type candidate)
            => testClassConditions.All(condition => condition(candidate));

        static bool ConcreteClasses(Type type)
            => type.IsClass && (!type.IsAbstract || type.IsStatic());

        static bool NonCustomizationClasses(Type type)
            => !IsDiscovery(type) && !IsLifecycle(type);

        static bool IsDiscovery(Type type)
            => type.IsSubclassOf(typeof(Discovery));

        static bool IsLifecycle(Type type)
            => type.GetInterfaces().Contains(typeof(Lifecycle));

        static bool NonCompilerGeneratedClasses(Type type)
            => !type.Has<CompilerGeneratedAttribute>();
    }
}