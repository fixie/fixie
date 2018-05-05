namespace Fixie.Samples.IoC
{
    using System;

    public class CustomConvention : Convention
    {
        public CustomConvention()
        {
            Methods
                .OrderBy(x => x.Name, StringComparer.Ordinal);

            Classes
                .Where(x => x.IsInNamespace(GetType().Namespace))
                .Where(x => x.Name.EndsWith("Tests"));
        }

        public override void Execute(TestClass testClass)
        {
            using (var container = InitContainerForIntegrationTests())
            {
                var instance = container.Get(testClass.Type);

                testClass.RunCases(@case => @case.Execute(instance));
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