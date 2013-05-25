using System;
using System.Linq;
using System.Reflection;
using Fixie.Behaviors;
using Fixie.Conventions;

namespace Fixie.Samples.NUnitStyle
{
    public class CustomConvention : Convention
    {
        readonly MethodFilter fixtureSetUps = new MethodFilter().HasOrInherits<TestFixtureSetUpAttribute>();
        readonly MethodFilter fixtureTearDowns = new MethodFilter().HasOrInherits<TestFixtureTearDownAttribute>();

        readonly MethodFilter setUps = new MethodFilter().HasOrInherits<SetUpAttribute>();
        readonly MethodFilter tearDowns = new MethodFilter().HasOrInherits<TearDownAttribute>();

        public CustomConvention()
        {
            Fixtures
                .HasOrInherits<TestFixtureAttribute>();

            Cases
                .HasOrInherits<TestAttribute>();

            CaseExecutionBehavior = new SetUpTearDown(setUps, CaseExecutionBehavior, tearDowns);

            FixtureExecutionBehavior = new CreateInstancePerFixture(fixtureSetUps, fixtureTearDowns);
        }
    }

    public class CreateInstancePerFixture : TypeBehavior
    {
        readonly Invoke invoke;
        readonly MethodFilter fixtureSetUps;
        readonly MethodFilter fixtureTearDowns;

        public CreateInstancePerFixture(MethodFilter fixtureSetUps, MethodFilter fixtureTearDowns)
        {
            invoke = new Invoke();
            this.fixtureSetUps = fixtureSetUps;
            this.fixtureTearDowns = fixtureTearDowns;
        }

        public void Execute(Type fixtureClass, Convention convention, Listener listener)
        {
            var caseMethods = convention.CaseMethods(fixtureClass).ToArray();
            var exceptionsByCase = caseMethods.ToDictionary(x => x, x => new ExceptionList());

            object instance;
            var constructionExceptions = new ExceptionList();
            if (TryConstruct(fixtureClass, constructionExceptions, out instance))
            {
                var fixtureSetUpExceptions = new ExceptionList();
                foreach (var fixtureSetUp in fixtureSetUps.Filter(fixtureClass))
                    invoke.Execute(fixtureSetUp, instance, fixtureSetUpExceptions);
                if (fixtureSetUpExceptions.Any())
                {
                    foreach (var caseMethod in caseMethods)
                        exceptionsByCase[caseMethod].Add(fixtureSetUpExceptions);
                }
                else
                {
                    foreach (var caseMethod in caseMethods)
                        convention.CaseExecutionBehavior.Execute(caseMethod, instance, exceptionsByCase[caseMethod]);
                
                    var fixtureTearDownExceptions = new ExceptionList();
                    foreach (var fixtureTearDown in fixtureTearDowns.Filter(fixtureClass))
                        invoke.Execute(fixtureTearDown, instance, fixtureTearDownExceptions);
                    foreach (var caseMethod in caseMethods)
                        exceptionsByCase[caseMethod].Add(fixtureTearDownExceptions);
                }

                var disposalExceptions = new ExceptionList();
                Dispose(instance, disposalExceptions);
                foreach (var caseMethod in caseMethods)
                    exceptionsByCase[caseMethod].Add(disposalExceptions);
            }
            else
            {
                foreach (var caseMethod in caseMethods)
                    exceptionsByCase[caseMethod].Add(constructionExceptions);
            }

            foreach (var caseMethod in caseMethods)
            {
                var @case = fixtureClass.FullName + "." + caseMethod.Name;
                var exceptions = exceptionsByCase[caseMethod];

                if (exceptions.Any())
                    listener.CaseFailed(@case, exceptions.ToArray());
                else
                    listener.CasePassed(@case);
            }
        }

        static bool TryConstruct(Type fixtureClass, ExceptionList exceptions, out object instance)
        {
            try
            {
                instance = Activator.CreateInstance(fixtureClass);
                return true;
            }
            catch (TargetInvocationException ex)
            {
                exceptions.Add(ex.InnerException);
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
            }

            instance = null;
            return false;
        }

        static void Dispose(object instance, ExceptionList exceptions)
        {
            try
            {
                var disposable = instance as IDisposable;
                if (disposable != null)
                    disposable.Dispose();
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
            }
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