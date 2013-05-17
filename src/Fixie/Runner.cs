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
            return RunTypes(assembly, assembly.GetTypes());
        }

        public Result RunNamespace(Assembly assembly, string ns)
        {
            return RunTypes(assembly, assembly.GetTypes().Where(type => type.IsInNamespace(ns)).ToArray());
        }

        public Result RunType(Type type)
        {
            return RunTypes(type.Assembly, type);
        }

        public Result RunMethod(MethodInfo method)
        {
            var convention = new DefaultConvention();

            convention.Cases.Where(m => m == method);

            var type = method.DeclaringType;

            return Run(type.Assembly, convention, type);
        }

        private Result RunTypes(Assembly context, params Type[] types)
        {
            var convention = new DefaultConvention();

            return Run(context, convention, types);
        }

        Result Run(Assembly context, Convention convention, params Type[] candidateTypes)
        {
            var resultListener = new ResultListener(listener);

            resultListener.RunStarted(context);

            convention.Execute(resultListener, candidateTypes);

            var result = resultListener.Result;

            resultListener.RunComplete(result);

            return result;
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

            public void RunStarted(Assembly context)
            {
                passed = 0;
                failed = 0;
                inner.RunStarted(context);
            }

            public void CasePassed(string @case)
            {
                passed++;
                inner.CasePassed(@case);
            }

            public void CaseFailed(string @case, Exception[] exceptions)
            {
                failed++;
                inner.CaseFailed(@case, exceptions);
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