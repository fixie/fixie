namespace Fixie.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;

    static class GenericArgumentResolver
    {
        public static MethodInfo TryResolveTypeArguments(this MethodInfo caseMethod, object?[] arguments)
        {
            if (!caseMethod.IsGenericMethodDefinition)
                return caseMethod;

            if (!TryResolveTypeArguments(caseMethod, arguments, out var resolvedTypeParameters))
                return caseMethod;

            try
            {
                return caseMethod.MakeGenericMethod(resolvedTypeParameters);
            }
            catch (Exception)
            {
                // Allow MethodInfo.Invoke(...) to provide a more useful error message.
                return caseMethod;
            }
        }

        static bool TryResolveTypeArguments(MethodInfo method, object?[] arguments, [NotNullWhen(true)] out Type[]? resolvedTypeParameters)
        {
            var genericArguments = method.GetGenericArguments();
            var parameterTypes = method.GetParameters().Select(p => p.ParameterType).ToArray();

            if (parameterTypes.Length > arguments.Length)
            {
                resolvedTypeParameters = null;
                return false;
            }

            if (parameterTypes.Length < arguments.Length)
            {
                // Proceed with type argument resolution as if the excess arguments were not provided,
                // allowing as much resolution as possible while still allowing the "Parameter count mismatch"
                // failure to arise at invocation time.

                arguments = arguments.Take(parameterTypes.Length).ToArray();
            }

            var genericToSpecific = new Dictionary<Type, Type>();

            for (int i = 0; i < parameterTypes.Length; i++)
            {
                var argument = arguments[i];
                var parameterType = parameterTypes[i];

                // The null value has no type, so it
                // provides no new type mappings.
                if (argument == null)
                    continue;

                TraverseTypes(genericToSpecific, parameterType, argument.GetType());
            }

            var results = new List<Type>();
            foreach (var genericArgument in genericArguments)
            {
                if (genericToSpecific.TryGetValue(genericArgument, out var specificType))
                {
                    results.Add(specificType);
                }
                else
                {
                    resolvedTypeParameters = null;
                    return false;
                }
            }

            resolvedTypeParameters = results.ToArray();
            return true;
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

            // Array item types may provide new type mappings:
            //   Parameter: T[], Argument: int[]
            if (parameterType.IsArray && argumentType.IsArray)
            {
                var parameterElementType = parameterType.GetElementType();
                var argumentElementType = argumentType.GetElementType();

                if (parameterElementType != null && argumentElementType != null)
                {
                    TraverseTypes(genericToSpecific, parameterElementType, argumentElementType);
                    return;
                }
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