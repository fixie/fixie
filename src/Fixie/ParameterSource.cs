namespace Fixie
{
    using System.Collections.Generic;
    using System.Reflection;

    /// <summary>
    /// Defines a source of test case input parameters.
    /// 
    /// <para>
    /// Given a test method, yields zero or more sets
    /// of method parameters to be passed into the test method.
    /// Each object array returned represents a distinct
    /// invocation of the test method.
    /// </para>
    /// </summary>
    public interface ParameterSource
    {
        IEnumerable<object?[]> GetParameters(MethodInfo method);
    }
}