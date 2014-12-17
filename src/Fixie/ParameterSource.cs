using System.Collections.Generic;
using System.Reflection;

namespace Fixie
{
    /// <summary>
    /// Defines a source of test case input parameters.
    /// Given a test case method, yields zero or more sets
    /// of method parameters to be passed into the test method.
    /// Each object array returned represents a distinct
    /// invocation of the test method.
    /// </summary>
    public interface ParameterSource
    {
        IEnumerable<object[]> GetParameters(MethodInfo method);
    }
}