using Fixie.Behaviors;
using Fixie.Conventions;

namespace Fixie.Samples
{
    //The types in this file serve the NUnit and xUnit samples.
    //Once refined, they'll be promoted to members of the Fixie
    //namespace.

    public static class MethodFilterExtensions
    {
        public static MethodBehaviorBuilder SetUpTearDown(this MethodBehaviorBuilder builder, MethodFilter hasSetUpAttribute, MethodFilter hasTearDownAttribute)
        {
            return builder.SetUpTearDown(
                (method, instance) => hasSetUpAttribute.InvokeAll(method.ReflectedType, instance),
                (method, instance) => hasTearDownAttribute.InvokeAll(method.ReflectedType, instance));
        }

        public static InstanceBehaviorBuilder SetUpTearDown(this InstanceBehaviorBuilder builder, MethodFilter hasSetUpAttribute, MethodFilter hasTearDownAttribute)
        {
            return builder.SetUpTearDown(hasSetUpAttribute.InvokeAll, hasTearDownAttribute.InvokeAll);
        }
    }
}