namespace Fixie.Tests.Execution
{
    using System;
    using Assertions;
    using Fixie.Execution;
    using static Utility;

    public class CompoundExceptionTests
    {
        readonly Convention convention;

        public CompoundExceptionTests()
        {
            convention = new Convention();
        }

        public void ShouldSummarizeAnyGivenException()
        {
            var assertionLibrary = AssertionLibraryFilter();
            var exception = GetException();

            var compoundException = new CompoundException(exception, assertionLibrary);
            var @case = Case("Test");
            @case.Fail(exception);
            var failure = new CaseFailed(@case, assertionLibrary);

            compoundException.Type.ShouldEqual(FullName<PrimaryException>());
            failure.Message.ShouldEqual("Primary Exception!");
            compoundException.FailedAssertion.ShouldEqual(false);

            compoundException.StackTrace
                .CleanStackTraceLineNumbers()
                .Lines()
                .ShouldEqual(
                    At<CompoundExceptionTests>("GetException()"),
                    "",
                    "------- Inner Exception: System.DivideByZeroException -------",
                    "Divide by Zero Exception!",
                    At<CompoundExceptionTests>("GetException()"));
        }

        public void ShouldFilterAssertionLibraryImplementationDetails()
        {
            convention
                .HideExceptionDetails
                .For<PrimaryException>()
                .For<CompoundExceptionTests>();

            var assertionLibrary = AssertionLibraryFilter();
            var exception = GetException();

            var compoundException = new CompoundException(exception, assertionLibrary);
            var @case = Case("Test");
            @case.Fail(exception);
            var failure = new CaseFailed(@case, assertionLibrary);

            compoundException.Type.ShouldEqual(FullName<PrimaryException>());
            failure.Message.ShouldEqual("Primary Exception!");
            compoundException.FailedAssertion.ShouldEqual(true);

            compoundException.StackTrace
                .Lines()
                .ShouldEqual(
                    "",
                    "",
                    "------- Inner Exception: System.DivideByZeroException -------",
                    "Divide by Zero Exception!");
        }

        class SampleTestClass
        {
            public void Test() { }
        }

        static Case Case(string methodName, params object[] parameters)
            => Case<SampleTestClass>(methodName, parameters);

        static Case Case<TTestClass>(string methodName, params object[] parameters)
            => new Case(typeof(TTestClass).GetInstanceMethod(methodName), parameters);

        AssertionLibraryFilter AssertionLibraryFilter()
        {
            return new AssertionLibraryFilter(convention);
        }

        static Exception GetException()
        {
            try
            {
                try
                {
                    throw new DivideByZeroException("Divide by Zero Exception!");
                }
                catch (Exception exception)
                {
                    throw new PrimaryException(exception);
                }
            }
            catch (Exception exception)
            {
                return exception;
            }
        }

        class PrimaryException : Exception
        {
            public PrimaryException(Exception innerException)
                : base("Primary Exception!", innerException) { }
        }
    }
}
