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
        readonly Type testClass;
        readonly Case[] cases;
        readonly Convention convention;

        public TypeBehaviorBuilderTests()
        {
            builder = new TypeBehaviorBuilder();
            testClass = typeof(SampleTestClass);
            cases = new[]
            {
                new Case(testClass, Method("Pass")),
                new Case(testClass, Method("Fail"))
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
                builder.Behavior.Execute(testClass, convention, cases);

                cases[0].Exceptions.Any().ShouldBeFalse();
                cases[1].Exceptions.ToArray().Single().Message.ShouldEqual("'Fail' failed!");

                console.Lines.ShouldEqual("Construct", "Pass", "Dispose", "Construct", "Fail", "Dispose");
            }
        }

        public void ShouldAllowCreatingInstancePerTestClass()
        {
            builder.CreateInstancePerTestClass();

            using (var console = new RedirectedConsole())
            {
                builder.Behavior.Execute(testClass, convention, cases);

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
                builder.Behavior.Execute(testClass, convention, cases);

                cases[0].Exceptions.Any().ShouldBeFalse();
                cases[1].Exceptions.ToArray().Single().Message.ShouldEqual("'Fail' failed!");

                console.Lines.ShouldEqual("Factory", "Construct", "Pass", "Dispose", "Factory", "Construct", "Fail", "Dispose");
            }
        }

        public void ShouldAllowCreatingInstancePerTestClassUsingFactory()
        {
            builder.CreateInstancePerTestClass(CreateInstance);

            using (var console = new RedirectedConsole())
            {
                builder.Behavior.Execute(testClass, convention, cases);

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
                builder.Behavior.Execute(testClass, convention, cases);

                cases[0].Exceptions.ToArray().Single().Message.ShouldEqual("Exception from FailingCreateInstance");
                cases[1].Exceptions.ToArray().Single().Message.ShouldEqual("Exception from FailingCreateInstance");

                console.Lines.ShouldEqual("Factory Contributes an Exception!", "Factory Contributes an Exception!");
            }
        }

        public void ShouldFailAllCasesWhenCreatingInstancePerTestClassAndFactoryContributesExceptions()
        {
            builder.CreateInstancePerTestClass(FailingCreateInstance);

            using (var console = new RedirectedConsole())
            {
                builder.Behavior.Execute(testClass, convention, cases);

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
                builder.Behavior.Execute(testClass, convention, cases);

                cases[0].Exceptions.ToArray().Single().Message.ShouldEqual("Unsafe factory threw!");
                cases[1].Exceptions.ToArray().Single().Message.ShouldEqual("Unsafe factory threw!");

                console.Lines.ShouldBeEmpty();
            }
        }

        public void ShouldHandleCatastrophicExceptionsByFailingAllCasesWhenCreatingInstancePerTestClassAndFactoriesThrowRatherThanContributeExceptions()
        {
            builder.CreateInstancePerTestClass(UnsafeCreateInstance);

            using (var console = new RedirectedConsole())
            {
                builder.Behavior.Execute(testClass, convention, cases);

                cases[0].Exceptions.ToArray().Single().Message.ShouldEqual("Unsafe factory threw!");
                cases[1].Exceptions.ToArray().Single().Message.ShouldEqual("Unsafe factory threw!");

                console.Lines.ShouldBeEmpty();
            }
        }

        public void ShouldAllowWrappingTheBehaviorInAnother()
        {
            builder
                .CreateInstancePerCase()
                .Wrap((testClass, convention, cases, innerBehavior) =>
                {
                    Console.WriteLine("Before");
                    innerBehavior();
                    Console.WriteLine("After");
                });

            using (var console = new RedirectedConsole())
            {
                builder.Behavior.Execute(testClass, convention, cases);

                cases[0].Exceptions.Any().ShouldBeFalse();
                cases[1].Exceptions.ToArray().Single().Message.ShouldEqual("'Fail' failed!");

                console.Lines.ShouldEqual("Before", "Construct", "Pass", "Dispose", "Construct", "Fail", "Dispose", "After");
            }
        }

        public void ShouldAllowWrappingTheBehaviorMultipleTimes()
        {
            builder
                .CreateInstancePerCase()
                .Wrap((testClass, convention, cases, innerBehavior) =>
                {
                    Console.WriteLine("Inner Before");
                    innerBehavior();
                    Console.WriteLine("Inner After");
                })
                .Wrap((testClass, convention, cases, innerBehavior) =>
                {
                    Console.WriteLine("Outer Before");
                    innerBehavior();
                    Console.WriteLine("Outer After");
                });

            using (var console = new RedirectedConsole())
            {
                builder.Behavior.Execute(testClass, convention, cases);

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
                .Wrap((testClass, convention, cases, innerBehavior) =>
                {
                    throw new Exception("Unsafe behavior threw!");
                });

            using (var console = new RedirectedConsole())
            {
                builder.Behavior.Execute(testClass, convention, cases);

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
                builder.Behavior.Execute(testClass, convention, cases);

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
                builder.Behavior.Execute(testClass, convention, cases);

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
                builder.Behavior.Execute(testClass, convention, cases);

                cases[0].Exceptions.ToArray().Single().Message.ShouldEqual("Exception from FailingTearDown");
                cases[1].Exceptions.ToArray().Select(x => x.Message).ShouldEqual(
                    "'Fail' failed!",
                    "Exception from FailingTearDown");

                console.Lines.ShouldEqual("SetUp", "Construct", "Pass", "Dispose", "Construct", "Fail", "Dispose", "FailingTearDown Contributes an Exception!");
            }
        }

        class SampleTestClass : IDisposable
        {
            public SampleTestClass()
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

        static ExceptionList CreateInstance(Type testClass, out object instance)
        {
            Console.WriteLine("Factory");
            instance = new SampleTestClass();
            return new ExceptionList();
        }

        static ExceptionList FailingCreateInstance(Type testClass, out object instance)
        {
            Console.WriteLine("Factory Contributes an Exception!");
            var exceptions = new ExceptionList();
            exceptions.Add(new Exception("Exception from FailingCreateInstance"));
            instance = null;
            return exceptions;
        }

        static ExceptionList UnsafeCreateInstance(Type testClass, out object instance)
        {
            throw new Exception("Unsafe factory threw!");
        }

        static ExceptionList SetUp(Type testClass)
        {
            Console.WriteLine("SetUp");
            return new ExceptionList();
        }

        static ExceptionList FailingSetUp(Type testClass)
        {
            Console.WriteLine("FailingSetUp Contributes an Exception!");
            var exceptions = new ExceptionList();
            exceptions.Add(new Exception("Exception from FailingSetUp"));
            return exceptions;
        }

        static ExceptionList TearDown(Type testClass)
        {
            Console.WriteLine("TearDown");
            return new ExceptionList();
        }

        static ExceptionList FailingTearDown(Type testClass)
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