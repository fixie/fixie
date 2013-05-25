using System.Reflection;
using Fixie.Behaviors;
using Fixie.Conventions;

namespace Fixie.Samples.NUnitStyle
{
    public class CustomConvention : Convention
    {
        public CustomConvention()
        {
            Fixtures
                .HasOrInherits<TestFixtureAttribute>();

            Cases
                .HasOrInherits<TestAttribute>();

            var fixtureSetUps = new MethodFilter().HasOrInherits<TestFixtureSetUpAttribute>();
            var fixtureTearDowns = new MethodFilter().HasOrInherits<TestFixtureTearDownAttribute>();

            var setUps = new MethodFilter().HasOrInherits<SetUpAttribute>();
            var tearDowns = new MethodFilter().HasOrInherits<TearDownAttribute>();

            CaseExecutionBehavior =
                new SetUpTearDown(fixtureSetUps, new SetUpTearDown(setUps, CaseExecutionBehavior, tearDowns), fixtureTearDowns);
        }
    }

    public class SetUpTearDown : MethodBehavior
    {
        readonly Invoke invoke;
        readonly MethodFilter setUps;
        readonly MethodBehavior inner;
        readonly MethodFilter tearDowns;

        public SetUpTearDown(MethodFilter setUps, MethodBehavior inner, MethodFilter tearDowns)
        {
            invoke = new Invoke();
            this.setUps = setUps;
            this.inner = inner;
            this.tearDowns = tearDowns;
        }

        public void Execute(MethodInfo method, object instance, ExceptionList exceptions)
        {
            var setUpExceptions = new ExceptionList();

            foreach (var setUp in setUps.Filter(method.ReflectedType))
                invoke.Execute(setUp, instance, setUpExceptions);

            if (setUpExceptions.Any())
            {
                exceptions.Add(setUpExceptions);
                return;
            }
            
            inner.Execute(method, instance, exceptions);

            foreach (var tearDown in tearDowns.Filter(method.ReflectedType))
                invoke.Execute(tearDown, instance, exceptions);
        }
    }
}