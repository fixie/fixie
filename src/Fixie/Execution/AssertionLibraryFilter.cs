using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Fixie.Execution
{
    public class AssertionLibraryFilter
    {
        readonly List<Type> exceptionTypes;
        readonly List<Type> stackTraceTypes;

        public AssertionLibraryFilter(params Type[] assertionLibraryTypes)
            : this((IEnumerable<Type>)assertionLibraryTypes) { }

        public AssertionLibraryFilter(IEnumerable<Type> assertionLibraryTypes)
        {
            exceptionTypes = new List<Type>();
            stackTraceTypes = new List<Type>();

            foreach (var type in assertionLibraryTypes)
            {
                bool isExceptionType = type.IsSubclassOf(typeof(Exception));

                if (isExceptionType)
                    exceptionTypes.Add(type);
                else
                    stackTraceTypes.Add(type);
            }
        }

        public string FilterStackTrace(Exception exception)
        {
            return exception.StackTrace == null
                ? null
                : String.Join(Environment.NewLine,
                    Lines(exception.StackTrace)
                        .SkipWhile(ContainsTypeToFilter));
        }

        public string DisplayName(Exception exception)
        {
            var exceptionType = exception.GetType();

            return exceptionTypes.Contains(exceptionType) ? "" : exceptionType.FullName;
        }

        bool ContainsTypeToFilter(string line)
        {
            return stackTraceTypes.Any(type => line.Contains(type.FullName));
        }

        static IEnumerable<string> Lines(string stackTrace)
        {
            using (var reader = new StringReader(stackTrace))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                    yield return line;
            }
        }
    }
}