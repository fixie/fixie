using System;
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
            const string originalStackTrace = @"   at Some.Assertion.Library.Namespace.Assert.AreEqual(object x, object y) in c:\path\to\assertion\library\AssertHelpers.cs:line 10
   at Some.Other.Assertion.Library.Namespace.Assert.AreEqual(Int32 x, Int32 y) in c:\path\to\assertion\library\Assert.cs:line 14
   at Your.Test.Project.TestClass.HelperMethodB() in c:\path\to\your\test\project\TestClass.cs:line 55
   at Your.Test.Project.TestClass.HelperMethodA() in c:\path\to\your\test\project\TestClass.cs:line 50
   at Your.Test.Project.TestClass.TestMethod() in c:\path\to\your\test\project\TestClass.cs:line 30";

            const string filteredStackTrace = @"   at Your.Test.Project.TestClass.HelperMethodB() in c:\path\to\your\test\project\TestClass.cs:line 55
   at Your.Test.Project.TestClass.HelperMethodA() in c:\path\to\your\test\project\TestClass.cs:line 50
   at Your.Test.Project.TestClass.TestMethod() in c:\path\to\your\test\project\TestClass.cs:line 30";

            new AssertionLibraryFilter()
                .Namespace("Some.Assertion.Library.Namespace")
                .Namespace("Some.Other.Assertion.Library.Namespace")
                .FilterStackTrace(new FakeException(originalStackTrace))
                .ShouldEqual(filteredStackTrace);
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