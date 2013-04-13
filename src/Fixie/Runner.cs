using System;
using System.Linq;
using System.Reflection;

namespace Fixie
{
    public class Runner
    {
        readonly Listener listener;

        public Runner(Listener listener)
        {
            this.listener = listener;
        }

        public Result RunAssembly(Assembly assembly)
        {
            var convention = new DefaultConvention();

            return Run(convention, assembly.GetTypes());
        }

        public Result RunNamespace(Assembly assembly, string ns)
        {
            var convention = new DefaultConvention();

            return Run(convention, assembly.GetTypes().Where(InNamespace(ns)).ToArray());
        }

        public Result RunType(Type type)
        {
            var convention = new DefaultConvention();

            return Run(convention, type);
        }

        public Result RunMethod(MethodInfo method)
        {
            var convention = new DefaultConvention();

            convention.Cases.Where(m => m == method);

            return Run(convention, method.DeclaringType);
        }

        Result Run(Convention convention, params Type[] candidateTypes)
        {
            var suite = new Suite(convention, candidateTypes);

            suite.Execute(listener);

            listener.RunComplete();

            return listener.State.ToResult();
        }

        static Func<Type, bool> InNamespace(string ns)
        {
            return type => type.Namespace != null && type.Namespace.StartsWith(ns);
        }
    }
}