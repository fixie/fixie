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

            Classes
                .Where(type => type.IsInNamespace(GetType().Namespace))
                .NameEndsWith("Tests");

            Cases
                .Where(method => method.Void())
                .ZeroParameters();

            ClassExecution
                .CreateInstancePerTestClass(UsingContainer);
        }

        static IoCContainer InitContainerForIntegrationTests()
        {
            var container = new IoCContainer();
            container.Add(typeof(IDatabase), new RealDatabase());
            container.Add(typeof(IThirdPartyService), new FakeThirdPartyService());
            return container;
        }

        ExceptionList UsingContainer(Type testClass, out object instance)
        {
            instance = container.Get(testClass);
            return new ExceptionList();
        }
    }
}