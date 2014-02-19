using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Fixie.Results
{
    [Serializable]
    public class ExceptionInfo
    {
        public ExceptionInfo(IReadOnlyList<ExceptionInfo> exceptions)
        {
            Type = exceptions.First().Type;
            Message = exceptions.First().Message;
            StackTrace = CompoundStackTrace(exceptions);
            InnerException = null;
        }

        public ExceptionInfo(Exception exception)
        {
            Type = exception.GetType().FullName;
            Message = exception.Message;
            StackTrace = exception.StackTrace;
            InnerException = exception.InnerException == null ? null : new ExceptionInfo(exception.InnerException);
        }

        public string Type { get; private set; }
        public string Message { get; private set; }
        public string StackTrace { get; private set; }
        public ExceptionInfo InnerException { get; private set; }

        static string CompoundStackTrace(IEnumerable<ExceptionInfo> exceptions)
        {
            using (var console = new StringWriter())
            {
                bool isPrimaryException = true;

                foreach (var ex in exceptions)
                {
                    if (isPrimaryException)
                    {
                        console.WriteLine(ex.Message);
                        console.Write(ex.StackTrace);
                    }
                    else
                    {
                        console.WriteLine();
                        console.WriteLine();
                        using (Foreground.DarkGray)
                            console.WriteLine("===== Secondary Exception: {0} =====", ex.Type);
                        console.WriteLine(ex.Message);
                        console.Write(ex.StackTrace);
                    }

                    var walk = ex;
                    while (walk.InnerException != null)
                    {
                        walk = walk.InnerException;
                        console.WriteLine();
                        console.WriteLine();
                        using (Foreground.DarkGray)
                            console.WriteLine("------- Inner Exception: {0} -------", walk.Type);
                        console.WriteLine(walk.Message);
                        console.Write(walk.StackTrace);
                    }

                    isPrimaryException = false;
                }
                return console.ToString();
            }
        }
    }
}