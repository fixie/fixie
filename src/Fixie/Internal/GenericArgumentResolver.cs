using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Fixie.Internal;

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

        List<Type> results = [];
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

        // Nullable<T> is a generic type with the unusual property that it can provide a type mapping
        // when the argument is a non-generic value type. For example, Nullable<T> can receive an int,
        // giving us a mapping from T to int.
        //
        // We cannot simply check for `parameterType == typeof(Nullable<>)`, though, because an unresolved
        // nullable parameter type would be some generic Nullable<T1> or Nullable<T2> or... Though not yet
        // resolved, that IS NOT equal to the more general Nullable<> generic type definition.
        //
        // The reflection API makes this distinction because the compiler must as well. Consider a generic
        // method that accepts 2 arguments, `Nullable<T>` and `Nullable<U>`: these must be assumed to be
        // different types even though they have not been fully resolved yet. The compiler must distinguish
        // them, and we must likewise avoid conflating them in the genericToSpecific type mapping.
        //
        // Instead, we check whether we are dealing with *any* unresolved generic whose generic type
        // definition is typeof(Nullable<>).
        if (parameterType.IsGenericType &&
            parameterType.GetGenericTypeDefinition() == typeof(Nullable<>) &&
            parameterType.ContainsGenericParameters)
        {
            if (!argumentType.IsGenericType && argumentType.IsValueType)
            {
                var genericTypeParameter = parameterType.GetGenericArguments().Single();
                    
                TraverseTypes(genericToSpecific, genericTypeParameter, argumentType);
                return;
            }
        }

        // With Nullable<T> handled, any remaining non-generics provide no new type mappings:
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