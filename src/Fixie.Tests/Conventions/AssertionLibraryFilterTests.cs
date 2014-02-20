using System;
using System.Text;
using Fixie.Conventions;
using Should;

namespace Fixie.Tests.Conventions
{
    public class AssertionLibraryFilterTests
    {
        public void ShouldDoNothingWhenFilteringNullStackTrace()
        {
            const string nullStackTrace = null;

            new AssertionLibraryFilter()
                .Namespace("Some.Assertion.Library.Namespace")
                .Namespace("Some.Other.Assertion.Library.Namespace")
                .FilterStackTrace(new FakeException(nullStackTrace))
                .ShouldEqual(nullStackTrace);
        }

        public void ShouldFilterAssertionLibraryImplementationDetailsFromStackTraces()
        {
            var originalStackTrace =
                new StringBuilder()
                    .AppendLine(@"   at Some.Assertion.Library.Namespace.Assert.AreEqual(object x, object y) in c:\path\to\assertion\library\AssertHelpers.cs:line 10")
                    .AppendLine(@"   at Some.Other.Assertion.Library.Namespace.Assert.AreEqual(Int32 x, Int32 y) in c:\path\to\assertion\library\Assert.cs:line 14")
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

            new AssertionLibraryFilter()
                .Namespace("Some.Assertion.Library.Namespace")
                .Namespace("Some.Other.Assertion.Library.Namespace")
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
            new AssertionLibraryFilter()
                .Namespace(typeof(FakeException).Namespace)
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