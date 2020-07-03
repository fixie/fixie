namespace Fixie.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    static class GenericArgumentResolver
    {
        public static MethodInfo TryResolveTypeArguments(this MethodInfo caseMethod, object?[] arguments)
        {
            if (!caseMethod.IsGenericMethodDefinition)
                return caseMethod;

            var typeArguments = ResolveTypeArguments(caseMethod, arguments);

            try
            {
                return caseMethod.MakeGenericMethod(typeArguments);
            }
            catch (Exception)
            {
                return caseMethod;
            }
        }

        static Type[] ResolveTypeArguments(MethodInfo method, object?[] arguments)
        {
            var genericArguments = method.GetGenericArguments();
            var parameterTypes = method.GetParameters().Select(p => p.ParameterType).ToArray();

            if (parameterTypes.Length != arguments.Length)
            {
                var allAmbiguous = new Type[genericArguments.Length];
                Array.Fill(allAmbiguous, typeof(object));
                return allAmbiguous;
            }

            var genericToSpecific = new Dictionary<Type, Type>();

            for (int i = 0; i < parameterTypes.Length; i++)
        {
                var argument = arguments[i];
                var parameterType = parameterTypes[i];

                if (argument == null)
                    continue;

                TraverseTypes(genericToSpecific, parameterType, argument.GetType());
            }

            return genericArguments
                .Select(genericArgument =>
                    genericToSpecific.TryGetValue(genericArgument, out var specificType)
                        ? specificType
                        : typeof(object)).ToArray();
        }

        static void TraverseTypes(Dictionary<Type, Type> genericToSpecific, Type parameterType, Type argumentType)
            {
            if (parameterType.IsGenericParameter)
                {
                // A type mapping has been detected:
                //   Parameter: T, Argument: Dictionary<int, string>

                var genericTypeParameterHasAlreadyBeenFixed = genericToSpecific.ContainsKey(parameterType);

                // The first mapping wins. Subsequent ambiguity will be detected by MethodInfo.Invoke(...).
                if (genericTypeParameterHasAlreadyBeenFixed)
                    return;

                genericToSpecific[parameterType] = argumentType;
                return;
            }

            // Non-generics provide no new type mappings:
            //   Parameter: int, Argument: bool
            //   Parameter: List<int>, Argument: bool
            //   Parameter: int, Argument: List<bool>
            if (!parameterType.IsGenericType || !argumentType.IsGenericType)
                return;

            // A perfect match provides no new type mappings:
            //   Parameter: Dictionary<int, bool>, Argument: Dictionary<int, bool>
            //   Parameter: Dictionary<T, bool>, Argument: Dictionary<T, bool>
            if (parameterType == argumentType)
                return;

            var parameterGenericTypeDefinition = parameterType.GetGenericTypeDefinition();
            var argumentGenericTypeDefinition = argumentType.GetGenericTypeDefinition();

            // A complete mismatch provides no new type mappings:
            //   Parameter: Nullable<T>, Argument: Dictionary<int, bool>
            if (parameterGenericTypeDefinition != argumentGenericTypeDefinition)
                return;

            // The generic types are compatible and may provide more type mappings:
            //   Parameter: Dictionary<T1, List<T2>>, Argument: Dictionary<int, List<bool>>
            var parameterTypeGenericArguments = parameterType.GetGenericArguments(); // [T1, List<T2>]
            var argumentTypeGenericArguments = argumentType.GetGenericArguments(); // [int, List<bool>]

            for (int i = 0; i < parameterTypeGenericArguments.Length; i++)
            {
                TraverseTypes(genericToSpecific,
                    parameterTypeGenericArguments[i],
                    argumentTypeGenericArguments[i]);
            }
        }
    }
}