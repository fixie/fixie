using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Fixie.Conventions
{
    public class AssertionLibraryFilter
    {
        readonly List<Type> exceptionTypes;
        readonly List<Type> stackTraceTypes;

        public AssertionLibraryFilter()
        {
            exceptionTypes = new List<Type>();
            stackTraceTypes = new List<Type>();
        }

        public AssertionLibraryFilter For(Type libraryInfrastructureType)
        {
            bool isExceptionType = libraryInfrastructureType.IsSubclassOf(typeof(Exception));

            (isExceptionType ? exceptionTypes : stackTraceTypes).Add(libraryInfrastructureType);

            return this;
        }

        public AssertionLibraryFilter For<TLibraryInfrastructure>()
        {
            return For(typeof(TLibraryInfrastructure));
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