namespace Fixie
{
    using System;
    using System.Reflection;
    using Internal;

    public class TestMethod
    {
        readonly ExecutionRecorder recorder;
        readonly ParameterGenerator parameterGenerator;

        internal TestMethod(ExecutionRecorder recorder, ParameterGenerator parameterGenerator, MethodInfo method)
        {
            this.recorder = recorder;
            this.parameterGenerator = parameterGenerator;
            Method = method;
        }

        public MethodInfo Method { get; }

        public void Run(object?[] parameters, Action<Case> caseLifecycle)
        {
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
    }
}