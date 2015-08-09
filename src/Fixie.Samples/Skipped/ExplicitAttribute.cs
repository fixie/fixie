using System;

namespace Fixie.Samples.Skipped
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public class ExplicitAttribute : Attribute { }
}