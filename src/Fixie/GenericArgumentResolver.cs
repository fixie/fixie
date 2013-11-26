using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Fixie
{
    public class GenericArgumentResolver
    {
        public static Type[] ResolveTypeArguments(MethodInfo method, object[] parameters)
        {
            var genericArguments = method.GetGenericArguments();
            var parameterTypes = method.GetParameters().Select(p => p.ParameterType).ToArray();

            return genericArguments.Select(genericArgument => GetArgumentType(genericArgument, parameterTypes, parameters)).ToArray();
        }

        static Type GetArgumentType(Type genericArgument, IList<Type> parameterTypes, object[] parameters)
        {
            var matchingArguments = new List<int>();
            for (int i = 0; i < parameterTypes.Count; i++)
                if (parameterTypes[i] == genericArgument)
                    matchingArguments.Add(i);

            if (matchingArguments.Count == 0)
                return typeof(object);

            if (matchingArguments.Count == 1)
                return parameters[matchingArguments[0]] == null ? typeof(object) : parameters[matchingArguments[0]].GetType();

            object result = Combine(parameters[matchingArguments[0]], parameters[matchingArguments[1]]);

            result = matchingArguments.Skip(2).Select(a => parameters[a]).Aggregate(result, Combine);
            return result == null ? typeof(object) : result.GetType();
        }

        static object Combine(object a, object b)
        {
            if (a == null)
                return b == null ? null : b.GetType().IsValueType ? null : b;
            if (b == null)
                return a.GetType().IsValueType ? null : a;
            if (a.GetType() == b.GetType())
                return a;
            return null;
        }
    }
}