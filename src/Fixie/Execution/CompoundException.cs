using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Fixie.Internal;

namespace Fixie.Execution
{
    [Serializable]
    public class CompoundException
    {
        public CompoundException(IReadOnlyList<Exception> exceptions, AssertionLibraryFilter filter)
        {
            var primary = exceptions.First();
            Type = primary.GetType().FullName;
            Message = primary.Message;
            IsAssertionException = filter.IsAssertionException(primary);

            var all = exceptions.Select(x => new ExceptionInfo(x, filter)).ToArray();
            CompoundStackTrace = GetCompoundStackTrace(all);
        }

        public string Type { get; }
        public string Message { get; }
        public bool IsAssertionException { get; }
        public string CompoundStackTrace { get; }

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
                        console.WriteLine($"===== Secondary Exception: {ex.Type} =====");
                        console.WriteLine(ex.Message);
                        console.Write(ex.StackTrace);
                    }

                    var walk = ex;
                    while (walk.InnerException != null)
                    {
                        walk = walk.InnerException;
                        console.WriteLine();
                        console.WriteLine();
                        console.WriteLine($"------- Inner Exception: {walk.Type} -------");
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