using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Fixie.Results;
using Should;

namespace Fixie.Tests.Results
{
    public class ExceptionInfoTests
    {
        public void ShouldSummarizeTheGivenException()
        {
            var exception = GetNestedException();

            var exceptionInfo = new ExceptionInfo(exception);

            exceptionInfo.Type.ShouldEqual("System.NullReferenceException");
            exceptionInfo.Message.ShouldEqual("Null reference!");
            exceptionInfo.StackTrace.ShouldEqual(exception.StackTrace);

            exceptionInfo.InnerException.Type.ShouldEqual("System.DivideByZeroException");
            exceptionInfo.InnerException.Message.ShouldEqual("Divide by zero!");
            exceptionInfo.InnerException.StackTrace.ShouldEqual(exception.InnerException.StackTrace);

            exceptionInfo.InnerException.InnerException.ShouldBeNull();
        }

        public void ShouldSummarizeCollectionsOfExceptionsComprisedOfPrimaryAndSecondaryExceptions()
        {
            var primaryException = GetNestedException();
            var secondaryExceptionA = new NotImplementedException();
            var secondaryExceptionB = GetSecondaryNestedException();

            var exceptionInfo = new ExceptionInfo(new[] { primaryException, secondaryExceptionA, secondaryExceptionB });

            exceptionInfo.Type.ShouldEqual("System.NullReferenceException");
            exceptionInfo.Message.ShouldEqual("Null reference!");
            exceptionInfo.StackTrace
                .Split(new[] { Environment.NewLine }, StringSplitOptions.None)
                .Select(x => Regex.Replace(x, @":line \d+", ":line #")) //Avoid brittle assertion introduced by stack trace line numbers.
                .ShouldEqual(
                    "Null reference!",
                    "   at Fixie.Tests.Results.ExceptionInfoTests.GetNestedException() in " + PathToThisFile() + ":line #",
                    "",
                    "------- Inner Exception: System.DivideByZeroException -------",
                    "Divide by zero!",
                    "   at Fixie.Tests.Results.ExceptionInfoTests.GetNestedException() in " + PathToThisFile() + ":line #",
                    "",
                    "===== Secondary Exception: System.NotImplementedException =====",
                    "The method or operation is not implemented.",
                    "",
                    "",
                    "===== Secondary Exception: System.ArgumentException =====",
                    "Argument!",
                    "   at Fixie.Tests.Results.ExceptionInfoTests.GetSecondaryNestedException() in " + PathToThisFile() + ":line #",
                    "",
                    "------- Inner Exception: System.ApplicationException -------",
                    "Application!",
                    "   at Fixie.Tests.Results.ExceptionInfoTests.GetSecondaryNestedException() in " + PathToThisFile() + ":line #",
                    "",
                    "------- Inner Exception: System.NotImplementedException -------",
                    "Not implemented!",
                    "   at Fixie.Tests.Results.ExceptionInfoTests.GetSecondaryNestedException() in " + PathToThisFile() + ":line #");

            exceptionInfo.InnerException.ShouldBeNull();
        }

        static Exception GetNestedException()
        {
            try
            {
                try
                {
                    throw new DivideByZeroException("Divide by zero!");
                }
                catch (Exception exception)
                {
                    throw new NullReferenceException("Null reference!", exception);
                }
            }
            catch (Exception exception)
            {
                return exception;
            }
        }

        static Exception GetSecondaryNestedException()
        {
            try
            {
                try
                {
                    try
                    {
                        throw new NotImplementedException("Not implemented!");
                    }
                    catch (Exception exception)
                    {
                        throw new ApplicationException("Application!", exception);
                    }
                }
                catch (Exception exception)
                {
                    throw new ArgumentException("Argument!", exception);
                }
            }
            catch (Exception exception)
            {
                return exception;
            }
        }

        static string PathToThisFile([CallerFilePath] string path = null)
        {
            return path;
        }
    }
}
