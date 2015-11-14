using System;

namespace Fixie.Samples.Skipped
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class ExplicitAttribute : Attribute { }
}