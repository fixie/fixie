namespace Fixie.Samples.Skipped
{
    using System;

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class SkipAttribute : Attribute { }
}