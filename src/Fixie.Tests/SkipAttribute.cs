namespace Fixie.Tests;

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public class SkipAttribute(string reason) :
    Attribute
{
    public string Reason { get; } = reason;
}