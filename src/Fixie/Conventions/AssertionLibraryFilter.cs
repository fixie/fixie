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

        bool ContainsNamespaceToFilter(string line)
        {
            return namespaces.Any(line.Contains);
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