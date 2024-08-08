using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading.Channels;
using Fixie.Internal;
using Fixie.Reports;

namespace Fixie;

public class Test
{
    static readonly object[] EmptyParameters = [];

    readonly ChannelWriter<IMessage> channelWriter;

    internal Test(ChannelWriter<IMessage> channelWriter, MethodInfo method)
    {
        this.channelWriter = channelWriter;
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
    public Task<TestResult> Run()
    {
        return RunCore(instance: null, EmptyParameters);
    }

    /// <summary>
    /// Runs the test, using the given input parameters. The test will be run against a new
    /// instance of the test class, using its default constructor.
    /// </summary>
    public Task<TestResult> Run(object?[] parameters)
    {
        return RunCore(instance: null, parameters);
    }

    /// <summary>
    /// Runs the test against the given test class instance.
    /// </summary>
    public Task<TestResult> Run(object instance)
    {
        return RunCore(instance, EmptyParameters);
    }

    /// <summary>
    /// Runs the test against the given test class instance, using
    /// the given input parameters.
    /// </summary>
    public Task<TestResult> Run(object instance, object?[] parameters)
    {
        return RunCore(instance, parameters);
    }

    /// <summary>
    /// Emits a start event for this test.
    /// </summary>
    public async Task Start()
    {
        await ReportStart(this);
    }

    /// <summary>
    /// Emits a pass result for this test.
    /// </summary>
    public async Task Pass(TimeSpan? duration = null)
    {
        await Pass(EmptyParameters, duration);
    }

    /// <summary>
    /// Emits a pass result for this test case.
    /// </summary>
    public async Task Pass(object?[] parameters, TimeSpan? duration = null)
    {
        var name = GetName(Method, parameters);

        await ReportPass(this, name, duration ?? TimeSpan.Zero);

        RecordedResult = true;
    }

    /// <summary>
    /// Emits a skip result for this test, with the given reason.
    /// </summary>
    public async Task Skip(string reason, TimeSpan? duration = null)
    {
        await Skip(EmptyParameters, reason, duration);
    }

    /// <summary>
    /// Emits a skip result for this test case, with the given reason.
    /// </summary>
    public async Task Skip(object?[] parameters, string reason, TimeSpan? duration = null)
    {
        var name = GetName(Method, parameters);

        if (string.IsNullOrWhiteSpace(reason))
            reason = "This test was explicitly skipped, but no reason was provided.";

        await ReportSkip(this, name, reason, duration ?? TimeSpan.Zero);

        RecordedResult = true;
    }

    /// <summary>
    /// Emits a failure result for this test, with the given reason.
    /// </summary>
    public async Task Fail(Exception reason, TimeSpan? duration = null)
    {
        await Fail(EmptyParameters, reason, duration);
    }

    /// <summary>
    /// Emits a failure result for this test case, with the given reason.
    /// </summary>
    public async Task Fail(object?[] parameters, Exception reason, TimeSpan? duration = null)
    {
        if (reason == null)
            throw new ArgumentNullException(nameof(reason));

        var name = GetName(Method, parameters);

        await ReportFail(this, name, reason, duration ?? TimeSpan.Zero);

        RecordedResult = true;
    }

    async Task<TestResult> RunCore(object? instance, object?[] parameters)
    {
        var resolvedMethod = Method.TryResolveTypeArguments(parameters);
        var name = CaseNameBuilder.GetName(resolvedMethod, parameters);

        await ReportStart(this);

        var startTime = Stopwatch.GetTimestamp();

        try
        {
            if (instance == null && !resolvedMethod.IsStatic)
                instance = Construct(resolvedMethod.ReflectedType!);

            await resolvedMethod.CallResolvedMethod(instance, parameters);
        }
        catch (Exception failureReason)
        {
            await ReportFail(this, name, failureReason, Stopwatch.GetElapsedTime(startTime));
            RecordedResult = true;
            return TestResult.Failed(failureReason);
        }

        await ReportPass(this, name, Stopwatch.GetElapsedTime(startTime));
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

    async Task ReportStart(Test test)
    {
        var message = new TestStarted(test);
        await channelWriter.WriteAsync(message);
    }

    async Task ReportSkip(Test test, string name, string reason, TimeSpan duration)
    {
        var message = new TestSkipped(test.Name, name, duration, reason);
        await channelWriter.WriteAsync(message);
    }

    async Task ReportPass(Test test, string name, TimeSpan duration)
    {
        var message = new TestPassed(test.Name, name, duration);
        await channelWriter.WriteAsync(message);
    }

    async Task ReportFail(Test test, string name, Exception reason, TimeSpan duration)
    {
        var message = new TestFailed(test.Name, name, duration, reason);
        await channelWriter.WriteAsync(message);
    }
}