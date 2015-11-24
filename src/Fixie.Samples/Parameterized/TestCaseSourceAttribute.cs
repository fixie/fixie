using System;

namespace Fixie.Samples.Parameterized
{
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