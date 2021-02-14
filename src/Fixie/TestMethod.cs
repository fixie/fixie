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
        readonly bool classIsDisposable;

        internal TestMethod(ExecutionRecorder recorder, bool classIsDisposable, MethodInfo method)
        {
            this.recorder = recorder;
            this.classIsDisposable = classIsDisposable;
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

        async Task<Exception?> RunCoreAsync(object?[] parameters, object? instance)
        {
            var @case = new Case(Method, parameters);

            await recorder.StartAsync(@case);

            string output;
            using (var console = new RedirectedConsole())
            {
                try
                {
                    if (instance == null && !@case.Method.IsStatic)
                        instance = Construct(@case.Method.ReflectedType!);

                    try
                    {
                        await @case.RunAsync(instance);
                        @case.Pass();
                    }
                    catch (Exception exception)
                    {
                        @case.Fail(exception);
                    }
                }
                catch (Exception constructionFailure)
                {
                    @case.Fail(constructionFailure);
                }

                output = console.Output;
            }

            Console.Write(output);

            if (@case.State == CaseState.Skipped)
            {
                await recorder.SkipAsync(@case, output, null);
            }
            else if (@case.State == CaseState.Failed)
            {
                await recorder.FailAsync(@case, output);
            }
            else
            {
                await recorder.PassAsync(@case, output);
            }

            RecordedResult = true;

            return @case.Exception;
        }

        public Task<Exception?> RunAsync()
        {
            return RunCoreAsync(EmptyParameters, instance: null);
        }

        public Task<Exception?> RunAsync(object?[] parameters)
        {
            return RunCoreAsync(parameters, instance: null);
        }

        public async Task RunAsync(ParameterSource parameterSource)
        {
            try
            {
                foreach (var parameters in GetCases(parameterSource))
                    await RunCoreAsync(parameters, instance: null);
            }
            catch (Exception exception)
            {
                await FailAsync(exception);
            }
        }

        public Task<Exception?> RunAsync(object? instance)
        {
            return RunCoreAsync(EmptyParameters, instance);
        }

        public Task<Exception?> RunAsync(object?[] parameters, object? instance)
        {
            return RunCoreAsync(parameters, instance);
        }

        public async Task RunAsync(ParameterSource parameterSource, object? instance)
        {
            try
            {
                foreach (var parameters in GetCases(parameterSource))
                    await RunCoreAsync(parameters, instance);
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
        public async Task SkipAsync(string? reason)
        {
            await SkipAsync(EmptyParameters, reason);
        }

        /// <summary>
        /// Emit a skip result for this test, with the given reason.
        /// </summary>
        public async Task SkipAsync(object?[] parameters, string? reason)
        {
            var @case = new Case(Method, parameters);
            @case.Skip();
            await recorder.SkipAsync(@case, "", reason);

            RecordedResult = true;
        }

        /// <summary>
        /// Emit a fail result for this test, with the given reason.
        /// </summary>
        public async Task FailAsync(Exception reason)
        {
            await FailAsync(EmptyParameters, reason);
        }

        /// <summary>
        /// Emit a fail result for this test, with the given reason.
        /// </summary>
        public async Task FailAsync(object?[] parameters, Exception reason)
        {
            if (reason == null)
                throw new ArgumentNullException(nameof(reason));

            var @case = new Case(Method, parameters);
            @case.Fail(reason);
            await recorder.FailAsync(@case, "");

            RecordedResult = true;
        }

        object? Construct(Type testClass)
        {
            if (classIsDisposable)
                FailDueToDisposalMisuse(testClass);

            try
            {
                return Activator.CreateInstance(testClass);
            }
            catch (TargetInvocationException exception)
            {
                throw new PreservedException(exception);
            }
        }

        static void FailDueToDisposalMisuse(Type testClass)
        {
            throw new Exception(
                $"Test class {testClass} is declared as disposable, which is firmly discouraged " +
                "for test tear-down purposes. Test class disposal is not supported when the test " +
                "runner is constructing test class instances implicitly. If you wish to use " +
                "IDisposable or IDisposableAsync for test class tear down, perform construction " +
                "and disposal explicitly in an implementation of Execution.RunAsync(...).");
        }
    }
}