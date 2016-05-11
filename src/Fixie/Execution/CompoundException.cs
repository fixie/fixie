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
        public CompoundException(IReadOnlyCollection<Exception> exceptions, AssertionLibraryFilter filter)
        {
            var primary = exceptions.First();
            var all = exceptions.Select(x => new ExceptionInfo(x)).ToArray();
            PrimaryException = all.First();
            SecondaryExceptions = all.Skip(1).ToArray();
            CompoundStackTrace = GetCompoundStackTrace(exceptions, filter);

            DisplayName = filter.DisplayName(primary);
        }

        [Obsolete]
        public ExceptionInfo PrimaryException { get; }
        public string DisplayName { get; }
        public string Type => PrimaryException.Type;
        public string Message => PrimaryException.Message;

        [Obsolete]
        public IReadOnlyList<ExceptionInfo> SecondaryExceptions { get; }

        public string CompoundStackTrace { get; }

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