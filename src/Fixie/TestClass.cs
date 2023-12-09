namespace Fixie;

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;

public class TestClass
{
    internal TestClass(Type type, IReadOnlyList<Test> tests)
    {
        Type = type;
        Tests = tests;
    }

    /// <summary>
    /// The test class under execution.
    /// </summary>
    public Type Type { get; }

    /// <summary>
    /// The tests under execution as discovered on this test class.
    /// </summary>
    public IReadOnlyList<Test> Tests { get; }

    /// <summary>
    /// Constructs an instance of the test class using
    /// the constructor that best matches the specified
    /// parameters.
    /// </summary>
    public object Construct(params object?[] parameters)
    {
        object? instance;

        try
        {
            instance = Activator.CreateInstance(Type, parameters);
        }
        catch (TargetInvocationException exception)
        {
            ExceptionDispatchInfo.Capture(exception.InnerException!).Throw();
            throw; // Unreachable.
        }

        if (instance == null)
        {
            throw new InvalidOperationException(
                "An attempt to construct an instance of test class '" +
                Type.FullName +
                "' unexpectedly resulted in null.");
        }

        return instance;
    }

    /// <summary>
    /// Emits failure results for all tests in the test class, with the given reason.
    /// </summary>
    public async Task Fail(Exception reason)
    {
        foreach (var test in Tests)
            await test.Fail(reason);
    }
}