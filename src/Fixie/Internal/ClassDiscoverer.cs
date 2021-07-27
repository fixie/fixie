namespace Fixie.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;

    class ClassDiscoverer
    {
        readonly IDiscovery discovery;

        public ClassDiscoverer(IDiscovery discovery)
            => this.discovery = discovery;

        public IReadOnlyList<Type> TestClasses(IEnumerable<Type> candidates)
        {
            try
            {
                return discovery.TestClasses(candidates.Where(IsApplicable)).ToList();
            }
            catch (Exception exception)
            {
                throw new Exception(
                    "Exception thrown during test class discovery. " +
                    "Check the inner exception for more details.", exception);
            }
        }

        static bool IsApplicable(Type candidate)
        {
            return ConcreteClasses(candidate) &&
                   NonCustomizationClasses(candidate) &&
                   NonCompilerGeneratedClasses(candidate);
        }

        static bool ConcreteClasses(Type type)
            => type.IsClass && (!type.IsAbstract || type.IsStatic());

        static bool NonCustomizationClasses(Type type)
            => !IsDiscovery(type) && !IsExecution(type) && !IsTestProject(type);

        static bool IsDiscovery(Type type)
            => type.GetInterfaces().Contains(typeof(IDiscovery));

        static bool IsExecution(Type type)
            => type.GetInterfaces().Contains(typeof(IExecution));

        static bool IsTestProject(Type type)
            => type.GetInterfaces().Contains(typeof(ITestProject));

        static bool NonCompilerGeneratedClasses(Type type)
            => !type.Has<CompilerGeneratedAttribute>();
    }
}