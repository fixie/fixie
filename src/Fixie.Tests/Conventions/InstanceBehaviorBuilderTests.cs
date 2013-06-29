using System;
using System.Linq;
using System.Reflection;
using Fixie.Behaviors;
using Fixie.Conventions;
using Should;

namespace Fixie.Tests.Conventions
{
    public class InstanceBehaviorBuilderTests
    {
        readonly InstanceBehaviorBuilder builder;
        readonly Fixture fixture;

        public InstanceBehaviorBuilderTests()
        {
            builder = new InstanceBehaviorBuilder();
            var testClass = typeof(SampleTestClass);
            var instance = new SampleTestClass();
            var caseExecutionBehavior = new Invoke();
            fixture = new Fixture(testClass, instance, caseExecutionBehavior,  new[]
            {
                new Case(testClass, Method("Pass")),
                new Case(testClass, Method("Fail"))
            });
        }

        public void ShouldJustExecuteCasesByDefault()
        {
            builder.Behavior.ShouldBeType<ExecuteCases>();

            using (var console = new RedirectedConsole())
            {
                builder.Behavior.Execute(fixture);

                fixture.Cases[0].Exceptions.Any().ShouldBeFalse();
                fixture.Cases[1].Exceptions.ToArray().Single().Message.ShouldEqual("'Fail' failed!");

                console.Lines.ShouldEqual("Pass", "Fail");
            }
        }

        public void ShouldAllowWrappingTheBehaviorInAnother()
        {
            builder.Wrap((fixture, inner) =>
            {
                Console.WriteLine("Before");
                inner.Execute(fixture);
                Console.WriteLine("After");
            });

            using (var console = new RedirectedConsole())
            {
                builder.Behavior.Execute(fixture);

                fixture.Cases[0].Exceptions.Any().ShouldBeFalse();
                fixture.Cases[1].Exceptions.ToArray().Single().Message.ShouldEqual("'Fail' failed!");

                console.Lines.ShouldEqual("Before", "Pass", "Fail", "After");
            }
        }

        public void ShouldAllowWrappingTheBehaviorMultipleTimes()
        {
            builder
                .Wrap((fixture, inner) =>
                {
                    Console.WriteLine("Inner Before");
                    inner.Execute(fixture);
                    Console.WriteLine("Inner After");
                })
                .Wrap((fixture, inner) =>
                {
                    Console.WriteLine("Outer Before");
                    inner.Execute(fixture);
                    Console.WriteLine("Outer After");
                });

            using (var console = new RedirectedConsole())
            {
                builder.Behavior.Execute(fixture);

                fixture.Cases[0].Exceptions.Any().ShouldBeFalse();
                fixture.Cases[1].Exceptions.ToArray().Single().Message.ShouldEqual("'Fail' failed!");

                console.Lines.ShouldEqual("Outer Before", "Inner Before", "Pass", "Fail", "Inner After", "Outer After");
            }
        }

        public void ShouldHandleCatastrophicExceptionsByFailingAllCasesWhenBehaviorsThrowRatherThanContributeExceptions()
        {
            builder.Wrap((fixture, inner) =>
            {
                throw new Exception("Unsafe behavior threw!");
            });

            using (var console = new RedirectedConsole())
            {
                builder.Behavior.Execute(fixture);

                fixture.Cases[0].Exceptions.ToArray().Single().Message.ShouldEqual("Unsafe behavior threw!");
                fixture.Cases[1].Exceptions.ToArray().Single().Message.ShouldEqual("Unsafe behavior threw!");

                console.Lines.ShouldBeEmpty();
            }
        }

        public void ShouldAllowWrappingTheBehaviorInSetUpTearDown()
        {
            builder.SetUpTearDown(SetUp, TearDown);
        
            using (var console = new RedirectedConsole())
            {
                builder.Behavior.Execute(fixture);

                fixture.Cases[0].Exceptions.Any().ShouldBeFalse();
                fixture.Cases[1].Exceptions.ToArray().Single().Message.ShouldEqual("'Fail' failed!");

                console.Lines.ShouldEqual("SetUp", "Pass", "Fail", "TearDown");
            }
        }

        public void ShouldShortCircuitInnerBehaviorAndTearDownByFailingAllCasesWhenSetupContributesExceptions()
        {
            builder.SetUpTearDown(FailingSetUp, TearDown);

            using (var console = new RedirectedConsole())
            {
                builder.Behavior.Execute(fixture);

                fixture.Cases[0].Exceptions.ToArray().Single().Message.ShouldEqual("Exception from FailingSetUp");
                fixture.Cases[1].Exceptions.ToArray().Single().Message.ShouldEqual("Exception from FailingSetUp");

                console.Lines.ShouldEqual("FailingSetUp Contributes an Exception!");
            }
        }

        public void ShouldFailAllCasesWhenTearDownContributesExceptions()
        {
            builder.SetUpTearDown(SetUp, FailingTearDown);

            using (var console = new RedirectedConsole())
            {
                builder.Behavior.Execute(fixture);

                fixture.Cases[0].Exceptions.ToArray().Single().Message.ShouldEqual("Exception from FailingTearDown");
                fixture.Cases[1].Exceptions.ToArray().Select(x => x.Message).ShouldEqual(
                    "'Fail' failed!",
                    "Exception from FailingTearDown");

                console.Lines.ShouldEqual("SetUp", "Pass", "Fail", "FailingTearDown Contributes an Exception!");
            }
        }

        public void ShouldAllowSetUpTearDownByInvokingAllMethodsFoundByMethodFilters()
        {
            var setUp = new MethodFilter().Where(m => m.Name.StartsWith("SetUp"));
            var tearDown = new MethodFilter().Where(m => m.Name.StartsWith("TearDown"));

            builder.SetUpTearDown(setUp, tearDown);

            using (var console = new RedirectedConsole())
            {
                builder.Behavior.Execute(fixture);

                fixture.Cases[0].Exceptions.Any().ShouldBeFalse();
                fixture.Cases[1].Exceptions.ToArray().Single().Message.ShouldEqual("'Fail' failed!");

                console.Lines.ShouldEqual("SetUpA", "SetUpB", "Pass", "Fail", "TearDownA", "TearDownB");
            }
        }

        class SampleTestClass
        {
            public void Pass()
            {
                Console.WriteLine("Pass");
            }

            public void Fail()
            {
                Console.WriteLine("Fail");
                throw new FailureException();
            }

            public void Ignored()
            {
                throw new ShouldBeUnreachableException();
            }

            public void SetUpA()
            {
                Console.WriteLine("SetUpA");
            }

            public void SetUpB()
            {
                Console.WriteLine("SetUpB");
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

        static ExceptionList SetUp(Fixture fixture)
        {
            Console.WriteLine("SetUp");
            return new ExceptionList();
        }

        static ExceptionList FailingSetUp(Fixture fixture)
        {
            Console.WriteLine("FailingSetUp Contributes an Exception!");
            var exceptions = new ExceptionList();
            exceptions.Add(new Exception("Exception from FailingSetUp"));
            return exceptions;
        }

        static ExceptionList TearDown(Fixture fixture)
        {
            Console.WriteLine("TearDown");
            return new ExceptionList();
        }

        static ExceptionList FailingTearDown(Fixture fixture)
        {
            Console.WriteLine("FailingTearDown Contributes an Exception!");
            var exceptions = new ExceptionList();
            exceptions.Add(new Exception("Exception from FailingTearDown"));
            return exceptions;
        }

        static MethodInfo Method(string name)
        {
            return typeof(SampleTestClass).GetMethod(name, BindingFlags.Instance | BindingFlags.Public);
        }
    }
}