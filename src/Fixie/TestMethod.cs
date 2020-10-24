namespace Fixie
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Threading.Tasks;
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

        async Task RunCoreAsync(object?[] parameters, object? instance, Action<Case>? inspectCase)
        {
            var @case = new Case(Method, parameters);

            await recorder.StartAsync(@case);

            Exception? caseInspectionFailure = null;
            Exception? disposalFailure = null;

            string output;
            using (var console = new RedirectedConsole())
            {
                if (instance != null)
                {
                    await TryRunCaseAsync(@case, instance);
                    TryInspectCase(@case, inspectCase, out caseInspectionFailure);
                }
                else
                {
                    try
                    {
                        var automaticInstance = @case.Method.IsStatic ? null : Construct(@case.Method.ReflectedType!);

                        await TryRunCaseAsync(@case, automaticInstance);
                        TryInspectCase(@case, inspectCase, out caseInspectionFailure);
                        disposalFailure = await TryDispose(automaticInstance);
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
                await recorder.SkipAsync(@case, output);
                accounted = true;
            }

            if (@case.State == CaseState.Failed)
            {
                await recorder.FailAsync(@case, output);
                accounted = true;
            }

            if (caseInspectionFailure != null)
            {
                await recorder.FailAsync(new Case(@case, caseInspectionFailure), output);
                accounted = true;
            }

            if (disposalFailure != null)
            {
                await recorder.FailAsync(new Case(@case, disposalFailure), output);
                accounted = true;
            }
            
            if (@case.State == CaseState.Passed && !accounted)
            {
                await recorder.PassAsync(@case, output);
            }

            RecordedResult = true;
        }

        static Task TryRunCaseAsync(Case @case, object? instance)
        {
            return @case.RunAsync(instance);
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

        static async Task<Exception?> TryDispose(object? automaticInstance)
        {
            Exception? disposalFailure = null;

            try
            {
                await automaticInstance.DisposeIfApplicableAsync();
            }
            catch (Exception exception)
            {
                disposalFailure = exception;
            }

            return disposalFailure;
        }

        public Task RunAsync(Action<Case>? inspectCase = null)
        {
            return RunCoreAsync(EmptyParameters, instance: null, inspectCase);
        }

        public Task RunAsync(object?[] parameters, Action<Case>? inspectCase = null)
        {
            return RunCoreAsync(parameters, instance: null, inspectCase);
        }

        public async Task RunCasesAsync(ParameterSource parameterSource, Action<Case>? inspectCase = null)
        {
            try
            {
                foreach (var parameters in GetCases(parameterSource))
                    await RunCoreAsync(parameters, instance: null, inspectCase);
            }
            catch (Exception exception)
            {
                await FailAsync(exception);
            }
        }

        public Task RunAsync(object? instance, Action<Case>? inspectCase = null)
        {
            return RunCoreAsync(EmptyParameters, instance, inspectCase);
        }

        public Task RunAsync(object?[] parameters, object? instance, Action<Case>? inspectCase = null)
        {
            return RunCoreAsync(parameters, instance, inspectCase);
        }

        public async Task RunCasesAsync(ParameterSource parameterSource, object? instance, Action<Case>? inspectCase = null)
        {
            try
            {
                foreach (var parameters in GetCases(parameterSource))
                    await RunCoreAsync(parameters, instance, inspectCase);
            }
            catch (Exception exception)
            {
                await FailAsync(exception);
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
        public async Task SkipAsync(string? reason = null)
        {
            await recorder.SkipAsync(this, reason);
            RecordedResult = true;
        }

        /// <summary>
        /// Emit a fail result for this test, with the given reason.
        /// </summary>
        public async Task FailAsync(Exception reason)
        {
            await recorder.FailAsync(this, reason);
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
                throw new PreservedException(exception);
            }
        }
    }
}