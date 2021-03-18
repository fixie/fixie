namespace Fixie
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;
    using System.Threading.Tasks;
    using Internal;
    using Reports;

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

        async Task<CaseCompleted> RunCoreAsync(object?[] parameters, object? instance)
        {
            var @case = new Case(Method, parameters);
            Exception? failureReason = null;

            await recorder.StartAsync(@case);

            string output;
            using (var console = new RedirectedConsole())
            {
                try
                {
                    if (instance == null && !@case.Method.IsStatic)
                        instance = Construct(@case.Method.ReflectedType!);

                    await @case.RunAsync(instance);
                }
                catch (Exception exception)
                {
                    if (exception is PreservedException preservedException)
                        exception = preservedException.OriginalException;

                    failureReason = exception;
                }

                output = console.Output;
            }

            Console.Write(output);

            CaseCompleted? result = null;
            if (failureReason != null)
            {
                result = await recorder.FailAsync(@case, output, failureReason);
            }
            else
            {
                result = await recorder.PassAsync(@case, output);
            }

            RecordedResult = true;

            return result;
        }

        public Task<CaseCompleted> RunAsync()
        {
            return RunCoreAsync(EmptyParameters, instance: null);
        }

        public Task<CaseCompleted> RunAsync(object?[] parameters)
        {
            return RunCoreAsync(parameters, instance: null);
        }

        public Task<CaseCompleted> RunAsync(object? instance)
        {
            return RunCoreAsync(EmptyParameters, instance);
        }

        public Task<CaseCompleted> RunAsync(object?[] parameters, object? instance)
        {
            return RunCoreAsync(parameters, instance);
        }

        public IEnumerable<object?[]> GetCases(ParameterSource parameterSource)
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

            if (reason is PreservedException preservedException)
                reason = preservedException.OriginalException;

            var @case = new Case(Method, parameters);

            await recorder.FailAsync(@case, "", reason);

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