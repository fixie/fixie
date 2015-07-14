using System;

namespace Fixie.Samples.ColumnParameter
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    class ColumnAttribute : Attribute
    {
        public ColumnAttribute(params object[] parameters)
        {
            Parameters = parameters;
        }

        public object[] Parameters { get; private set; }
    }
}