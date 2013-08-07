using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Fixie.Conventions
{
    public interface ICaseFilter
    {
        IEnumerable<MemberInfo> Filter(Type type);
    }

    public class CustomCaseFilter : ICaseFilter
    {
        Func<Type, IEnumerable<MemberInfo>> filter;

        public IEnumerable<MemberInfo> Filter(Type type)
        {
            return filter.Invoke(type);
        }

        public ICaseFilter SetFilterMethod(Func<Type, IEnumerable<MemberInfo>> filter)
        {
            this.filter = filter;
            return this;
        }
    }

    public class MethodFilter : ICaseFilter
    {
        readonly List<Func<MethodInfo, bool>> conditions;

        public MethodFilter()
        {
            conditions = new List<Func<MethodInfo, bool>>{m => !m.IsDispose()};

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

        public MethodFilter ZeroParameters()
        {
            return Where(method => method.GetParameters().Length == 0);
        }

        public IEnumerable<MemberInfo> Filter(Type type)
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