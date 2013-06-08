using System;
using Fixie.Conventions;

namespace Fixie.Samples.IoC
{
    public class CustomConvention : Convention
    {
        readonly IoCContainer container;

        public CustomConvention()
        {
            container = InitContainerForIntegrationTests();

            Fixtures
                .Where(type => type.IsInNamespace(GetType().Namespace))
                .NameEndsWith("Tests");

            Cases
                .Where(method => method.Void())
                .ZeroParameters();

            FixtureExecution
                .CreateInstancePerFixture(UsingContainer);
        }

        static IoCContainer InitContainerForIntegrationTests()
        {
            var container = new IoCContainer();
            container.Add(typeof(IDatabase), new RealDatabase());
            container.Add(typeof(IThirdPartyService), new FakeThirdPartyService());
            return container;
        }

        ExceptionList UsingContainer(Type fixtureclass, out object instance)
        {
            instance = container.Get(fixtureclass);
            return new ExceptionList();
        }
    }
}