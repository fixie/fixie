using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Fixie
{
    public class MethodFilter
    {
        BindingFlags bindingFlags;
        readonly List<Func<MethodInfo, bool>> conditions;

        public MethodFilter()
        {
            bindingFlags = BindingFlags.Default;
            conditions = new List<Func<MethodInfo, bool>>();

            ExcludeMethodsDefinedOnObject();
        }

        public MethodFilter Visibility(BindingFlags flags)
        {
            bindingFlags |= flags;
            return this;
        }

        public MethodFilter Where(Func<MethodInfo, bool> condition)
        {
            conditions.Add(condition);
            return this;
        }

        public MethodFilter ZeroParameters()
        {
            return Where(method => method.GetParameters().Length == 0);
        }

        public IEnumerable<MethodInfo> Filter(Type type)
        {
            return type.GetMethods(bindingFlags).Where(IsMatch).ToArray();
        }

        bool IsMatch(MethodInfo candidate)
        {
            return conditions.All(condition => condition(candidate));
        }

        void ExcludeMethodsDefinedOnObject()
        {
            Where(method => method.DeclaringType != typeof(object));
        }
    }
}