namespace Fixie.Execution
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    public class CompoundException
    {
        public CompoundException(IReadOnlyCollection<Exception> exceptions, AssertionLibraryFilter filter)
        {
            var primary = exceptions.First();
            var all = exceptions.Select(x => new ExceptionInfo(x, filter)).ToArray();
            PrimaryException = all.First();
            SecondaryExceptions = all.Skip(1).ToArray();
            CompoundStackTrace = GetCompoundStackTrace(exceptions, filter);

            DisplayName = filter.DisplayName(primary);
        }

        [Obsolete]
        public ExceptionInfo PrimaryException { get; private set; }
        public string DisplayName { get; }
        public string Type => PrimaryException.Type;
        public string Message => PrimaryException.Message;

        [Obsolete]
        public IReadOnlyList<ExceptionInfo> SecondaryExceptions { get; private set; }

        public string CompoundStackTrace { get; private set; }

        static string GetCompoundStackTrace(IEnumerable<Exception> exceptions, AssertionLibraryFilter filter)
        {
            using (var console = new StringWriter())
            {
                bool isPrimaryException = true;

                foreach (var ex in exceptions)
                {
                    if (isPrimaryException)
                    {
                        console.WriteLine(ex.Message);
                        console.Write(filter.FilterStackTrace(ex));
                    }
                    else
                    {
                        console.WriteLine();
                        console.WriteLine();
                        console.WriteLine("===== Secondary Exception: {0} =====", ex.GetType().FullName);
                        console.WriteLine(ex.Message);
                        console.Write(filter.FilterStackTrace(ex));
                    }

                    var walk = ex;
                    while (walk.InnerException != null)
                    {
                        walk = walk.InnerException;
                        console.WriteLine();
                        console.WriteLine();
                        console.WriteLine("------- Inner Exception: {0} -------", walk.GetType().FullName);
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