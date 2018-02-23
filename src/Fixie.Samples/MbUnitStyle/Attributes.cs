namespace Fixie.Samples.MbUnitStyle
{
    using System;

    [AttributeUsage(AttributeTargets.Class)]
    public class TestFixture : Attribute { }

    [AttributeUsage(AttributeTargets.Method)]
    public class FixtureSetUp : Attribute { }

    [AttributeUsage(AttributeTargets.Method)]
    public class SetUp : Attribute { }

    [AttributeUsage(AttributeTargets.Method)]
    public class Test : Attribute { }

    [AttributeUsage(AttributeTargets.Method)]
    public class TearDown : Attribute { }

    [AttributeUsage(AttributeTargets.Method)]
    public class FixtureTearDown : Attribute { }

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

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    class RowAttribute : Attribute
    {
        public RowAttribute(params object[] parameters)
        {
            Parameters = parameters;
        }

        public object[] Parameters { get; }
    }
    
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    class ColumnAttribute : Attribute
    {
        public ColumnAttribute(params object[] parameters)
        {
            Parameters = parameters;
        }

        public object[] Parameters { get; }
    }
}