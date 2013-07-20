using System;
using System.Reflection;
using Fixie.Behaviors;
using Fixie.Conventions;
using Should;

namespace Fixie.Tests.Conventions
{
    public class CaseBehaviorBuilderTests
    {
        readonly CaseBehaviorBuilder builder;
        readonly object instance;

        public CaseBehaviorBuilderTests()
        {
            builder = new CaseBehaviorBuilder();
            instance = new SampleTestClass();
        }

        public void ShouldJustInvokeMethodByDefault()
        {
            builder.Behavior.ShouldBeType<Invoke>();

            using (var console = new RedirectedConsole())
            {
                var @case = Case("Pass");

                builder.Behavior.Execute(@case, instance);
                
                @case.Exceptions.Any().ShouldBeFalse();
                console.Lines.ShouldEqual("Pass");
            }

            using (var console = new RedirectedConsole())
            {
                var @case = Case("Fail");

                builder.Behavior.Execute(@case, instance);

                @case.Exceptions.Count.ShouldEqual(1);
                console.Lines.ShouldEqual("Fail Threw!");
            }
        }

        public void ShouldAllowWrappingTheBehaviorInAnother()
        {
            builder.Wrap((@case, instance, inner) =>
            {
                Console.WriteLine("Before");
                inner.Execute(@case, instance);
                Console.WriteLine("After");
            });

            using (var console = new RedirectedConsole())
            {
                var @case = Case("Pass");

                builder.Behavior.Execute(@case, instance);

                @case.Exceptions.Any().ShouldBeFalse();
                console.Lines.ShouldEqual("Before", "Pass", "After");
            }

            using (var console = new RedirectedConsole())
            {
                var @case = Case("Fail");

                builder.Behavior.Execute(@case, instance);

                @case.Exceptions.Count.ShouldEqual(1);
                console.Lines.ShouldEqual("Before", "Fail Threw!", "After");
            }
        }

        public void ShouldAllowWrappingTheBehaviorMultipleTimes()
        {
            builder.Wrap((@case, instance, inner) =>
            {
                Console.WriteLine("Inner Before");
                inner.Execute(@case, instance);
                Console.WriteLine("Inner After");
            })
            .Wrap((@case, instance, inner) =>
            {
                Console.WriteLine("Outer Before");
                inner.Execute(@case, instance);
                Console.WriteLine("Outer After");
            });

            using (var console = new RedirectedConsole())
            {
                var @case = Case("Pass");

                builder.Behavior.Execute(@case, instance);

                @case.Exceptions.Any().ShouldBeFalse();
                console.Lines.ShouldEqual("Outer Before", "Inner Before", "Pass", "Inner After", "Outer After");
            }
        }

        public void ShouldHandleCatastrophicExceptionsWhenBehaviorsThrowRatherThanContributeExceptions()
        {
            builder.Wrap((@case, instance, inner) =>
            {
                throw new Exception("Unsafe behavior threw!");
            });

            using (var console = new RedirectedConsole())
            {
                var @case = Case("Pass");

                builder.Behavior.Execute(@case, instance);

                @case.Exceptions.Count.ShouldEqual(1);
                console.Lines.ShouldBeEmpty();
            }
        }

        public void ShouldAllowWrappingTheBehaviorInSetUpTearDown()
        {
            builder.SetUpTearDown(SetUp, TearDown);

            using (var console = new RedirectedConsole())
            {
                var @case = Case("Pass");

                builder.Behavior.Execute(@case, instance);

                @case.Exceptions.Any().ShouldBeFalse();
                console.Lines.ShouldEqual("SetUp", "Pass", "TearDown");
            }
        }

        public void ShouldShortCircuitInnerBehaviorAndTearDownWhenSetupContributesExceptions()
        {
            builder.SetUpTearDown(FailingSetUp, TearDown);

            using (var console = new RedirectedConsole())
            {
                var @case = Case("Pass");

                builder.Behavior.Execute(@case, instance);

                @case.Exceptions.Count.ShouldEqual(1);
                console.Lines.ShouldEqual("FailingSetUp Contributes an Exception!");
            }
        }

        public void ShouldNotShortCircuitTearDownWhenInnerBehaviorContributesExceptions()
        {
            builder.SetUpTearDown(SetUp, TearDown);

            using (var console = new RedirectedConsole())
            {
                var @case = Case("Fail");

                builder.Behavior.Execute(@case, instance);

                @case.Exceptions.Count.ShouldEqual(1);
                console.Lines.ShouldEqual("SetUp", "Fail Threw!", "TearDown");
            }
        }

        public void ShouldFailWhenTearDownContributesExceptions()
        {
            builder.SetUpTearDown(SetUp, FailingTearDown);

            using (var console = new RedirectedConsole())
            {
                var @case = Case("Pass");

                builder.Behavior.Execute(@case, instance);

                @case.Exceptions.Count.ShouldEqual(1);
                console.Lines.ShouldEqual("SetUp", "Pass", "FailingTearDown Contributes an Exception!");
            }

            using (var console = new RedirectedConsole())
            {
                var @case = Case("Fail");

                builder.Behavior.Execute(@case, instance);

                @case.Exceptions.Count.ShouldEqual(2);
                console.Lines.ShouldEqual("SetUp", "Fail Threw!", "FailingTearDown Contributes an Exception!");
            }
        }

        public void ShouldAllowSetUpTearDownByInvokingAllMethodsFoundByMethodFilter()
        {
            var setUp = new MethodFilter().Where(m => m.Name.StartsWith("SetUp"));
            var tearDown = new MethodFilter().Where(m => m.Name.StartsWith("TearDown"));

            builder.SetUpTearDown(setUp, tearDown);

            using (var console = new RedirectedConsole())
            {
                var @case = Case("Pass");

                builder.Behavior.Execute(@case, instance);

                @case.Exceptions.Any().ShouldBeFalse();
                console.Lines.ShouldEqual("SetUpA", "SetUpB", "Pass", "TearDownA", "TearDownB");
            }
        }

        class SampleTestClass
        {
            public void SetUpA()
            {
                Console.WriteLine("SetUpA");
            }

            public void SetUpB()
            {
                Console.WriteLine("SetUpB");
            }

            public void Pass()
            {
                Console.WriteLine("Pass");
            }

            public void Fail()
            {
                Console.WriteLine("Fail Threw!");
                throw new FailureException();
            }

            public void TearDownA()
            {
                Console.WriteLine("TearDownA");
            }

            public void TearDownB()
            {
                Console.WriteLine("TearDownB");
            }
        }

        static ExceptionList SetUp(MethodInfo method, object instance)
        {
            Console.WriteLine("SetUp");
            return new ExceptionList();
        }

        static ExceptionList FailingSetUp(MethodInfo method, object instance)
        {
            Console.WriteLine("FailingSetUp Contributes an Exception!");
            var exceptions = new ExceptionList();
            exceptions.Add(new Exception());
            return exceptions;
        }

        static ExceptionList TearDown(MethodInfo method, object instance)
        {
            Console.WriteLine("TearDown");
            return new ExceptionList();
        }

        static ExceptionList FailingTearDown(MethodInfo method, object instance)
        {
            Console.WriteLine("FailingTearDown Contributes an Exception!");
            var exceptions = new ExceptionList();
            exceptions.Add(new Exception());
            return exceptions;
        }

        static Case Case(string methodName)
        {
            var testClass = typeof(SampleTestClass);
            return new Case(testClass, testClass.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public));
        }
    }
}