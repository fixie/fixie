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
            SampleTestClass.ForceConstructorFailure = false;
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
            SampleTestClass.ForceConstructorFailure = true;
            builder.CreateInstancePerCase();

            using (var console = new RedirectedConsole())
            {
                builder.Behavior.Execute(testClass, convention, cases);

                cases[0].Exceptions.ToArray().Single().Message.ShouldEqual("'.ctor' failed!");
                cases[1].Exceptions.ToArray().Single().Message.ShouldEqual("'.ctor' failed!");

                console.Lines.ShouldEqual("Construct", "Construct");
            }
        }

        public void ShouldFailAllCasesWhenCreatingInstancePerTestClassAndFactoryContributesExceptions()
        {
            SampleTestClass.ForceConstructorFailure = true;
            builder.CreateInstancePerTestClass();

            using (var console = new RedirectedConsole())
            {
                builder.Behavior.Execute(testClass, convention, cases);

                cases[0].Exceptions.ToArray().Single().Message.ShouldEqual("'.ctor' failed!");
                cases[1].Exceptions.ToArray().Single().Message.ShouldEqual("'.ctor' failed!");

                console.Lines.ShouldEqual("Construct");
            }
        }

        public void ShouldHandleCatastrophicExceptionsByFailingAllCasesWhenCreatingInstancePerCaseAndFactoriesThrowRatherThanContributeExceptions()
        {
            builder.CreateInstancePerCase(UnsafeCreateInstance);

            using (var console = new RedirectedConsole())
            {
                builder.Behavior.Execute(testClass, convention, cases);

                cases[0].Exceptions.ToArray().Single().Message.ShouldEqual("'UnsafeCreateInstance' failed!");
                cases[1].Exceptions.ToArray().Single().Message.ShouldEqual("'UnsafeCreateInstance' failed!");

                console.Lines.ShouldBeEmpty();
            }
        }

        public void ShouldHandleCatastrophicExceptionsByFailingAllCasesWhenCreatingInstancePerTestClassAndFactoriesThrowRatherThanContributeExceptions()
        {
            builder.CreateInstancePerTestClass(UnsafeCreateInstance);

            using (var console = new RedirectedConsole())
            {
                builder.Behavior.Execute(testClass, convention, cases);

                cases[0].Exceptions.ToArray().Single().Message.ShouldEqual("'UnsafeCreateInstance' failed!");
                cases[1].Exceptions.ToArray().Single().Message.ShouldEqual("'UnsafeCreateInstance' failed!");

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

                cases[0].Exceptions.ToArray().Single().Message.ShouldEqual("'FailingSetUp' failed!");
                cases[1].Exceptions.ToArray().Single().Message.ShouldEqual("'FailingSetUp' failed!");

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

                cases[0].Exceptions.ToArray().Single().Message.ShouldEqual("'FailingTearDown' failed!");
                cases[1].Exceptions.ToArray().Select(x => x.Message).ShouldEqual(
                    "'Fail' failed!",
                    "'FailingTearDown' failed!");

                console.Lines.ShouldEqual("SetUp", "Construct", "Pass", "Dispose", "Construct", "Fail", "Dispose", "FailingTearDown Contributes an Exception!");
            }
        }

        class SampleTestClass : IDisposable
        {
            public static bool ForceConstructorFailure = false;

            public SampleTestClass()
            {
                Console.WriteLine("Construct");

                if (ForceConstructorFailure)
                    throw new FailureException();
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

        static object CreateInstance(Type testClass)
        {
            Console.WriteLine("Factory");
            return new SampleTestClass();
        }

        static object UnsafeCreateInstance(Type testClass)
        {
            throw new FailureException();
        }

        static void SetUp(Type testClass)
        {
            Console.WriteLine("SetUp");
        }

        static void FailingSetUp(Type testClass)
        {
            Console.WriteLine("FailingSetUp Contributes an Exception!");
            throw new FailureException();
        }

        static void TearDown(Type testClass)
        {
            Console.WriteLine("TearDown");
        }

        static void FailingTearDown(Type testClass)
        {
            Console.WriteLine("FailingTearDown Contributes an Exception!");
            throw new FailureException();
        }

        static MethodInfo Method(string name)
        {
            return typeof(SampleTestClass).GetMethod(name, BindingFlags.Instance | BindingFlags.Public);
        }
    }
}