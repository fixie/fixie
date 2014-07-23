using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Fixie.Execution;
using Fixie.Results;
using Should;

namespace Fixie.Tests.Results
{
    public class CompoundExceptionTests
    {
        public void ShouldSummarizeTheGivenException()
        {
            var assertionLibrary = new AssertionLibraryFilter();
            var exception = GetPrimaryException();

            var exceptionInfo = new ExceptionInfo(exception, assertionLibrary);

            exceptionInfo.DisplayName.ShouldEqual("Fixie.Tests.Results.CompoundExceptionTests+PrimaryException");
            exceptionInfo.Type.ShouldEqual("Fixie.Tests.Results.CompoundExceptionTests+PrimaryException");
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

            var exceptionInfo = new CompoundException(new[] { primaryException, secondaryExceptionA, secondaryExceptionB }, assertionLibrary);

            exceptionInfo.PrimaryException.DisplayName.ShouldEqual("Fixie.Tests.Results.CompoundExceptionTests+PrimaryException");
            exceptionInfo.PrimaryException.Type.ShouldEqual("Fixie.Tests.Results.CompoundExceptionTests+PrimaryException");
            exceptionInfo.PrimaryException.Message.ShouldEqual("Primary Exception!");
            exceptionInfo.PrimaryException.StackTrace.ShouldEqual(primaryException.StackTrace);
            exceptionInfo.PrimaryException.InnerException.DisplayName.ShouldEqual("System.DivideByZeroException");
            exceptionInfo.PrimaryException.InnerException.Type.ShouldEqual("System.DivideByZeroException");
            exceptionInfo.PrimaryException.InnerException.Message.ShouldEqual("Divide by Zero Exception!");
            exceptionInfo.PrimaryException.InnerException.StackTrace.ShouldEqual(primaryException.InnerException.StackTrace);
            exceptionInfo.PrimaryException.InnerException.InnerException.ShouldBeNull();

            exceptionInfo.SecondaryExceptions.Count.ShouldEqual(2);

            exceptionInfo.SecondaryExceptions[0].DisplayName.ShouldEqual("System.NotImplementedException");
            exceptionInfo.SecondaryExceptions[0].Type.ShouldEqual("System.NotImplementedException");
            exceptionInfo.SecondaryExceptions[0].Message.ShouldEqual("The method or operation is not implemented.");
            exceptionInfo.SecondaryExceptions[0].StackTrace.ShouldBeNull();
            exceptionInfo.SecondaryExceptions[0].InnerException.ShouldBeNull();

            exceptionInfo.SecondaryExceptions[1].DisplayName.ShouldEqual("Fixie.Tests.Results.CompoundExceptionTests+SecondaryException");
            exceptionInfo.SecondaryExceptions[1].Type.ShouldEqual("Fixie.Tests.Results.CompoundExceptionTests+SecondaryException");
            exceptionInfo.SecondaryExceptions[1].Message.ShouldEqual("Secondary Exception!");
            exceptionInfo.SecondaryExceptions[1].StackTrace.ShouldEqual(secondaryExceptionB.StackTrace);
            exceptionInfo.SecondaryExceptions[1].InnerException.DisplayName.ShouldEqual("System.ApplicationException");
            exceptionInfo.SecondaryExceptions[1].InnerException.Type.ShouldEqual("System.ApplicationException");
            exceptionInfo.SecondaryExceptions[1].InnerException.Message.ShouldEqual("Application Exception!");
            exceptionInfo.SecondaryExceptions[1].InnerException.StackTrace.ShouldEqual(secondaryExceptionB.InnerException.StackTrace);
            exceptionInfo.SecondaryExceptions[1].InnerException.InnerException.DisplayName.ShouldEqual("System.NotImplementedException");
            exceptionInfo.SecondaryExceptions[1].InnerException.InnerException.Type.ShouldEqual("System.NotImplementedException");
            exceptionInfo.SecondaryExceptions[1].InnerException.InnerException.Message.ShouldEqual("Not Implemented Exception!");
            exceptionInfo.SecondaryExceptions[1].InnerException.InnerException.StackTrace.ShouldEqual(secondaryExceptionB.InnerException.InnerException.StackTrace);
            exceptionInfo.SecondaryExceptions[1].InnerException.InnerException.InnerException.ShouldBeNull();

            exceptionInfo.CompoundStackTrace
                .Split(new[] { Environment.NewLine }, StringSplitOptions.None)
                .Select(x => Regex.Replace(x, @":line \d+", ":line #")) //Avoid brittle assertion introduced by stack trace line numbers.
                .ShouldEqual(
                    "Primary Exception!",
                    "   at Fixie.Tests.Results.CompoundExceptionTests.GetPrimaryException() in " + PathToThisFile() + ":line #",
                    "",
                    "------- Inner Exception: System.DivideByZeroException -------",
                    "Divide by Zero Exception!",
                    "   at Fixie.Tests.Results.CompoundExceptionTests.GetPrimaryException() in " + PathToThisFile() + ":line #",
                    "",
                    "===== Secondary Exception: System.NotImplementedException =====",
                    "The method or operation is not implemented.",
                    "",
                    "",
                    "===== Secondary Exception: Fixie.Tests.Results.CompoundExceptionTests+SecondaryException =====",
                    "Secondary Exception!",
                    "   at Fixie.Tests.Results.CompoundExceptionTests.GetSecondaryException() in " + PathToThisFile() + ":line #",
                    "",
                    "------- Inner Exception: System.ApplicationException -------",
                    "Application Exception!",
                    "   at Fixie.Tests.Results.CompoundExceptionTests.GetSecondaryException() in " + PathToThisFile() + ":line #",
                    "",
                    "------- Inner Exception: System.NotImplementedException -------",
                    "Not Implemented Exception!",
                    "   at Fixie.Tests.Results.CompoundExceptionTests.GetSecondaryException() in " + PathToThisFile() + ":line #");
        }

        public void ShouldFilterAssertionLibraryImplementationDetails()
        {
            var assertionLibrary = new AssertionLibraryFilter(typeof(PrimaryException), typeof(SecondaryException), typeof(CompoundExceptionTests));
            var primaryException = GetPrimaryException();
            var secondaryExceptionA = new NotImplementedException();
            var secondaryExceptionB = GetSecondaryException();

            var exceptionInfo = new CompoundException(new[] { primaryException, secondaryExceptionA, secondaryExceptionB }, assertionLibrary);

            exceptionInfo.PrimaryException.DisplayName.ShouldEqual("");
            exceptionInfo.PrimaryException.Type.ShouldEqual("Fixie.Tests.Results.CompoundExceptionTests+PrimaryException");
            exceptionInfo.PrimaryException.Message.ShouldEqual("Primary Exception!");
            exceptionInfo.PrimaryException.StackTrace.ShouldEqual("");
            exceptionInfo.PrimaryException.InnerException.DisplayName.ShouldEqual("System.DivideByZeroException");
            exceptionInfo.PrimaryException.InnerException.Type.ShouldEqual("System.DivideByZeroException");
            exceptionInfo.PrimaryException.InnerException.Message.ShouldEqual("Divide by Zero Exception!");
            exceptionInfo.PrimaryException.InnerException.StackTrace.ShouldEqual("");
            exceptionInfo.PrimaryException.InnerException.InnerException.ShouldBeNull();

            exceptionInfo.SecondaryExceptions.Count.ShouldEqual(2);

            exceptionInfo.SecondaryExceptions[0].DisplayName.ShouldEqual("System.NotImplementedException");
            exceptionInfo.SecondaryExceptions[0].Type.ShouldEqual("System.NotImplementedException");
            exceptionInfo.SecondaryExceptions[0].Message.ShouldEqual("The method or operation is not implemented.");
            exceptionInfo.SecondaryExceptions[0].StackTrace.ShouldBeNull();
            exceptionInfo.SecondaryExceptions[0].InnerException.ShouldBeNull();

            exceptionInfo.SecondaryExceptions[1].DisplayName.ShouldEqual("");
            exceptionInfo.SecondaryExceptions[1].Type.ShouldEqual("Fixie.Tests.Results.CompoundExceptionTests+SecondaryException");
            exceptionInfo.SecondaryExceptions[1].Message.ShouldEqual("Secondary Exception!");
            exceptionInfo.SecondaryExceptions[1].StackTrace.ShouldEqual("");
            exceptionInfo.SecondaryExceptions[1].InnerException.DisplayName.ShouldEqual("System.ApplicationException");
            exceptionInfo.SecondaryExceptions[1].InnerException.Type.ShouldEqual("System.ApplicationException");
            exceptionInfo.SecondaryExceptions[1].InnerException.Message.ShouldEqual("Application Exception!");
            exceptionInfo.SecondaryExceptions[1].InnerException.StackTrace.ShouldEqual("");
            exceptionInfo.SecondaryExceptions[1].InnerException.InnerException.DisplayName.ShouldEqual("System.NotImplementedException");
            exceptionInfo.SecondaryExceptions[1].InnerException.InnerException.Type.ShouldEqual("System.NotImplementedException");
            exceptionInfo.SecondaryExceptions[1].InnerException.InnerException.Message.ShouldEqual("Not Implemented Exception!");
            exceptionInfo.SecondaryExceptions[1].InnerException.InnerException.StackTrace.ShouldEqual("");
            exceptionInfo.SecondaryExceptions[1].InnerException.InnerException.InnerException.ShouldBeNull();

            exceptionInfo.CompoundStackTrace
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
                    "===== Secondary Exception: Fixie.Tests.Results.CompoundExceptionTests+SecondaryException =====",
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
