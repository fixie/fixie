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
            var exception = GetPrimaryException();

            var compoundException = new CompoundException(new[] { exception }, assertionLibrary);

            compoundException.Type.ShouldEqual(FullName<PrimaryException>());
            compoundException.Message.ShouldEqual("Primary Exception!");
            compoundException.FailedAssertion.ShouldEqual(false);

            compoundException.StackTrace
                .CleanStackTraceLineNumbers()
                .Lines()
                .ShouldEqual(
                    "Primary Exception!",
                    At<CompoundExceptionTests>("GetPrimaryException()"),
                    "",
                    "------- Inner Exception: System.DivideByZeroException -------",
                    "Divide by Zero Exception!",
                    At<CompoundExceptionTests>("GetPrimaryException()"));
        }

        public void ShouldSummarizeCollectionsOfExceptionsComprisedOfPrimaryAndSecondaryExceptions()
        {
            var assertionLibrary = AssertionLibraryFilter();
            var primaryException = GetPrimaryException();
            var secondaryExceptionA = new NotImplementedException();
            var secondaryExceptionB = GetSecondaryException();

            var compoundException = new CompoundException(new[] { primaryException, secondaryExceptionA, secondaryExceptionB }, assertionLibrary);

            compoundException.Type.ShouldEqual(FullName<PrimaryException>());
            compoundException.Message.ShouldEqual("Primary Exception!");
            compoundException.FailedAssertion.ShouldEqual(false);

            compoundException.StackTrace
                .CleanStackTraceLineNumbers()
                .Lines()
                .ShouldEqual(
                    "Primary Exception!",
                    At<CompoundExceptionTests>("GetPrimaryException()"),
                    "",
                    "------- Inner Exception: System.DivideByZeroException -------",
                    "Divide by Zero Exception!",
                    At<CompoundExceptionTests>("GetPrimaryException()"),
                    "",
                    "===== Secondary Exception: System.NotImplementedException =====",
                    "The method or operation is not implemented.",
                    "",
                    "",
                    "===== Secondary Exception: " + FullName<SecondaryException>() + " =====",
                    "Secondary Exception!",
                    At<CompoundExceptionTests>("GetSecondaryException()"),
                    "",
                    "------- Inner Exception: System.ApplicationException -------",
                    "Application Exception!",
                    At<CompoundExceptionTests>("GetSecondaryException()"),
                    "",
                    "------- Inner Exception: System.NotImplementedException -------",
                    "Not Implemented Exception!",
                    At<CompoundExceptionTests>("GetSecondaryException()"));
        }

        public void ShouldFilterAssertionLibraryImplementationDetails()
        {
            convention
                .HideExceptionDetails
                .For<PrimaryException>()
                .For<SecondaryException>()
                .For<CompoundExceptionTests>();

            var assertionLibrary = AssertionLibraryFilter();
            var primaryException = GetPrimaryException();
            var secondaryExceptionA = new NotImplementedException();
            var secondaryExceptionB = GetSecondaryException();

            var compoundException = new CompoundException(new[] { primaryException, secondaryExceptionA, secondaryExceptionB }, assertionLibrary);

            compoundException.Type.ShouldEqual(FullName<PrimaryException>());
            compoundException.Message.ShouldEqual("Primary Exception!");
            compoundException.FailedAssertion.ShouldEqual(true);

            compoundException.StackTrace
                .Lines()
                .ShouldEqual(
                    "Primary Exception!",
                    "",
                    "",
                    "------- Inner Exception: System.DivideByZeroException -------",
                    "Divide by Zero Exception!",
                    "",
                    "",
                    "===== Secondary Exception: System.NotImplementedException =====",
                    "The method or operation is not implemented.",
                    "",
                    "",
                    "===== Secondary Exception: " + FullName<SecondaryException>() + " =====",
                    "Secondary Exception!",
                    "",
                    "",
                    "------- Inner Exception: System.ApplicationException -------",
                    "Application Exception!",
                    "",
                    "",
                    "------- Inner Exception: System.NotImplementedException -------",
                    "Not Implemented Exception!");
        }

        AssertionLibraryFilter AssertionLibraryFilter()
        {
            return new AssertionLibraryFilter(convention);
        }

        static Exception GetPrimaryException()
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

        static Exception GetSecondaryException()
        {
            try
            {
                try
                {
                    try
                    {
                        throw new NotImplementedException("Not Implemented Exception!");
                    }
                    catch (Exception exception)
                    {
                        throw new ApplicationException("Application Exception!", exception);
                    }
                }
                catch (Exception exception)
                {
                    throw new SecondaryException(exception);
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

        class SecondaryException : Exception
        {
            public SecondaryException(Exception innerException)
                : base("Secondary Exception!", innerException) { }
        }
    }
}
