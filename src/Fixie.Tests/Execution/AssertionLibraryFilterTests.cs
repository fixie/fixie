using System;
using System.Text;
using Fixie.Execution;
using Should;

namespace Fixie.Tests.Execution
{
    public class AssertionLibraryFilterTests
    {
        public void ShouldDoNothingWhenFilteringNullStackTrace()
        {
            const string nullStackTrace = null;

            new AssertionLibraryFilter(typeof(SampleAssertionLibrary.SampleAssert))
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

            new AssertionLibraryFilter(typeof(SampleAssertionLibrary.SampleAssert))
                .FilterStackTrace(new FakeException(originalStackTrace))
                .ShouldEqual(filteredStackTrace);
        }

        public void ShouldGetExceptionTypeAsDisplayNameByDefault()
        {
            new AssertionLibraryFilter()
                .DisplayName(new FakeException(null))
                .ShouldEqual(typeof(FakeException).FullName);
        }

        public void ShouldGetBlankDisplayNameWhenExceptionTypeIsAnAssertionLibraryImplementationDetail()
        {
            new AssertionLibraryFilter(typeof(FakeException))
                .DisplayName(new FakeException(null))
                .ShouldEqual("");
        }

        class FakeException : Exception
        {
            readonly string stackTrace;

            public FakeException(string stackTrace)
            {
                this.stackTrace = stackTrace;
            }

            public override string StackTrace
            {
                get { return stackTrace; }
            }
        }
    }
}