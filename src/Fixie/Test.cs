namespace Fixie
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Internal;
    using Reports;

    public class Test
    {
        static readonly object[] EmptyParameters = {};

        readonly ExecutionRecorder recorder;
        readonly bool classIsDisposable;

        internal Test(ExecutionRecorder recorder, bool classIsDisposable, MethodInfo method)
        {
            this.recorder = recorder;
            this.classIsDisposable = classIsDisposable;
            Name = method.TestName();
            Method = method;
            RecordedResult = false;
        }

        /// <summary>
        /// Gets the full name of the test. Note that the full name of an individual test
        /// <em>case</em> may further clarify this name with parameters and/or generic type
        /// parameters.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the parameters of the test.
        /// </summary>
        public ParameterInfo[] Parameters => cachedParameters ??= Method.GetParameters();
        ParameterInfo[]? cachedParameters;

        /// <summary>
        /// Determines whether the test is parameterized.
        /// </summary>
        public bool HasParameters => Parameters.Length > 0;

        internal MethodInfo Method { get; }

        internal bool RecordedResult { get; private set; }

        /// <summary>
        /// Determines whether any attributes of the specified type are applied to the test declaration.
        /// </summary>
        public bool Has<TAttribute>() where TAttribute : Attribute
            => Method.Has<TAttribute>();

        /// <summary>
        /// Determines whether an attribute of the specified type is applied to the test declaration,
        /// providing that attribute as an `out` parameter when found.
        /// </summary>
        public bool Has<TAttribute>([NotNullWhen(true)] out TAttribute? matchingAttribute) where TAttribute : Attribute
            => Method.Has(out matchingAttribute);

        /// <summary>
        /// Gets all attributes of the specified type that are applied to the test declaration.
        /// </summary>
        public TAttribute[] GetAll<TAttribute>() where TAttribute : Attribute
            => Method.GetCustomAttributes<TAttribute>(true).ToArray();

        /// <summary>
        /// Runs the test. The test will be run against a new instance of the test class, using
        /// its default constructor.
        /// </summary> 
        public Task<CaseCompleted> RunAsync()
        {
            return RunCoreAsync(instance: null, EmptyParameters);
        }

        /// <summary>
        /// Runs the test, using the given input parameters. The test will be run against a new
        /// instance of the test class, using its default constructor.
        /// </summary>
        public Task<CaseCompleted> RunAsync(object?[] parameters)
        {
            return RunCoreAsync(instance: null, parameters);
        }

        /// <summary>
        /// Runs the test against the given test class instance.
        /// </summary>
        public Task<CaseCompleted> RunAsync(object? instance)
        {
            return RunCoreAsync(instance, EmptyParameters);
        }

        /// <summary>
        /// Runs the test against the given test class instance, using
        /// the given input parameters.
        /// </summary>
        public Task<CaseCompleted> RunAsync(object? instance, object?[] parameters)
        {
            return RunCoreAsync(instance, parameters);
        }

        /// <summary>
        /// Emits a skip result for this test, with the given reason.
        /// </summary>
        public async Task SkipAsync(string? reason)
        {
            await SkipAsync(EmptyParameters, reason);
        }

        /// <summary>
        /// Emits a skip result for this test case, with the given reason.
        /// </summary>
        public async Task SkipAsync(object?[] parameters, string? reason)
        {
            var @case = new Case(this, parameters);
            await recorder.SkipAsync(@case, "", reason);

            RecordedResult = true;
        }

        /// <summary>
        /// Emits a failure result for this test, with the given reason.
        /// </summary>
        public async Task FailAsync(Exception reason)
        {
            await FailAsync(EmptyParameters, reason);
        }

        /// <summary>
        /// Emits a failure result for this test case, with the given reason.
        /// </summary>
        public async Task FailAsync(object?[] parameters, Exception reason)
        {
            if (reason == null)
                throw new ArgumentNullException(nameof(reason));

            if (reason is PreservedException preservedException)
                reason = preservedException.OriginalException;

            var @case = new Case(this, parameters);

            await recorder.FailAsync(@case, "", reason);

            RecordedResult = true;
        }

        async Task<CaseCompleted> RunCoreAsync(object? instance, object?[] parameters)
        {
            var @case = new Case(this, parameters);
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