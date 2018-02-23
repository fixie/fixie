namespace Fixie.Samples.NUnitStyle
{
    using System;

    [AttributeUsage(AttributeTargets.Class)]
    public class TestFixture : Attribute { }

    [AttributeUsage(AttributeTargets.Method)]
    public class TestFixtureSetUp : Attribute { }

    [AttributeUsage(AttributeTargets.Method)]
    public class SetUp : Attribute { }

    [AttributeUsage(AttributeTargets.Method)]
    public class Test : Attribute { }

    [AttributeUsage(AttributeTargets.Method)]
    public class TearDown : Attribute { }

    [AttributeUsage(AttributeTargets.Method)]
    public class TestFixtureTearDown : Attribute { }

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
    public class TestCaseSourceAttribute : Attribute
    {
        public TestCaseSourceAttribute(string sourceName, Type sourceType)
        {
            SourceName = sourceName;
            SourceType = sourceType;
        }

        public TestCaseSourceAttribute(string sourceName)
        {
            SourceName = sourceName;
            SourceType = null;
        }

        public Type SourceType { get; set; }
        public string SourceName { get; private set; }
    }
}