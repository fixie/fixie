namespace Fixie
{
    using System;
    using System.Collections.Generic;
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

        bool? hasParameters;
        public bool HasParameters => hasParameters ??= Method.GetParameters().Length > 0;

        public MethodInfo Method { get; }

        internal bool Invoked { get; private set; }

        void RunCore(object?[] parameters, Action<Case>? caseLifecycle = null)
        {
            Invoked = true;

            var @case = new Case(Method, parameters);

            Exception? caseLifecycleFailure = null;

            string output;
            using (var console = new RedirectedConsole())
            {
                try
                {
                    if (caseLifecycle == null)
                        @case.Execute();
                    else
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

        public void Run(object?[] parameters, Action<Case>? caseLifecycle = null)
        {
            RunCore(parameters, caseLifecycle);
        }

        public void Run(Action<Case>? caseLifecycle = null)
        {
            RunCore(EmptyParameters, caseLifecycle);
        }

        public void RunCases(ParameterSource parameterSource, Action<Case>? caseLifecycle = null)
        {
            foreach (var parameters in GetCases(parameterSource))
                RunCore(parameters, caseLifecycle);
        }

        public void RunCases(ParameterSource parameterSource, object? instance)
        {
            foreach (var parameters in GetCases(parameterSource))
                RunCore(parameters, @case => @case.Execute(instance));
        }

        IEnumerable<object?[]> GetCases(ParameterSource parameterSource)
        {
            return HasParameters
                ? parameterSource(Method)
                : InvokeOnceWithZeroParameters;
        }

        /// <summary>
        /// Emit a skip result for this test, with the given reason.
        /// </summary>
        public void Skip(string? reason)
        {
            RunCore(EmptyParameters, @case =>
            {
                @case.Skip(reason);
            });
        }

        /// <summary>
        /// Emit a fail result for this test, with the given reason.
        /// </summary>
        public void Fail(Exception reason)
        {
            RunCore(EmptyParameters, @case =>
            {
                @case.Fail(reason);
            });
        }
    }
}