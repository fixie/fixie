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
            return RunTypes(assembly.GetTypes());
        }

        public Result RunNamespace(Assembly assembly, string ns)
        {
            return RunTypes(assembly.GetTypes().Where(InNamespace(ns)).ToArray());
        }

        public Result RunTypes(params Type[] types)
        {
            var convention = new DefaultConvention();

            return Run(convention, types);
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
            var resultListener = new ResultListener(listener);

            suite.Execute(resultListener);

            var result = resultListener.Result;

            resultListener.RunComplete(result);

            return result;
        }

        static Func<Type, bool> InNamespace(string ns)
        {
            return type => type.Namespace != null && type.Namespace.StartsWith(ns);
        }

        class ResultListener : Listener
        {
            int passed;
            int failed;
            readonly Listener inner;

            public ResultListener(Listener inner)
            {
                this.inner = inner;
            }

            public void CasePassed(Case @case)
            {
                passed++;
                inner.CasePassed(@case);
            }

            public void CaseFailed(Case @case, Exception ex)
            {
                failed++;
                inner.CaseFailed(@case, ex);
            }

            public void RunComplete(Result result)
            {
                inner.RunComplete(result);
            }

            public Result Result
            {
                get { return new Result(passed, failed); }
            }
        }
    }
}