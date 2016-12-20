namespace Fixie.Tests.Internal
{
    using System;
    using System.Text;
    using Fixie.Internal;
    using Should;

    public class AssertionLibraryFilterTests
    {
        readonly Convention convention;

        public AssertionLibraryFilterTests()
        {
            convention = new Convention();
        }

        public void ShouldDoNothingWhenFilteringNullStackTrace()
        {
            const string nullStackTrace = null;

            convention
                .HideExceptionDetails
                .For(typeof(SampleAssertionLibrary.SampleAssert));

            AssertionLibraryFilter()
                .FilterStackTrace(new FakeException(nullStackTrace))
                .ShouldEqual(nullStackTrace);
        }

        public void ShouldFilterAssertionLibraryImplementationDetailsFromStackTraces()
        {
            var originalStackTrace =
                new StringBuilder()
                    .AppendLine(@"   at Fixie.Tests.SampleAssertionLibrary.SampleAssert.AreEqual(object x, object y) in c:\path\to\assertion\library\AssertHelpers.cs:line 10")
                    .AppendLine(@"   at Fixie.Tests.SampleAssertionLibrary.SampleAssert.AreEqual(Int32 x, Int32 y) in c:\path\to\assertion\library\Assert.cs:line 14")
                    .AppendLine(@"   at Your.Test.Project.TestClass.HelperMethodB() in c:\path\to\your\test\project\TestClass.cs:line 55")
                    .AppendLine(@"   at Your.Test.Project.TestClass.HelperMethodA() in c:\path\to\your\test\project\TestClass.cs:line 50")
                    .AppendLine(@"   at Your.Test.Project.TestClass.TestMethod() in c:\path\to\your\test\project\TestClass.cs:line 30")
                    .ToString()
                    .TrimEnd();

            var filteredStackTrace =
                new StringBuilder()
                    .AppendLine(@"   at Your.Test.Project.TestClass.HelperMethodB() in c:\path\to\your\test\project\TestClass.cs:line 55")
                    .AppendLine(@"   at Your.Test.Project.TestClass.HelperMethodA() in c:\path\to\your\test\project\TestClass.cs:line 50")
                    .AppendLine(@"   at Your.Test.Project.TestClass.TestMethod() in c:\path\to\your\test\project\TestClass.cs:line 30")
                    .ToString()
                    .TrimEnd();

            convention
                .HideExceptionDetails
                .For(typeof(SampleAssertionLibrary.SampleAssert));

            AssertionLibraryFilter()
                .FilterStackTrace(new FakeException(originalStackTrace))
                .ShouldEqual(filteredStackTrace);
        }

        public void ShouldGetExceptionTypeAsDisplayNameByDefault()
        {
            AssertionLibraryFilter()
                .DisplayName(new FakeException(null))
                .ShouldEqual(typeof(FakeException).FullName);
        }

        public void ShouldGetBlankDisplayNameWhenExceptionTypeIsAnAssertionLibraryImplementationDetail()
        {
            convention
                .HideExceptionDetails
                .For<FakeException>();

            AssertionLibraryFilter()
                .DisplayName(new FakeException(null))
                .ShouldEqual("");
        }

        AssertionLibraryFilter AssertionLibraryFilter()
        {
            return new AssertionLibraryFilter(convention);
        }

        class FakeException : Exception
        {
            public FakeException(string stackTrace)
            {
                StackTrace = stackTrace;
            }

            public override string StackTrace { get; }
        }
    }
}