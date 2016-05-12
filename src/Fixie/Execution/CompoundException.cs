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
            DisplayName = filter.DisplayName(primary);
            Type = primary.GetType().FullName;
            Message = primary.Message;
            StackTrace = GetCompoundStackTrace(exceptions, filter);
        }

        public string DisplayName { get; }
        public string Type { get; }
        public string Message { get; }
        public string StackTrace { get; }

        static string GetCompoundStackTrace(IEnumerable<Exception> exceptions, AssertionLibraryFilter filter)
        {
            using (var console = new StringWriter())
            {
                bool isPrimaryException = true;

                foreach (var ex in exceptions)
                {
                    if (isPrimaryException)
                    {
                        console.Write(filter.FilterStackTrace(ex));
                    }
                    else
                    {
                        console.WriteLine();
                        console.WriteLine();
                        console.WriteLine($"===== Secondary Exception: {ex.GetType().FullName} =====");
                        console.WriteLine(ex.Message);
                        console.Write(filter.FilterStackTrace(ex));
                    }

                    var walk = ex;
                    while (walk.InnerException != null)
                    {
                        walk = walk.InnerException;
                        console.WriteLine();
                        console.WriteLine();
                        console.WriteLine($"------- Inner Exception: {walk.GetType().FullName} -------");
                        console.WriteLine(walk.Message);
                        console.Write(filter.FilterStackTrace(walk));
                    }

                    isPrimaryException = false;
                }
                return console.ToString();
            }
        }
    }
}