namespace Fixie.Samples.IoC
{
    using System;

    public class CustomConvention : Convention
    {
        public CustomConvention()
        {
            Classes
                .InTheSameNamespaceAs(typeof(CustomConvention))
                .NameEndsWith("Tests");

            ClassExecution
                .Lifecycle<IocLifecycle>()
                .SortMethods((methodA, methodB) => String.Compare(methodA.Name, methodB.Name, StringComparison.Ordinal));
        }

        public class IocLifecycle : Lifecycle
        {
            public void Execute(Type testClass, Action<CaseAction> runCases)
            {
                using (var container = InitContainerForIntegrationTests())
                {
                    var instance = container.Get(testClass);

                    runCases(@case => @case.Execute(instance));
                }
            }

            static IoCContainer InitContainerForIntegrationTests()
            {
                var container = new IoCContainer();
                container.Add(typeof(IDatabase), typeof(RealDatabase));
                container.Add(typeof(IThirdPartyService), typeof(FakeThirdPartyService));
                return container;
            }
        }
    }
}