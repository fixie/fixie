using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Fixie.Execution;
using Should;

namespace Fixie.Tests.Execution
{
    public static class AppDomainCommunicationAssertions
    {
        private const BindingFlags AllMembers =
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

        public static void ShouldBeSafeAppDomainCommunicationInterface(this Type crossAppDomainInterfaceType)
        {
            foreach (var method in crossAppDomainInterfaceType.GetMethods())
            {
                if (method.DeclaringType == typeof(object) || method.DeclaringType == typeof(MarshalByRefObject))
                    continue;

                if (method.Name == "InitializeLifetimeService")
                    continue;

                if (!method.IsVoid())
                {
                    IsSafeForAppDomainCommunication(method.ReturnType)
                        .ShouldBeTrue(string.Format(
                            "{0} is not an acceptable return type for method {1}.{2} " +
                            "because it will not successfully cross AppDomain boundaries.",
                            method.ReturnType,
                            crossAppDomainInterfaceType.FullName,
                            method.Name));
                }

                foreach (var parameterType in method.GetParameters().Select(x => x.ParameterType))
                {
                    IsSafeForAppDomainCommunication(parameterType)
                        .ShouldBeTrue(string.Format(
                            "{0} is not an acceptable parameter type for method {1}.{2} " +
                            "because it will not successfully cross AppDomain boundaries.",
                            parameterType,
                            crossAppDomainInterfaceType.FullName,
                            method.Name));
                }
            }
        }

        static bool IsSafeForAppDomainCommunication(Type type)
        {
            return IsSafeForAppDomainCommunication(type, new HashSet<Type>());
        }

        static bool IsSafeForAppDomainCommunication(Type type, HashSet<Type> visitedTypes)
        {
            //This test is meant to catch *typical* mistakes with AppDomain communication, but does not
            //perfectly vet all possible candiate types:
            //
            //  1. Checking for [Serializable] does not guarantee a type is truly serializable.
            //  2. Not every IReadOnlyList<T> will necessarily serialize, but it is convenient to
            //     allow it as the most likely implementations *are* serializable.
            //  3. Although reflection types like Assembly and Type are serializable, they are
            //     also not acceptable for AppDomain communication because they can cause
            //     assembly load failures at runtime.

            if (type == typeof(Listener))
            {
                //Tests use this routine to vet Listener as a safe remoting *interface*,
                //but it is also used as an *argument* on other remoting interfaces.
                //Because it is the responsibility of the Listener implementation to be
                //a valid MarshalByRefObject, there is nothing left to check for here,
                //so it is assumed to be valid.
                return true;
            }

            visitedTypes.Add(type);

            if (type == typeof(object))
                return false;

            if (type == typeof(Type))
                return false;

            if (type.IsInNamespace("System.Reflection"))
                return false;

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IReadOnlyList<>))
            {
                if (!IsSafeForAppDomainCommunication(type.GetGenericArguments().Single(), visitedTypes))
                    return false;
            }
            else if (type.IsArray)
            {
                if (!IsSafeForAppDomainCommunication(type.GetElementType(), visitedTypes))
                    return false;
            }
            else if (!type.HasOrInherits<SerializableAttribute>())
            {
                return false;
            }

            if (!type.IsArray && type.IsInNamespace("Fixie"))
            {
                foreach (var property in type.GetProperties(AllMembers))
                {
                    var propertyType = property.PropertyType;

                    if (!visitedTypes.Contains(propertyType) && !IsSafeForAppDomainCommunication(propertyType, visitedTypes))
                        return false;
                }
            }

            return true;
        }
    }
}