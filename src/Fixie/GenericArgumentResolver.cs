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
            var parameterTypes = method.GetParameters().Select(p => p.ParameterType).ToList();

            var typeParameters = new Type[genericArguments.Length];
            for (int i = 0; i < typeParameters.Length; i++)
                typeParameters[i] = GetArgumentType(genericArguments[i], parameterTypes, parameters);
            return typeParameters;
        }

        static Type GetArgumentType(Type genericArgumentType, IList<Type> parameterTypes, object[] parameterValues)
        {
            var matchingArguments = new List<int>();
            for (int i = 0; i < parameterTypes.Count; i++)
                if (parameterTypes[i] == genericArgumentType)
                    matchingArguments.Add(i);

            if (matchingArguments.Count == 0)
                return typeof(object);

            if (matchingArguments.Count == 1)
                return parameterValues[matchingArguments[0]] == null ? typeof(object) : parameterValues[matchingArguments[0]].GetType();

            object result = Combine(parameterValues[matchingArguments[0]], parameterValues[matchingArguments[1]]);

            result = matchingArguments.Skip(2).Select(a => parameterValues[a]).Aggregate(result, Combine);
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