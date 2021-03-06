namespace Fixie
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.ExceptionServices;
    using System.Threading.Tasks;
    using Internal;

    public class Test
    {
        static readonly object[] EmptyParameters = {};

        readonly ExecutionRecorder recorder;

        internal Test(ExecutionRecorder recorder, MethodInfo method)
        {
            this.recorder = recorder;
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

        /// <summary>
        /// The method that defines this test.
        /// </summary>
        public MethodInfo Method { get; }

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
        public Task<TestResult> RunAsync()
        {
            return RunCoreAsync(instance: null, EmptyParameters);
        }

        /// <summary>
        /// Runs the test, using the given input parameters. The test will be run against a new
        /// instance of the test class, using its default constructor.
        /// </summary>
        public Task<TestResult> RunAsync(object?[] parameters)
        {
            return RunCoreAsync(instance: null, parameters);
        }

        /// <summary>
        /// Runs the test against the given test class instance.
        /// </summary>
        public Task<TestResult> RunAsync(object instance)
        {
            return RunCoreAsync(instance, EmptyParameters);
        }

        /// <summary>
        /// Runs the test against the given test class instance, using
        /// the given input parameters.
        /// </summary>
        public Task<TestResult> RunAsync(object instance, object?[] parameters)
        {
            return RunCoreAsync(instance, parameters);
        }

        /// <summary>
        /// Emits a pass result for this test.
        /// </summary>
        public async Task PassAsync()
        {
            await PassAsync(EmptyParameters);
        }

        /// <summary>
        /// Emits a pass result for this test case.
        /// </summary>
        public async Task PassAsync(object?[] parameters)
        {
            var name = GetName(Method, parameters);

            await recorder.PassAsync(this, name);

            RecordedResult = true;
        }

        /// <summary>
        /// Emits a skip result for this test, with the given reason.
        /// </summary>
        public async Task SkipAsync(string reason)
        {
            await SkipAsync(EmptyParameters, reason);
        }

        /// <summary>
        /// Emits a skip result for this test case, with the given reason.
        /// </summary>
        public async Task SkipAsync(object?[] parameters, string reason)
        {
            var name = GetName(Method, parameters);

            if (string.IsNullOrWhiteSpace(reason))
                reason = "This test was explicitly skipped, but no reason was provided.";

            await recorder.SkipAsync(this, name, reason);

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

            var name = GetName(Method, parameters);

            await recorder.FailAsync(this, name, reason);

            RecordedResult = true;
        }

        async Task<TestResult> RunCoreAsync(object? instance, object?[] parameters)
        {
            var resolvedMethod = Method.TryResolveTypeArguments(parameters);
            var name = CaseNameBuilder.GetName(resolvedMethod, parameters);

            await recorder.StartAsync(this);

            try
            {
                if (instance == null && !resolvedMethod.IsStatic)
                    instance = Construct(resolvedMethod.ReflectedType!);

                await resolvedMethod.CallResolvedMethodAsync(instance, parameters);
            }
            catch (Exception failureReason)
            {
                await recorder.FailAsync(this, name, failureReason);
                RecordedResult = true;
                return TestResult.Failed(failureReason);
            }

            await recorder.PassAsync(this, name);
            RecordedResult = true;
            return TestResult.Passed;
        }

        static object? Construct(Type testClass)
        {
            try
            {
                return Activator.CreateInstance(testClass);
            }
            catch (TargetInvocationException exception)
            {
                ExceptionDispatchInfo.Capture(exception.InnerException!).Throw();
                throw; // Unreachable.
            }
        }

        static string GetName(MethodInfo method, object?[] parameters)
            => CaseNameBuilder.GetName(method.TryResolveTypeArguments(parameters), parameters);
    }
}