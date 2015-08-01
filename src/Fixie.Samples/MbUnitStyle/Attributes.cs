using System;

namespace Fixie.Samples.MbUnitStyle
{
    [AttributeUsage(AttributeTargets.Class)]
    public class TestFixtureAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Method)]
    public class FixtureSetUpAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Method)]
    public class SetUpAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Method)]
    public class TestAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Method)]
    public class TearDownAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Method)]
    public class FixtureTearDownAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class ExpectedExceptionAttribute : Attribute
    {
        public ExpectedExceptionAttribute(Type exceptionType)
        {
            ExpectedException = exceptionType;
        }

        public Type ExpectedException { get; set; }

        public string ExpectedMessage { get; set; }
    }
}