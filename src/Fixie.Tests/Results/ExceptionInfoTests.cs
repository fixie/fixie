using System;
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
    }
}
