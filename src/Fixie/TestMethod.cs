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
            RecordedResult = false;
        }

        bool? hasParameters;
        public bool HasParameters => hasParameters ??= Method.GetParameters().Length > 0;

        public MethodInfo Method { get; }

        internal bool RecordedResult { get; private set; }

        void RunCore(object?[] parameters, object? instance, Action<Case>? inspectCase)
        {
            var @case = new Case(Method, parameters);

            Exception? caseLifecycleFailure = null;

            string output;
            using (var console = new RedirectedConsole())
            {
                try
                {
                    if (instance != null)
                        @case.Execute(instance);
                    else
                        @case.Execute();

                    inspectCase?.Invoke(@case);
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
            
            RecordedResult = true;
        }

        public void Run(Action<Case>? inspectCase = null)
        {
            RunCore(EmptyParameters, instance: null, inspectCase);
        }

        public void Run(object?[] parameters, Action<Case>? inspectCase = null)
        {
            RunCore(parameters, instance: null, inspectCase);
        }

        public void RunCases(ParameterSource parameterSource, Action<Case>? inspectCase = null)
        {
            foreach (var parameters in GetCases(parameterSource))
                RunCore(parameters, instance: null, inspectCase);
        }

        public void Run(object? instance, Action<Case>? inspectCase = null)
        {
            RunCore(EmptyParameters, instance, inspectCase);
        }

        public void Run(object?[] parameters, object? instance, Action<Case>? inspectCase = null)
        {
            RunCore(parameters, instance, inspectCase);
        }

        public void RunCases(ParameterSource parameterSource, object? instance, Action<Case>? inspectCase = null)
        {
            foreach (var parameters in GetCases(parameterSource))
                RunCore(parameters, instance, inspectCase);
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
        public void Skip(string? reason = null)
        {
            recorder.Skip(this, reason);
            RecordedResult = true;
        }

        /// <summary>
        /// Emit a fail result for this test, with the given reason.
        /// </summary>
        public void Fail(Exception reason)
        {
            recorder.Fail(this, reason);
            RecordedResult = true;
        }
    }
}