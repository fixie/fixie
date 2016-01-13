using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Fixie.Execution;
using Fixie.Internal;
using Should;

namespace Fixie.Tests.Execution
{
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

            compoundException.PrimaryException.DisplayName.ShouldEqual("Fixie.Tests.Execution.CompoundExceptionTests+PrimaryException");
            compoundException.PrimaryException.Type.ShouldEqual("Fixie.Tests.Execution.CompoundExceptionTests+PrimaryException");
            compoundException.PrimaryException.Message.ShouldEqual("Primary Exception!");
            compoundException.PrimaryException.StackTrace.ShouldEqual(exception.StackTrace);

            compoundException.PrimaryException.InnerException.DisplayName.ShouldEqual("System.DivideByZeroException");
            compoundException.PrimaryException.InnerException.Type.ShouldEqual("System.DivideByZeroException");
            compoundException.PrimaryException.InnerException.Message.ShouldEqual("Divide by Zero Exception!");
            compoundException.PrimaryException.InnerException.StackTrace.ShouldEqual(exception.InnerException.StackTrace);

            compoundException.PrimaryException.InnerException.InnerException.ShouldBeNull();

            compoundException.SecondaryExceptions.Count.ShouldEqual(0);

            compoundException.CompoundStackTrace
               .Split(new[] { Environment.NewLine }, StringSplitOptions.None)
               .Select(x => Regex.Replace(x, @":line \d+", ":line #")) //Avoid brittle assertion introduced by stack trace line numbers.
               .ShouldEqual(
                   "Primary Exception!",
                   "   at Fixie.Tests.Execution.CompoundExceptionTests.GetPrimaryException() in " + PathToThisFile() + ":line #",
                   "",
                   "------- Inner Exception: System.DivideByZeroException -------",
                   "Divide by Zero Exception!",
                   "   at Fixie.Tests.Execution.CompoundExceptionTests.GetPrimaryException() in " + PathToThisFile() + ":line #");
        }

        public void ShouldSummarizeCollectionsOfExceptionsComprisedOfPrimaryAndSecondaryExceptions()
        {
            var assertionLibrary = AssertionLibraryFilter();
            var primaryException = GetPrimaryException();
            var secondaryExceptionA = new NotImplementedException();
            var secondaryExceptionB = GetSecondaryException();

            var compoundException = new CompoundException(new[] { primaryException, secondaryExceptionA, secondaryExceptionB }, assertionLibrary);

            compoundException.PrimaryException.DisplayName.ShouldEqual("Fixie.Tests.Execution.CompoundExceptionTests+PrimaryException");
            compoundException.PrimaryException.Type.ShouldEqual("Fixie.Tests.Execution.CompoundExceptionTests+PrimaryException");
            compoundException.PrimaryException.Message.ShouldEqual("Primary Exception!");
            compoundException.PrimaryException.StackTrace.ShouldEqual(primaryException.StackTrace);
            compoundException.PrimaryException.InnerException.DisplayName.ShouldEqual("System.DivideByZeroException");
            compoundException.PrimaryException.InnerException.Type.ShouldEqual("System.DivideByZeroException");
            compoundException.PrimaryException.InnerException.Message.ShouldEqual("Divide by Zero Exception!");
            compoundException.PrimaryException.InnerException.StackTrace.ShouldEqual(primaryException.InnerException.StackTrace);
            compoundException.PrimaryException.InnerException.InnerException.ShouldBeNull();

            compoundException.SecondaryExceptions.Count.ShouldEqual(2);

            compoundException.SecondaryExceptions[0].DisplayName.ShouldEqual("System.NotImplementedException");
            compoundException.SecondaryExceptions[0].Type.ShouldEqual("System.NotImplementedException");
            compoundException.SecondaryExceptions[0].Message.ShouldEqual("The method or operation is not implemented.");
            compoundException.SecondaryExceptions[0].StackTrace.ShouldBeNull();
            compoundException.SecondaryExceptions[0].InnerException.ShouldBeNull();

            compoundException.SecondaryExceptions[1].DisplayName.ShouldEqual("Fixie.Tests.Execution.CompoundExceptionTests+SecondaryException");
            compoundException.SecondaryExceptions[1].Type.ShouldEqual("Fixie.Tests.Execution.CompoundExceptionTests+SecondaryException");
            compoundException.SecondaryExceptions[1].Message.ShouldEqual("Secondary Exception!");
            compoundException.SecondaryExceptions[1].StackTrace.ShouldEqual(secondaryExceptionB.StackTrace);
            compoundException.SecondaryExceptions[1].InnerException.DisplayName.ShouldEqual("System.ApplicationException");
            compoundException.SecondaryExceptions[1].InnerException.Type.ShouldEqual("System.ApplicationException");
            compoundException.SecondaryExceptions[1].InnerException.Message.ShouldEqual("Application Exception!");
            compoundException.SecondaryExceptions[1].InnerException.StackTrace.ShouldEqual(secondaryExceptionB.InnerException.StackTrace);
            compoundException.SecondaryExceptions[1].InnerException.InnerException.DisplayName.ShouldEqual("System.NotImplementedException");
            compoundException.SecondaryExceptions[1].InnerException.InnerException.Type.ShouldEqual("System.NotImplementedException");
            compoundException.SecondaryExceptions[1].InnerException.InnerException.Message.ShouldEqual("Not Implemented Exception!");
            compoundException.SecondaryExceptions[1].InnerException.InnerException.StackTrace.ShouldEqual(secondaryExceptionB.InnerException.InnerException.StackTrace);
            compoundException.SecondaryExceptions[1].InnerException.InnerException.InnerException.ShouldBeNull();

            compoundException.CompoundStackTrace
                .Split(new[] { Environment.NewLine }, StringSplitOptions.None)
                .Select(x => Regex.Replace(x, @":line \d+", ":line #")) //Avoid brittle assertion introduced by stack trace line numbers.
                .ShouldEqual(
                    "Primary Exception!",
                    "   at Fixie.Tests.Execution.CompoundExceptionTests.GetPrimaryException() in " + PathToThisFile() + ":line #",
                    "",
                    "------- Inner Exception: System.DivideByZeroException -------",
                    "Divide by Zero Exception!",
                    "   at Fixie.Tests.Execution.CompoundExceptionTests.GetPrimaryException() in " + PathToThisFile() + ":line #",
                    "",
                    "===== Secondary Exception: System.NotImplementedException =====",
                    "The method or operation is not implemented.",
                    "",
                    "",
                    "===== Secondary Exception: Fixie.Tests.Execution.CompoundExceptionTests+SecondaryException =====",
                    "Secondary Exception!",
                    "   at Fixie.Tests.Execution.CompoundExceptionTests.GetSecondaryException() in " + PathToThisFile() + ":line #",
                    "",
                    "------- Inner Exception: System.ApplicationException -------",
                    "Application Exception!",
                    "   at Fixie.Tests.Execution.CompoundExceptionTests.GetSecondaryException() in " + PathToThisFile() + ":line #",
                    "",
                    "------- Inner Exception: System.NotImplementedException -------",
                    "Not Implemented Exception!",
                    "   at Fixie.Tests.Execution.CompoundExceptionTests.GetSecondaryException() in " + PathToThisFile() + ":line #");
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

            compoundException.PrimaryException.DisplayName.ShouldEqual("");
            compoundException.PrimaryException.Type.ShouldEqual("Fixie.Tests.Execution.CompoundExceptionTests+PrimaryException");
            compoundException.PrimaryException.Message.ShouldEqual("Primary Exception!");
            compoundException.PrimaryException.StackTrace.ShouldEqual("");
            compoundException.PrimaryException.InnerException.DisplayName.ShouldEqual("System.DivideByZeroException");
            compoundException.PrimaryException.InnerException.Type.ShouldEqual("System.DivideByZeroException");
            compoundException.PrimaryException.InnerException.Message.ShouldEqual("Divide by Zero Exception!");
            compoundException.PrimaryException.InnerException.StackTrace.ShouldEqual("");
            compoundException.PrimaryException.InnerException.InnerException.ShouldBeNull();

            compoundException.SecondaryExceptions.Count.ShouldEqual(2);

            compoundException.SecondaryExceptions[0].DisplayName.ShouldEqual("System.NotImplementedException");
            compoundException.SecondaryExceptions[0].Type.ShouldEqual("System.NotImplementedException");
            compoundException.SecondaryExceptions[0].Message.ShouldEqual("The method or operation is not implemented.");
            compoundException.SecondaryExceptions[0].StackTrace.ShouldBeNull();
            compoundException.SecondaryExceptions[0].InnerException.ShouldBeNull();

            compoundException.SecondaryExceptions[1].DisplayName.ShouldEqual("");
            compoundException.SecondaryExceptions[1].Type.ShouldEqual("Fixie.Tests.Execution.CompoundExceptionTests+SecondaryException");
            compoundException.SecondaryExceptions[1].Message.ShouldEqual("Secondary Exception!");
            compoundException.SecondaryExceptions[1].StackTrace.ShouldEqual("");
            compoundException.SecondaryExceptions[1].InnerException.DisplayName.ShouldEqual("System.ApplicationException");
            compoundException.SecondaryExceptions[1].InnerException.Type.ShouldEqual("System.ApplicationException");
            compoundException.SecondaryExceptions[1].InnerException.Message.ShouldEqual("Application Exception!");
            compoundException.SecondaryExceptions[1].InnerException.StackTrace.ShouldEqual("");
            compoundException.SecondaryExceptions[1].InnerException.InnerException.DisplayName.ShouldEqual("System.NotImplementedException");
            compoundException.SecondaryExceptions[1].InnerException.InnerException.Type.ShouldEqual("System.NotImplementedException");
            compoundException.SecondaryExceptions[1].InnerException.InnerException.Message.ShouldEqual("Not Implemented Exception!");
            compoundException.SecondaryExceptions[1].InnerException.InnerException.StackTrace.ShouldEqual("");
            compoundException.SecondaryExceptions[1].InnerException.InnerException.InnerException.ShouldBeNull();

            compoundException.CompoundStackTrace
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
                    "===== Secondary Exception: Fixie.Tests.Execution.CompoundExceptionTests+SecondaryException =====",
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

        static string PathToThisFile([CallerFilePath] string path = null)
        {
            return path;
        }
    }
}
