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

            compoundException.DisplayName.ShouldEqual("Fixie.Tests.Execution.CompoundExceptionTests+PrimaryException");
            compoundException.Type.ShouldEqual("Fixie.Tests.Execution.CompoundExceptionTests+PrimaryException");
            compoundException.Message.ShouldEqual("Primary Exception!");

            compoundException.StackTrace
               .Split(new[] { Environment.NewLine }, StringSplitOptions.None)
               .Select(x => Regex.Replace(x, @":line \d+", ":line #")) //Avoid brittle assertion introduced by stack trace line numbers.
               .ShouldEqual(
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

            compoundException.DisplayName.ShouldEqual("Fixie.Tests.Execution.CompoundExceptionTests+PrimaryException");
            compoundException.Type.ShouldEqual("Fixie.Tests.Execution.CompoundExceptionTests+PrimaryException");
            compoundException.Message.ShouldEqual("Primary Exception!");

            compoundException.StackTrace
                .Split(new[] { Environment.NewLine }, StringSplitOptions.None)
                .Select(x => Regex.Replace(x, @":line \d+", ":line #")) //Avoid brittle assertion introduced by stack trace line numbers.
                .ShouldEqual(
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

            compoundException.DisplayName.ShouldEqual("");
            compoundException.Type.ShouldEqual("Fixie.Tests.Execution.CompoundExceptionTests+PrimaryException");
            compoundException.Message.ShouldEqual("Primary Exception!");

            compoundException.StackTrace
                .Split(new[] { Environment.NewLine }, StringSplitOptions.None)
                .Select(x => Regex.Replace(x, @":line \d+", ":line #")) //Avoid brittle assertion introduced by stack trace line numbers.
                .ShouldEqual(
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
