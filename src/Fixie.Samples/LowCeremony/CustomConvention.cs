namespace Fixie.Samples.LowCeremony
{
    using System;
    using System.Linq;
    using System.Reflection;

    public class CustomConvention : Convention
    {
        static readonly string[] LifecycleMethods = { "FixtureSetUp", "FixtureTearDown", "SetUp", "TearDown" };

        public CustomConvention()
        {
            Classes
                .InTheSameNamespaceAs(typeof(CustomConvention))
                .NameEndsWith("Tests");

            Methods
                .Where(method => method.IsVoid())
                .Where(method => LifecycleMethods.All(x => x != method.Name));

            ClassExecution
                .Lifecycle<CallSetUpTearDownMethodsByName>()
                .SortCases((caseA, caseB) => String.Compare(caseA.Name, caseB.Name, StringComparison.Ordinal));
        }

        class CallSetUpTearDownMethodsByName : Lifecycle
        {
            public void Execute(Type testClass, Action<CaseAction> runCases)
            {
                var instance = Activator.CreateInstance(testClass);

                testClass.TryInvoke("FixtureSetUp", instance);
                runCases(@case =>
                {
                    @case.Class.TryInvoke("SetUp", instance);
                    @case.Execute(instance);
                    @case.Class.TryInvoke("TearDown", instance);
                });
                testClass.TryInvoke("FixtureTearDown", instance);

                (instance as IDisposable)?.Dispose();
            }
        }
    }

    public static class BehaviorBuilderExtensions
    {
        public static void TryInvoke(this Type type, string method, object instance)
        {
            var lifecycleMethod =
                type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                    .SingleOrDefault(x => x.Name == method &&
                                          x.ReturnType == typeof(void) &&
                                          x.GetParameters().Length == 0);

            if (lifecycleMethod == null)
                return;

            try
            {
                lifecycleMethod.Invoke(instance, null);
            }
            catch (TargetInvocationException exception)
            {
                throw new PreservedException(exception.InnerException);
            }
        }
    }
}