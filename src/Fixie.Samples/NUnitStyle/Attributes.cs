using System;

namespace Fixie.Samples.NUnitStyle
{
    [AttributeUsage(AttributeTargets.Class)]
    public class TestFixtureAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Method)]
    public class TestAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Method)]
    public class SetUpAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Method)]
    public class TearDownAttribute : Attribute { }
}