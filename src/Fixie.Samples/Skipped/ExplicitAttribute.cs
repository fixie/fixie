using System;

namespace Fixie.Samples.Skipped
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public class ExplicitAttribute : Attribute { }
}