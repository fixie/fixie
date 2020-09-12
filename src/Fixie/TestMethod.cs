namespace Fixie
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.ExceptionServices;
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

            recorder.Start(@case);

            Exception? caseInspectionFailure = null;
            Exception? disposalFailure = null;

            string output;
            using (var console = new RedirectedConsole())
            {
                if (instance != null)
                {
                    TryRunCase(@case, instance);
                    TryInspectCase(@case, inspectCase, out caseInspectionFailure);
                }
                else
                {
                    try
                    {
                        var automaticInstance = @case.Method.IsStatic ? null : Construct(@case.Method.ReflectedType!);

                        TryRunCase(@case, automaticInstance);
                        TryInspectCase(@case, inspectCase, out caseInspectionFailure);
                        TryDispose(automaticInstance, out disposalFailure);
                    }
                    catch (Exception constructionFailure)
                    {
                        @case.Fail(constructionFailure);

                        TryInspectCase(@case, inspectCase, out caseInspectionFailure);
                    }
                }

                output = console.Output;
            }

            Console.Write(output);

            bool accounted = false;
            if (@case.State == CaseState.Skipped)
            {
                recorder.Skip(@case, output);
                accounted = true;
            }

            if (@case.State == CaseState.Failed)
            {
                recorder.Fail(@case, output);
                accounted = true;
            }

            if (caseInspectionFailure != null)
            {
                recorder.Fail(new Case(@case, caseInspectionFailure), output);
                accounted = true;
            }

            if (disposalFailure != null)
            {
                recorder.Fail(new Case(@case, disposalFailure), output);
                accounted = true;
            }
            
            if (@case.State == CaseState.Passed && !accounted)
            {
                recorder.Pass(@case, output);
            }

            RecordedResult = true;
        }

        static void TryRunCase(Case @case, object? instance)
        {
            @case.Execute(instance);
        }

        static void TryInspectCase(Case @case, Action<Case>? inspectCase, out Exception? caseInspectionFailure)
        {
            caseInspectionFailure = null;

            try
            {
                inspectCase?.Invoke(@case);
            }
            catch (Exception exception)
            {
                caseInspectionFailure = exception;
            }
        }

        static void TryDispose(object? automaticInstance, out Exception? disposalFailure)
        {
            disposalFailure = null;

            try
            {
                automaticInstance.Dispose();
            }
            catch (Exception exception)
            {
                disposalFailure = exception;
            }
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
            try
            {
                foreach (var parameters in GetCases(parameterSource))
                    RunCore(parameters, instance: null, inspectCase);
            }
            catch (Exception exception)
            {
                Fail(exception);
            }
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
            try
            {
                foreach (var parameters in GetCases(parameterSource))
                    RunCore(parameters, instance, inspectCase);
            }
            catch (Exception exception)
            {
                Fail(exception);
            }
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

        static object? Construct(Type type)
        {
            try
            {
                return Activator.CreateInstance(type);
            }
            catch (TargetInvocationException exception)
            {
                ExceptionDispatchInfo.Capture(exception.InnerException!).Throw();
                throw; //Unreachable.
            }
        }
    }
}