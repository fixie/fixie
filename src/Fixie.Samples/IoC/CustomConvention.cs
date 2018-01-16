namespace Fixie.Samples.IoC
{
    using System;

    public class CustomConvention : Convention
    {
        readonly IoCContainer container;

        public CustomConvention()
        {
            container = InitContainerForIntegrationTests();

            Classes
                .InTheSameNamespaceAs(typeof(CustomConvention))
                .NameEndsWith("Tests");

            Methods
                .Where(method => method.IsVoid());

            ClassExecution
                .Lifecycle(GetFromContainer)
                .SortCases((caseA, caseB) => String.Compare(caseA.Name, caseB.Name, StringComparison.Ordinal));
        }

        static IoCContainer InitContainerForIntegrationTests()
        {
            var container = new IoCContainer();
            container.Add(typeof(IDatabase), new RealDatabase());
            container.Add(typeof(IThirdPartyService), new FakeThirdPartyService());
            return container;
        }

        void GetFromContainer(Type testClass, Action<CaseAction> runCases)
        {
            var instance = container.Get(testClass);

            runCases(@case => { @case.Execute(instance); });

            (instance as IDisposable)?.Dispose();
        }
    }
}