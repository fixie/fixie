using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Fixie.Execution;

namespace Fixie.Results
{
    [Serializable]
    public class CompoundException
    {
        public ExceptionInfo PrimaryException { get; private set; }
        public IReadOnlyList<ExceptionInfo> SecondaryExceptions { get; private set; }
        public string CompoundStackTrace { get; private set; }

        public CompoundException(IEnumerable<Exception> exceptions, AssertionLibraryFilter filter)
        {
            var all = exceptions.Select(x => new ExceptionInfo(x, filter)).ToArray();
            PrimaryException = all.First();
            SecondaryExceptions = all.Skip(1).ToArray();
            CompoundStackTrace = GetCompoundStackTrace(all);
        }

        static string GetCompoundStackTrace(IEnumerable<ExceptionInfo> exceptions)
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