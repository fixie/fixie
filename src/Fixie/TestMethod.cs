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
                    @case.Execute(instance);

                    try
                    {
                        inspectCase?.Invoke(@case);
                    }
                    catch (Exception exception)
                    {
                        caseInspectionFailure = exception;
                    }
                }
                else
                {
                    try
                    {
                        var automaticInstance = @case.Method.IsStatic ? null : Construct(@case.Method.ReflectedType!);

                        @case.Execute(automaticInstance);

                        try
                        {
                            inspectCase?.Invoke(@case);
                        }
                        catch (Exception exception)
                        {
                            caseInspectionFailure = exception;
                        }

                        try
                        {
                            automaticInstance.Dispose();
                        }
                        catch (Exception exception)
                        {
                            // Because the case already has a primary
                            // result, capture the failure so that it
                            // can be recorded after the primary result.
                            disposalFailure = exception;
                        }
                    }
                    catch (Exception constructionFailure)
                    {
                        // Because a construction failure prevents
                        // executing the case method, we can safely
                        // record the failure directly on the case
                        // without risk of overwriting some other
                        // primary result.
                        @case.Fail(constructionFailure);

                        try
                        {
                            inspectCase?.Invoke(@case);
                        }
                        catch (Exception exception)
                        {
                            caseInspectionFailure = exception;
                        }
                    }
                }

                output = console.Output;
            }

            Console.Write(output);

            if (@case.State == CaseState.Skipped)
                recorder.Skip(@case, output);
            else if (@case.State == CaseState.Failed)
                recorder.Fail(@case, output);
            else if (@case.State == CaseState.Passed)
                recorder.Pass(@case, output);

            if (caseInspectionFailure != null)
                recorder.Fail(new Case(@case, caseInspectionFailure));

            if (disposalFailure != null)
                recorder.Fail(new Case(@case, disposalFailure));
            
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