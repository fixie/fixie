using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Fixie.Conventions
{
    public class MethodFilter
    {
        readonly List<Func<MethodInfo, bool>> conditions;
        Random shuffler;

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

        public MethodFilter Shuffle(Random random)
        {
            shuffler = random;
            return this;
        }

        public MethodFilter Shuffle()
        {
            return Shuffle(new Random());
        }

        public IEnumerable<MethodInfo> Filter(Type type)
        {
            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance).Where(IsMatch).ToArray();

            if (shuffler != null)
                methods.Shuffle(shuffler);

            return methods;
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