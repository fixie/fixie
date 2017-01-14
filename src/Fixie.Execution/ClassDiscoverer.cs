namespace Fixie.Execution
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;

    public class ClassDiscoverer
    {
        readonly IReadOnlyList<Func<Type, bool>> testClassConditions;

        public ClassDiscoverer(Convention convention)
        {
            var conditions = new List<Func<Type, bool>>()
            {
                ConcreteClasses,
                NonDiscoveryClasses,
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
            => type.IsClass && !type.IsAbstract;

        static bool NonDiscoveryClasses(Type type)
            => !type.IsSubclassOf(typeof(Convention));

        static bool NonCompilerGeneratedClasses(Type type)
            => !type.Has<CompilerGeneratedAttribute>();
    }
}