using System;
using System.Linq;
using System.Reflection;
using Fixie.Conventions;
using Should;

namespace Fixie.Tests.Conventions
{
    public class TypeBehaviorBuilderTests
    {
        readonly TypeBehaviorBuilder builder;
        readonly Type fixtureClass;
        readonly Case[] cases;
        readonly Convention convention;

        public TypeBehaviorBuilderTests()
        {
            builder = new TypeBehaviorBuilder();
            fixtureClass = typeof(SampleFixture);
            cases = new[]
            {
                new Case(fixtureClass, Method("Pass")),
                new Case(fixtureClass, Method("Fail"))
            };
            convention = new SelfTestConvention();
        }

        public void ShouldHaveNoBehaviorByDefault()
        {
            builder.Behavior.ShouldBeNull();
        }

        public void ShouldAllowCreatingInstancePerCase()
        {
            builder.CreateInstancePerCase();

            using (var console = new RedirectedConsole())
            {
                builder.Behavior.Execute(fixtureClass, convention, cases);

                cases[0].Exceptions.Any().ShouldBeFalse();
                cases[1].Exceptions.ToArray().Single().Message.ShouldEqual("'Fail' failed!");

                console.Lines.ShouldEqual("Construct", "Pass", "Dispose", "Construct", "Fail", "Dispose");
            }
        }

        public void ShouldAllowCreatingInstancePerFixture()
        {
            builder.CreateInstancePerFixture();

            using (var console = new RedirectedConsole())
            {
                builder.Behavior.Execute(fixtureClass, convention, cases);

                cases[0].Exceptions.Any().ShouldBeFalse();
                cases[1].Exceptions.ToArray().Single().Message.ShouldEqual("'Fail' failed!");

                console.Lines.ShouldEqual("Construct", "Pass", "Fail", "Dispose");
            }
        }

        public void ShouldAllowCreatingInstancePerCaseUsingFactory()
        {
            builder.CreateInstancePerCase(CreateInstance);

            using (var console = new RedirectedConsole())
            {
                builder.Behavior.Execute(fixtureClass, convention, cases);

                cases[0].Exceptions.Any().ShouldBeFalse();
                cases[1].Exceptions.ToArray().Single().Message.ShouldEqual("'Fail' failed!");

                console.Lines.ShouldEqual("Factory", "Construct", "Pass", "Dispose", "Factory", "Construct", "Fail", "Dispose");
            }
        }

        public void ShouldAllowCreatingInstancePerFixtureUsingFactory()
        {
            builder.CreateInstancePerFixture(CreateInstance);

            using (var console = new RedirectedConsole())
            {
                builder.Behavior.Execute(fixtureClass, convention, cases);

                cases[0].Exceptions.Any().ShouldBeFalse();
                cases[1].Exceptions.ToArray().Single().Message.ShouldEqual("'Fail' failed!");

                console.Lines.ShouldEqual("Factory", "Construct", "Pass", "Fail", "Dispose");
            }
        }

        public void ShouldFailAllCasesWhenCreatingInstancePerCaseAndFactoryContributesExceptions()
        {
            builder.CreateInstancePerCase(FailingCreateInstance);

            using (var console = new RedirectedConsole())
            {
                builder.Behavior.Execute(fixtureClass, convention, cases);

                cases[0].Exceptions.ToArray().Single().Message.ShouldEqual("Exception from FailingCreateInstance");
                cases[1].Exceptions.ToArray().Single().Message.ShouldEqual("Exception from FailingCreateInstance");

                console.Lines.ShouldEqual("Factory Contributes an Exception!", "Factory Contributes an Exception!");
            }
        }

        public void ShouldFailAllCasesWhenCreatingInstancePerFixtureAndFactoryContributesExceptions()
        {
            builder.CreateInstancePerFixture(FailingCreateInstance);

            using (var console = new RedirectedConsole())
            {
                builder.Behavior.Execute(fixtureClass, convention, cases);

                cases[0].Exceptions.ToArray().Single().Message.ShouldEqual("Exception from FailingCreateInstance");
                cases[1].Exceptions.ToArray().Single().Message.ShouldEqual("Exception from FailingCreateInstance");

                console.Lines.ShouldEqual("Factory Contributes an Exception!");
            }
        }

        public void ShouldHandleCatastrophicExceptionsByFailingAllCasesWhenCreatingInstancePerCaseAndFactoriesThrowRatherThanContributeExceptions()
        {
            builder.CreateInstancePerCase(UnsafeCreateInstance);

            using (var console = new RedirectedConsole())
            {
                builder.Behavior.Execute(fixtureClass, convention, cases);

                cases[0].Exceptions.ToArray().Single().Message.ShouldEqual("Unsafe factory threw!");
                cases[1].Exceptions.ToArray().Single().Message.ShouldEqual("Unsafe factory threw!");

                console.Lines.ShouldBeEmpty();
            }
        }

        public void ShouldHandleCatastrophicExceptionsByFailingAllCasesWhenCreatingInstancePerFixtureAndFactoriesThrowRatherThanContributeExceptions()
        {
            builder.CreateInstancePerFixture(UnsafeCreateInstance);

            using (var console = new RedirectedConsole())
            {
                builder.Behavior.Execute(fixtureClass, convention, cases);

                cases[0].Exceptions.ToArray().Single().Message.ShouldEqual("Unsafe factory threw!");
                cases[1].Exceptions.ToArray().Single().Message.ShouldEqual("Unsafe factory threw!");

                console.Lines.ShouldBeEmpty();
            }
        }

        public void ShouldAllowWrappingTheBehaviorInAnother()
        {
            builder
                .CreateInstancePerCase()
                .Wrap((fixtureClass, convention, cases, inner) =>
                {
                    Console.WriteLine("Before");
                    inner.Execute(fixtureClass, convention, cases);
                    Console.WriteLine("After");
                });

            using (var console = new RedirectedConsole())
            {
                builder.Behavior.Execute(fixtureClass, convention, cases);

                cases[0].Exceptions.Any().ShouldBeFalse();
                cases[1].Exceptions.ToArray().Single().Message.ShouldEqual("'Fail' failed!");

                console.Lines.ShouldEqual("Before", "Construct", "Pass", "Dispose", "Construct", "Fail", "Dispose", "After");
            }
        }

        public void ShouldAllowWrappingTheBehaviorMultipleTimes()
        {
            builder
                .CreateInstancePerCase()
                .Wrap((fixtureClass, convention, cases, inner) =>
                {
                    Console.WriteLine("Inner Before");
                    inner.Execute(fixtureClass, convention, cases);
                    Console.WriteLine("Inner After");
                })
                .Wrap((fixtureClass, convention, cases, inner) =>
                {
                    Console.WriteLine("Outer Before");
                    inner.Execute(fixtureClass, convention, cases);
                    Console.WriteLine("Outer After");
                });

            using (var console = new RedirectedConsole())
            {
                builder.Behavior.Execute(fixtureClass, convention, cases);

                cases[0].Exceptions.Any().ShouldBeFalse();
                cases[1].Exceptions.ToArray().Single().Message.ShouldEqual("'Fail' failed!");

                console.Lines.ShouldEqual("Outer Before", "Inner Before",
                                          "Construct", "Pass", "Dispose",
                                          "Construct", "Fail", "Dispose",
                                          "Inner After", "Outer After");
            }
        }

        public void ShouldHandleCatastrophicExceptionsByFailingAllCasesWhenBehaviorsThrowRatherThanContributeExceptions()
        {
            builder
                .CreateInstancePerCase()
                .Wrap((fixtureClass, convention, cases, inner) =>
                {
                    throw new Exception("Unsafe behavior threw!");
                });

            using (var console = new RedirectedConsole())
            {
                builder.Behavior.Execute(fixtureClass, convention, cases);

                cases[0].Exceptions.ToArray().Single().Message.ShouldEqual("Unsafe behavior threw!");
                cases[1].Exceptions.ToArray().Single().Message.ShouldEqual("Unsafe behavior threw!");

                console.Lines.ShouldBeEmpty();
            }
        }

        public void ShouldAllowWrappingTheBehaviorInSetUpTearDown()
        {
            builder
                .CreateInstancePerCase()
                .SetUpTearDown(SetUp, TearDown);

            using (var console = new RedirectedConsole())
            {
                builder.Behavior.Execute(fixtureClass, convention, cases);

                cases[0].Exceptions.Any().ShouldBeFalse();
                cases[1].Exceptions.ToArray().Single().Message.ShouldEqual("'Fail' failed!");

                console.Lines.ShouldEqual("SetUp", "Construct", "Pass", "Dispose", "Construct", "Fail", "Dispose", "TearDown");
            }
        }

        public void ShouldShortCircuitInnerBehaviorAndTearDownByFailingAllCasesWhenSetupContributesExceptions()
        {
            builder
                .CreateInstancePerCase()
                .SetUpTearDown(FailingSetUp, TearDown);

            using (var console = new RedirectedConsole())
            {
                builder.Behavior.Execute(fixtureClass, convention, cases);

                cases[0].Exceptions.ToArray().Single().Message.ShouldEqual("Exception from FailingSetUp");
                cases[1].Exceptions.ToArray().Single().Message.ShouldEqual("Exception from FailingSetUp");

                console.Lines.ShouldEqual("FailingSetUp Contributes an Exception!");
            }
        }

        public void ShouldFailAllCasesWhenTearDownContributesExceptions()
        {
            builder
                .CreateInstancePerCase()
                .SetUpTearDown(SetUp, FailingTearDown);

            using (var console = new RedirectedConsole())
            {
                builder.Behavior.Execute(fixtureClass, convention, cases);

                cases[0].Exceptions.ToArray().Single().Message.ShouldEqual("Exception from FailingTearDown");
                cases[1].Exceptions.ToArray().Select(x => x.Message).ShouldEqual(
                    "'Fail' failed!",
                    "Exception from FailingTearDown");

                console.Lines.ShouldEqual("SetUp", "Construct", "Pass", "Dispose", "Construct", "Fail", "Dispose", "FailingTearDown Contributes an Exception!");
            }
        }

        class SampleFixture : IDisposable
        {
            public SampleFixture()
            {
                Console.WriteLine("Construct");
            }

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

            public void Dispose()
            {
                Console.WriteLine("Dispose");
            }
        }

        static ExceptionList CreateInstance(Type fixtureclass, out object instance)
        {
            Console.WriteLine("Factory");
            instance = new SampleFixture();
            return new ExceptionList();
        }

        static ExceptionList FailingCreateInstance(Type fixtureclass, out object instance)
        {
            Console.WriteLine("Factory Contributes an Exception!");
            var exceptions = new ExceptionList();
            exceptions.Add(new Exception("Exception from FailingCreateInstance"));
            instance = null;
            return exceptions;
        }

        static ExceptionList UnsafeCreateInstance(Type fixtureclass, out object instance)
        {
            throw new Exception("Unsafe factory threw!");
        }

        static ExceptionList SetUp(Type fixtureClass)
        {
            Console.WriteLine("SetUp");
            return new ExceptionList();
        }

        static ExceptionList FailingSetUp(Type fixtureClass)
        {
            Console.WriteLine("FailingSetUp Contributes an Exception!");
            var exceptions = new ExceptionList();
            exceptions.Add(new Exception("Exception from FailingSetUp"));
            return exceptions;
        }

        static ExceptionList TearDown(Type fixtureClass)
        {
            Console.WriteLine("TearDown");
            return new ExceptionList();
        }

        static ExceptionList FailingTearDown(Type fixtureClass)
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