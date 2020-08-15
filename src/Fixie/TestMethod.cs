namespace Fixie
{
    using System;
    using System.Reflection;
    using Internal;

    public class TestMethod
    {
        static readonly object[] EmptyParameters = {};
        static readonly object[][] InvokeOnceWithZeroParameters = { EmptyParameters };

        readonly ExecutionRecorder recorder;

        internal TestMethod(ExecutionRecorder recorder, MethodInfo method)
        {
            this.recorder = recorder;
            Method = method;
            Invoked = false;
        }

        public MethodInfo Method { get; }

        internal bool Invoked { get; private set; }

        public void Run(object?[] parameters, Action<Case> caseLifecycle)
        {
            Invoked = true;

            var @case = new Case(Method, parameters);

            Exception? caseLifecycleFailure = null;

            string output;
            using (var console = new RedirectedConsole())
            {
                try
                {
                    caseLifecycle(@case);
                }
                catch (Exception exception)
                {
                    caseLifecycleFailure = exception;
                }

                output = console.Output;
            }

            Console.Write(output);

            if (@case.State == CaseState.Failed)
                recorder.Fail(@case, output);
            else if (@case.State == CaseState.Passed && caseLifecycleFailure == null)
                recorder.Pass(@case, output);

            if (caseLifecycleFailure != null)
                recorder.Fail(new Case(@case, caseLifecycleFailure));
            else if (@case.State == CaseState.Skipped)
                recorder.Skip(@case, output);
        }

        public void Run(ParameterGenerator generator, Action<Case> caseLifecycle)
        {
            var lazyInvocations = Method.GetParameters().Length == 0
                ? InvokeOnceWithZeroParameters
                : generator.GetParameters(Method);

            foreach (var parameters in lazyInvocations)
                Run(parameters, caseLifecycle);
        }
    }
}