using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Fixie.Results;

namespace Fixie
{
    public class FailResult
    {
        public FailResult(CaseExecution execution)
        {
            Case = execution.Case;
            Output = execution.Output;
            Duration = execution.Duration;
            Exceptions = execution.Exceptions.Select(x => new ExceptionInfo(x)).ToArray();
        }

        public Case Case { get; private set; }
        public string Output { get; private set; }
        public TimeSpan Duration { get; private set; }
        public IReadOnlyList<ExceptionInfo> Exceptions { get; private set; }

        public ExceptionInfo PrimaryException
        {
            get { return Exceptions.First(); }
        }

        public string CompoundStackTrace()
        {
            using (var console = new StringWriter())
            {
                bool isPrimaryException = true;

                foreach (var ex in Exceptions)
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