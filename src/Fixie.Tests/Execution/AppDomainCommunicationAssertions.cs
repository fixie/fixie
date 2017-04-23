namespace Fixie.Tests.Execution
{
    using System;
    using System.Linq;
    using Assertions;

    public static class AppDomainCommunicationAssertions
    {
        static readonly Type[] KnownSafeTypes =
        {
            typeof(string),
            typeof(string[]),
            typeof(int),
            typeof(TimeSpan),

            //Because we cannot fully automate verification of all cross-AppDomain
            //argument/return types, anything declared as object[] is assumed to be
            //OK and it is the caller's responsibility to pass safe types from
            //loadable assemblies.
            typeof(object[])
        };

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
                    KnownSafeTypes.Contains(method.ReturnType)
                        .ShouldBeTrue(
                            $"{method.ReturnType} is not an acceptable return type for method {crossAppDomainInterfaceType.FullName}.{method.Name} " +
                            "because it will not successfully cross AppDomain boundaries.");
                }

                foreach (var parameterType in method.GetParameters().Select(x => x.ParameterType))
                {
                    KnownSafeTypes.Contains(parameterType)
                        .ShouldBeTrue(
                            $"{parameterType} is not an acceptable parameter type for method {crossAppDomainInterfaceType.FullName}.{method.Name} " +
                            "because it will not successfully cross AppDomain boundaries.");
                }
            }
        }
    }
}