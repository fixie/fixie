using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Fixie.Conventions
{
    public class MethodFilter
    {
        readonly List<Func<MethodInfo, bool>> conditions;

        public MethodFilter()
        {
            conditions = new List<Func<MethodInfo, bool>>();

            ExcludeMethodsDefinedOnObject();
        }

        public MethodFilter Where(Func<MethodInfo, bool> condition)
        {
            conditions.Add(condition);
            return this;
        }

        public MethodFilter Has<TAttribute>() where TAttribute : Attribute
        {
            return Where(method => method.Has<TAttribute>());
        }

        public MethodFilter HasOrInherits<TAttribute>() where TAttribute : Attribute
        {
            return Where(method => method.HasOrInherits<TAttribute>());
        }

        public IEnumerable<MethodInfo> Filter(Type type)
        {
            return type.GetMethods(BindingFlags.Public | BindingFlags.Instance).Where(IsMatch).ToArray();
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