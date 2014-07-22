using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Fixie.Execution;
using Fixie.Results;
using Should;

namespace Fixie.Tests.Results
{
    public class ExceptionInfoTests
    {
        public void ShouldSummarizeTheGivenException()
        {
            var assertionLibrary = new AssertionLibraryFilter();
            var exception = GetPrimaryException();

            var exceptionInfo = new ExceptionInfo(exception, assertionLibrary);

            exceptionInfo.DisplayName.ShouldEqual("Fixie.Tests.Results.ExceptionInfoTests+PrimaryException");
            exceptionInfo.Type.ShouldEqual("Fixie.Tests.Results.ExceptionInfoTests+PrimaryException");
            exceptionInfo.Message.ShouldEqual("Primary Exception!");
            exceptionInfo.StackTrace.ShouldEqual(exception.StackTrace);

            exceptionInfo.InnerException.DisplayName.ShouldEqual("System.DivideByZeroException");
            exceptionInfo.InnerException.Type.ShouldEqual("System.DivideByZeroException");
            exceptionInfo.InnerException.Message.ShouldEqual("Divide by Zero Exception!");
            exceptionInfo.InnerException.StackTrace.ShouldEqual(exception.InnerException.StackTrace);

            exceptionInfo.InnerException.InnerException.ShouldBeNull();
        }

        public void ShouldSummarizeCollectionsOfExceptionsComprisedOfPrimaryAndSecondaryExceptions()
        {
            var assertionLibrary = new AssertionLibraryFilter();
            var primaryException = GetPrimaryException();
            var secondaryExceptionA = new NotImplementedException();
            var secondaryExceptionB = GetSecondaryException();

            var exceptionInfo = new ExceptionInfo(new[] { primaryException, secondaryExceptionA, secondaryExceptionB }, assertionLibrary);

            exceptionInfo.DisplayName.ShouldEqual("Fixie.Tests.Results.ExceptionInfoTests+PrimaryException");
            exceptionInfo.Type.ShouldEqual("Fixie.Tests.Results.ExceptionInfoTests+PrimaryException");
            exceptionInfo.Message.ShouldEqual("Primary Exception!");
            exceptionInfo.StackTrace
                .Split(new[] { Environment.NewLine }, StringSplitOptions.None)
                .Select(x => Regex.Replace(x, @":line \d+", ":line #")) //Avoid brittle assertion introduced by stack trace line numbers.
                .ShouldEqual(
                    "Primary Exception!",
                    "   at Fixie.Tests.Results.ExceptionInfoTests.GetPrimaryException() in " + PathToThisFile() + ":line #",
                    "",
                    "------- Inner Exception: System.DivideByZeroException -------",
                    "Divide by Zero Exception!",
                    "   at Fixie.Tests.Results.ExceptionInfoTests.GetPrimaryException() in " + PathToThisFile() + ":line #",
                    "",
                    "===== Secondary Exception: System.NotImplementedException =====",
                    "The method or operation is not implemented.",
                    "",
                    "",
                    "===== Secondary Exception: Fixie.Tests.Results.ExceptionInfoTests+SecondaryException =====",
                    "Secondary Exception!",
                    "   at Fixie.Tests.Results.ExceptionInfoTests.GetSecondaryException() in " + PathToThisFile() + ":line #",
                    "",
                    "------- Inner Exception: System.ApplicationException -------",
                    "Application Exception!",
                    "   at Fixie.Tests.Results.ExceptionInfoTests.GetSecondaryException() in " + PathToThisFile() + ":line #",
                    "",
                    "------- Inner Exception: System.NotImplementedException -------",
                    "Not Implemented Exception!",
                    "   at Fixie.Tests.Results.ExceptionInfoTests.GetSecondaryException() in " + PathToThisFile() + ":line #");

            exceptionInfo.InnerException.ShouldBeNull();
        }

        public void ShouldFilterAssertionLibraryImplementationDetails()
        {
            var assertionLibrary = new AssertionLibraryFilter(typeof(PrimaryException), typeof(SecondaryException), typeof(ExceptionInfoTests));
            var primaryException = GetPrimaryException();
            var secondaryExceptionA = new NotImplementedException();
            var secondaryExceptionB = GetSecondaryException();

            var exceptionInfo = new ExceptionInfo(new[] { primaryException, secondaryExceptionA, secondaryExceptionB }, assertionLibrary);

            exceptionInfo.DisplayName.ShouldEqual("");
            exceptionInfo.Type.ShouldEqual("Fixie.Tests.Results.ExceptionInfoTests+PrimaryException");
            exceptionInfo.Message.ShouldEqual("Primary Exception!");
            exceptionInfo.StackTrace
                .Split(new[] { Environment.NewLine }, StringSplitOptions.None)
                .Select(x => Regex.Replace(x, @":line \d+", ":line #")) //Avoid brittle assertion introduced by stack trace line numbers.
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
                    "===== Secondary Exception: Fixie.Tests.Results.ExceptionInfoTests+SecondaryException =====",
                    "Secondary Exception!",
                    "",
                    "",
                    "------- Inner Exception: System.ApplicationException -------",
                    "Application Exception!",
                    "",
                    "",
                    "------- Inner Exception: System.NotImplementedException -------",
                    "Not Implemented Exception!",
                    "");

            exceptionInfo.InnerException.ShouldBeNull();
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

        static string PathToThisFile([CallerFilePath] string path = null)
        {
            return path;
        }
    }
}
