namespace Fixie.Internal
{
    using System;
    using System.Linq;
    using System.Reflection;

    static class GenericArgumentResolver
    {
        public static MethodInfo TryResolveTypeArguments(this MethodInfo caseMethod, object?[] parameters)
        {
            if (!caseMethod.IsGenericMethodDefinition)
                return caseMethod;

            var typeArguments = ResolveTypeArguments(caseMethod, parameters);

            try
            {
                return caseMethod.MakeGenericMethod(typeArguments);
            }
            catch (Exception)
            {
                return caseMethod;
            }
        }

        static Type[] ResolveTypeArguments(MethodInfo method, object?[] parameters)
        {
            var genericArguments = method.GetGenericArguments();
            var parameterTypes = method.GetParameters().Select(p => p.ParameterType).ToArray();

            if (parameterTypes.Length != parameters.Length)
            {
                var allAmbiguous = new Type[genericArguments.Length];
                Array.Fill(allAmbiguous, typeof(object));
                return allAmbiguous;
            }

            return genericArguments.Select(genericArgument => ResolveTypeArgument(genericArgument, parameterTypes, parameters)).ToArray();
        }

        static Type ResolveTypeArgument(Type genericArgument, Type[] parameterTypes, object?[] parameters)
        {
            bool hasNullValue = false;
            Type? resolvedTypeOfNonNullValues = null;

            for (int i = 0; i < parameterTypes.Length; i++)
            {
                if (parameterTypes[i] == genericArgument)
                {
                    object? parameterValue = parameters[i];

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