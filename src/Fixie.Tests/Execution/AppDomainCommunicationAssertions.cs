namespace Fixie.Tests.Execution
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Should;

    public static class AppDomainCommunicationAssertions
    {
        public static void ShouldBeSafeAppDomainCommunicationInterface(this Type crossAppDomainInterfaceType)
        {
            foreach (var method in crossAppDomainInterfaceType.GetMethods())
            {
                if (method.DeclaringType == typeof(object) || method.DeclaringType == typeof(MarshalByRefObject))
                    continue;

                if (!method.IsVoid())
                {
                    IsApprovedForAppDomainCommunication(method.ReturnType)
                        .ShouldBeTrue(
                            $"{method.ReturnType} is not an acceptable return type for method {crossAppDomainInterfaceType.FullName}.{method.Name} " +
                            "because it will not successfully cross AppDomain boundaries.");
                }

                foreach (var parameterType in method.GetParameters().Select(x => x.ParameterType))
                {
                    IsApprovedForAppDomainCommunication(parameterType)
                        .ShouldBeTrue(
                            $"{parameterType} is not an acceptable parameter type for method {crossAppDomainInterfaceType.FullName}.{method.Name} " +
                            "because it will not successfully cross AppDomain boundaries.");
                }
            }
        }

        static bool IsApprovedForAppDomainCommunication(Type type)
        {
            //Although object and object[] types may or may not cross the AppDomain boundary successfully,
            //it is the responsibilty of the caller to pass safe types.

            return type == typeof(int)
                || type == typeof(string) || type == typeof(string[]) || type == typeof(IReadOnlyList<string>)
                || type == typeof(object) || type == typeof(object[]);
        }
    }
}