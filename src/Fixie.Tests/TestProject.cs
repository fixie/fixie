namespace Fixie.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    class BrokenDiscovery : IDiscovery
    {
        public IEnumerable<Type> TestClasses(IEnumerable<Type> concreteClasses)
            => concreteClasses.Where(x => false);

        public IEnumerable<MethodInfo> TestMethods(IEnumerable<MethodInfo> publicMethods)
            => publicMethods.Where(x => false);
    }

    class TestProject : ITestProject
    {
        public void Configure(TestConfiguration configuration, TestEnvironment environment)
        {
            if (environment.IsDevelopment())
                configuration.Reports.Add<DiffToolReport>();

            configuration.Conventions.Add<BrokenDiscovery, DefaultExecution>();
        }
    }
}
