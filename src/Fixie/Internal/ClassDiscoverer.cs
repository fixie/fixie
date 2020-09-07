namespace Fixie.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;

    class ClassDiscoverer
    {
        readonly Discovery discovery;
        readonly IReadOnlyList<Func<Type, bool>> testClassConditions;

        public ClassDiscoverer(Discovery discovery)
        {
            this.discovery = discovery;

            testClassConditions = new List<Func<Type, bool>>
            {
                ConcreteClasses,
                NonCustomizationClasses,
                NonCompilerGeneratedClasses
            };
        }

        public IReadOnlyList<Type> TestClasses(IEnumerable<Type> candidates)
        {
            try
            {
                return discovery.TestClasses(candidates.Where(IsApplicable)).ToList();
            }
            catch (Exception exception)
            {
                throw new Exception(
                    "Exception thrown while attempting to run a custom class-discovery predicate. " +
                    "Check the inner exception for more details.", exception);
            }
        }

        bool IsApplicable(Type candidate)
            => testClassConditions.All(condition => condition(candidate));

        static bool ConcreteClasses(Type type)
            => type.IsClass && (!type.IsAbstract || type.IsStatic());

        static bool NonCustomizationClasses(Type type)
            => !IsDiscovery(type) && !IsExecution(type);

        static bool IsDiscovery(Type type)
            => type.GetInterfaces().Contains(typeof(Discovery));

        static bool IsExecution(Type type)
            => type.GetInterfaces().Contains(typeof(Execution));

        static bool NonCompilerGeneratedClasses(Type type)
            => !type.Has<CompilerGeneratedAttribute>();
    }
}