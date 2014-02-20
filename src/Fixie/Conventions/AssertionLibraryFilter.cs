using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Fixie.Conventions
{
    public class AssertionLibraryFilter
    {
        readonly List<string> namespaces;

        public AssertionLibraryFilter()
        {
            namespaces = new List<string>();
        }

        public AssertionLibraryFilter Namespace(string @namespace)
        {
            namespaces.Add(@namespace);
            return this;
        }

        public string FilterStackTrace(Exception exception)
        {
            return exception.StackTrace == null
                ? null
                : String.Join(Environment.NewLine,
                    Lines(exception.StackTrace)
                        .SkipWhile(ContainsNamespaceToFilter));
        }

        public string DisplayName(Exception exception)
        {
            var exceptionType = exception.GetType();

            return namespaces.Contains(exceptionType.Namespace) ? "" : exceptionType.FullName;
        }

        bool ContainsNamespaceToFilter(string s)
        {
            return namespaces.Any(s.Contains);
        }

        private static IEnumerable<string> Lines(string stackTrace)
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