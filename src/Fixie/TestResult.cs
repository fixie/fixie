namespace Fixie;

/// <summary>
/// A discriminated union representing the result of a single run of a test.
/// <para>Use pattern matching to inspect:</para>
/// <list type="bullet">
///     <item>Passed</item>
///     <item>Failed (Exception Reason)</item>
/// </list>
/// </summary>
public abstract class TestResult
{
    internal static readonly Passed Passed = new();
    internal static Failed Failed(Exception reason) => new(reason);
}

/// <summary>
/// Represents the fact that a test has passed.
/// </summary>
public class Passed : TestResult
{
    internal Passed() { }
}

/// <summary>
/// Represents the fact that a test has failed due to its throwing an exception.
/// </summary>
public class Failed : TestResult
{
    internal Failed(Exception reason) => Reason = reason;

    /// <summary>
    /// The exception thrown by a failing test.
    /// </summary>
    public Exception Reason { get; }
}