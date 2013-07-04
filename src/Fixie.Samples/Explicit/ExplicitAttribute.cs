using System;

namespace Fixie.Samples.Explicit
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public class ExplicitAttribute : Attribute { }
}