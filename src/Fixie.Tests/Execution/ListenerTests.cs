using System;
using System.Collections.Generic;
using System.Linq;
using Fixie.Execution;
using Should;

namespace Fixie.Tests.Execution
{
    public class ListenerTests
    {
        public void ShouldUseTypesThatAllowRunnersInOtherAppDomainsToProvideTheirOwnImplementation()
        {
            foreach (var method in typeof(Listener).GetMethods())
            {
                if (!method.IsVoid())
                    ShouldBeSafeForAppDomainCommunication(method.ReturnType);

                foreach (var parameterType in method.GetParameters().Select(x => x.ParameterType))
                    ShouldBeSafeForAppDomainCommunication(parameterType);
            }
        }

        static void ShouldBeSafeForAppDomainCommunication(Type type)
        {
            IsSafeForAppDomainCommunication(type)
                .ShouldBeTrue(string.Format(
                    "{0} is not an acceptable input/output type for a method on {1} " +
                    "because it will not successfully cross AppDomain boundaries.", type, typeof(Listener).Name));
        }

        static bool IsSafeForAppDomainCommunication(Type type)
        {
            if (type.IsInNamespace("Fixie"))
                foreach (var property in type.GetProperties())
                    if (!PassesShallowCheck(property.PropertyType))
                        return false;

            return PassesShallowCheck(type);
        }

        static bool PassesShallowCheck(Type type)
        {
            //Checking for [Serializable] is not strictly sufficient to guarantee a type is truly serializable,
            //and not every IReadOnlyList<T> will necessarily serialize, but this test is meant to catch *typical*
            //mistakes early without being excessively strict.

            //Although reflection types like Assembly and Type are serializable, they are also not acceptable
            //input/output types due to the fact that they causes assembly load failures at runtime.

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IReadOnlyList<>))
                return true;

            return type != typeof(Type) &&
                   !type.IsInNamespace("System.Reflection") &&
                   type.HasOrInherits<SerializableAttribute>();
        }
    }
}