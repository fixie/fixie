using System;
using System.Linq;
using System.Reflection;
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

        void ShouldBeSafeForAppDomainCommunication(Type type)
        {
            IsSafeForAppDomainCommunication(type)
                .ShouldBeTrue(string.Format(
                    "{0} is not an acceptable input/output type for a method on {1} " +
                    "because it will not successfully cross AppDomain boundaries.", type, typeof(Listener).Name));
        }

        bool IsSafeForAppDomainCommunication(Type type)
        {
            //Checking for [Serializable] is not strictly sufficient to guarantee a type is truly serializable,
            //but this test is meant to catch typical mistakes early.

            //Although Assembly is serializable, it is also not an acceptable input/output type due
            //to the fact that it causes assembly load failures at runtime.

            return type != typeof(Assembly) && type.HasOrInherits<SerializableAttribute>();
        }
    }
}