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
        readonly Type fixtureClass;
        readonly object instance;
        readonly Case[] cases;
        readonly Convention convention;

        public InstanceBehaviorBuilderTests()
        {
            builder = new InstanceBehaviorBuilder();
            fixtureClass = typeof(SampleFixture);
            instance = new SampleFixture();
            cases = new[]
            {
                new Case(fixtureClass, Method("Pass")),
                new Case(fixtureClass, Method("Fail"))
            };
            convention = new SelfTestConvention();
        }

        public void ShouldJustExecuteCasesByDefault()
        {
            builder.Behavior.ShouldBeType<ExecuteCases>();

            using (var console = new RedirectedConsole())
            {
                builder.Behavior.Execute(fixtureClass, instance, cases, convention);

                cases[0].Exceptions.Any().ShouldBeFalse();
                cases[1].Exceptions.ToArray().Single().Message.ShouldEqual("'Fail' failed!");

                console.Lines.ShouldEqual("Pass", "Fail");
            }
        }

        public void ShouldAllowWrappingTheBehaviorInAnother()
        {
            builder.Wrap((fixtureClass, instance, cases, convention, inner) =>
            {
                Console.WriteLine("Before");
                inner.Execute(fixtureClass, instance, cases, convention);
                Console.WriteLine("After");
            });

            using (var console = new RedirectedConsole())
            {
                builder.Behavior.Execute(fixtureClass, instance, cases, convention);

                cases[0].Exceptions.Any().ShouldBeFalse();
                cases[1].Exceptions.ToArray().Single().Message.ShouldEqual("'Fail' failed!");

                console.Lines.ShouldEqual("Before", "Pass", "Fail", "After");
            }
        }

        public void ShouldAllowWrappingTheBehaviorMultipleTimes()
        {
            builder
                .Wrap((fixtureClass, instance, cases, convention, inner) =>
                {
                    Console.WriteLine("Inner Before");
                    inner.Execute(fixtureClass, instance, cases, convention);
                    Console.WriteLine("Inner After");
                })
                .Wrap((fixtureClass, instance, cases, convention, inner) =>
                {
                    Console.WriteLine("Outer Before");
                    inner.Execute(fixtureClass, instance, cases, convention);
                    Console.WriteLine("Outer After");
                });

            using (var console = new RedirectedConsole())
            {
                builder.Behavior.Execute(fixtureClass, instance, cases, convention);

                cases[0].Exceptions.Any().ShouldBeFalse();
                cases[1].Exceptions.ToArray().Single().Message.ShouldEqual("'Fail' failed!");

                console.Lines.ShouldEqual("Outer Before", "Inner Before", "Pass", "Fail", "Inner After", "Outer After");
            }
        }

        public void ShouldHandleCatastrophicExceptionsByFailingAllCasesWhenBehaviorsThrowRatherThanContributeExceptions()
        {
            builder.Wrap((fixtureClass, instance, cases, convention, inner) =>
            {
                throw new Exception("Unsafe behavior threw!");
            });

            using (var console = new RedirectedConsole())
            {
                builder.Behavior.Execute(fixtureClass, instance, cases, convention);

                cases[0].Exceptions.ToArray().Single().Message.ShouldEqual("Unsafe behavior threw!");
                cases[1].Exceptions.ToArray().Single().Message.ShouldEqual("Unsafe behavior threw!");

                console.Lines.ShouldBeEmpty();
            }
        }

        public void ShouldAllowWrappingTheBehaviorInSetUpTearDown()
        {
            builder.SetUpTearDown(SetUp, TearDown);
        
            using (var console = new RedirectedConsole())
            {
                builder.Behavior.Execute(fixtureClass, instance, cases, convention);

                cases[0].Exceptions.Any().ShouldBeFalse();
                cases[1].Exceptions.ToArray().Single().Message.ShouldEqual("'Fail' failed!");

                console.Lines.ShouldEqual("SetUp", "Pass", "Fail", "TearDown");
            }
        }

        public void ShouldShortCircuitInnerBehaviorAndTearDownByFailingAllCasesWhenSetupContributesExceptions()
        {
            builder.SetUpTearDown(FailingSetUp, TearDown);

            using (var console = new RedirectedConsole())
            {
                builder.Behavior.Execute(fixtureClass, instance, cases, convention);

                cases[0].Exceptions.ToArray().Single().Message.ShouldEqual("Exception from FailingSetUp");
                cases[1].Exceptions.ToArray().Single().Message.ShouldEqual("Exception from FailingSetUp");

                console.Lines.ShouldEqual("FailingSetUp Contributes an Exception!");
            }
        }

        public void ShouldFailAllCasesWhenTearDownContributesExceptions()
        {
            builder.SetUpTearDown(SetUp, FailingTearDown);

            using (var console = new RedirectedConsole())
            {
                builder.Behavior.Execute(fixtureClass, instance, cases, convention);

                cases[0].Exceptions.ToArray().Single().Message.ShouldEqual("Exception from FailingTearDown");
                cases[1].Exceptions.ToArray().Select(x => x.Message).ShouldEqual(
                    "'Fail' failed!",
                    "Exception from FailingTearDown");

                console.Lines.ShouldEqual("SetUp", "Pass", "Fail", "FailingTearDown Contributes an Exception!");
            }
        }

        class SampleFixture
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
        }

        static ExceptionList SetUp(Type fixtureClass, object instance)
        {
            Console.WriteLine("SetUp");
            return new ExceptionList();
        }

        static ExceptionList FailingSetUp(Type fixtureClass, object instance)
        {
            Console.WriteLine("FailingSetUp Contributes an Exception!");
            var exceptions = new ExceptionList();
            exceptions.Add(new Exception("Exception from FailingSetUp"));
            return exceptions;
        }

        static ExceptionList TearDown(Type fixtureClass, object instance)
        {
            Console.WriteLine("TearDown");
            return new ExceptionList();
        }

        static ExceptionList FailingTearDown(Type fixtureClass, object instance)
        {
            Console.WriteLine("FailingTearDown Contributes an Exception!");
            var exceptions = new ExceptionList();
            exceptions.Add(new Exception("Exception from FailingTearDown"));
            return exceptions;
        }

        static MethodInfo Method(string name)
        {
            return typeof(SampleFixture).GetMethod(name, BindingFlags.Instance | BindingFlags.Public);
        }
    }
}