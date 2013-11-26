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

            return genericArguments.Select(genericArgument => ResolveTypeArgument(genericArgument, parameterTypes, parameters)).ToArray();
        }

        static Type ResolveTypeArgument(Type genericArgument, Type[] parameterTypes, object[] parameters)
        {
            bool hasNullValue = false;
            Type resolvedTypeOfNonNullValues = null;

            for (int i = 0; i < parameterTypes.Length; i++)
            {
                if (parameterTypes[i] == genericArgument)
                {
                    object parameterValue = parameters[i];

                    if (parameterValue == null)
                        hasNullValue = true;
                    else if (resolvedTypeOfNonNullValues == null)
                        resolvedTypeOfNonNullValues = parameterValue.GetType();
                    else if (resolvedTypeOfNonNullValues != parameterValue.GetType())
                        return typeof(object);
                }
            }

            if (resolvedTypeOfNonNullValues == null)
                return typeof(object);

            return hasNullValue && resolvedTypeOfNonNullValues.IsValueType ? typeof(object) : resolvedTypeOfNonNullValues;
        }
    }
}