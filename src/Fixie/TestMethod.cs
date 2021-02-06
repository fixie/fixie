namespace Fixie
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
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

        public bool Has<TAttribute>() where TAttribute : Attribute
            => Method.Has<TAttribute>();

        public bool Has<TAttribute>([NotNullWhen(true)] out TAttribute? matchingAttribute) where TAttribute : Attribute
            => Method.Has(out matchingAttribute);

        async Task<Exception?> RunCoreAsync(object?[] parameters, object? instance, Action<Case>? inspectFailure)
        {
            var @case = new Case(Method, parameters);

            await recorder.StartAsync(@case);

            Exception? inspectionFailure = null;
            Exception? disposalFailure = null;

            string output;
            using (var console = new RedirectedConsole())
            {
                if (instance != null)
                {
                    await TryRunCaseAsync(@case, instance);
                    TryInspectFailures(@case, inspectFailure, out inspectionFailure);
                }
                else
                {
                    try
                    {
                        var automaticInstance = @case.Method.IsStatic ? null : Construct(@case.Method.ReflectedType!);

                        await TryRunCaseAsync(@case, automaticInstance);
                        TryInspectFailures(@case, inspectFailure, out inspectionFailure);
                        disposalFailure = await TryDisposeAsync(automaticInstance);
                    }
                    catch (Exception constructionFailure)
                    {
                        @case.Fail(constructionFailure);

                        TryInspectFailures(@case, inspectFailure, out inspectionFailure);
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

            if (inspectionFailure != null)
            {
                await recorder.FailAsync(new Case(@case, inspectionFailure), output);
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

            return @case.Exception;
        }

        static Task TryRunCaseAsync(Case @case, object? instance)
        {
            return @case.RunAsync(instance);
        }

        static void TryInspectFailures(Case @case, Action<Case>? inspectFailure, out Exception? inspectionFailure)
        {
            inspectionFailure = null;

            if (@case.State != CaseState.Failed)
                return;

            try
            {
                inspectFailure?.Invoke(@case);
            }
            catch (Exception exception)
            {
                inspectionFailure = exception;
            }
        }

        static async Task<Exception?> TryDisposeAsync(object? automaticInstance)
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

        public Task<Exception?> RunAsync(Action<Case>? inspectFailure = null)
        {
            return RunCoreAsync(EmptyParameters, instance: null, inspectFailure);
        }

        public Task<Exception?> RunAsync(object?[] parameters, Action<Case>? inspectFailure = null)
        {
            return RunCoreAsync(parameters, instance: null, inspectFailure);
        }

        public async Task RunAsync(ParameterSource parameterSource, Action<Case>? inspectFailure = null)
        {
            try
            {
                foreach (var parameters in GetCases(parameterSource))
                    await RunCoreAsync(parameters, instance: null, inspectFailure);
            }
            catch (Exception exception)
            {
                await FailAsync(exception);
            }
        }

        public Task<Exception?> RunAsync(object? instance, Action<Case>? inspectFailure = null)
        {
            return RunCoreAsync(EmptyParameters, instance, inspectFailure);
        }

        public Task<Exception?> RunAsync(object?[] parameters, object? instance, Action<Case>? inspectFailure = null)
        {
            return RunCoreAsync(parameters, instance, inspectFailure);
        }

        public async Task RunAsync(ParameterSource parameterSource, object? instance, Action<Case>? inspectFailure = null)
        {
            try
            {
                foreach (var parameters in GetCases(parameterSource))
                    await RunCoreAsync(parameters, instance, inspectFailure);
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
            if (reason == null)
                throw new ArgumentNullException(nameof(reason));

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