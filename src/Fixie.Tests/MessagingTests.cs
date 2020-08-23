namespace Fixie.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using Assertions;
    using Fixie.Internal;
    using static Utility;

    public abstract class MessagingTests
    {
        protected MessagingTests()
        {
            TestClass = FullName<SampleTestClass>();
            GenericTestClass = FullName<SampleGenericTestClass>();
        }

        protected string TestClass { get; }
        protected string GenericTestClass { get; }
        protected Type TestClassType => typeof(SampleTestClass);

        readonly Type[] candidateTypes =
        {
            typeof(SampleTestClass),
            typeof(SampleGenericTestClass),
            typeof(EmptyTestClass)
        };

        protected void Discover(Listener listener, out IEnumerable<string> consoleLines)
        {
            var discovery = new SelfTestDiscovery();

            using var console = new RedirectedConsole();

            Utility.Discover(listener, discovery, candidateTypes);

            consoleLines = console.Lines();
        }

        protected void Run(Listener listener, out IEnumerable<string> consoleLines, Action<Discovery>? customize = null)
        {
            var discovery = new SelfTestDiscovery();
            discovery.Parameters.Add<InputAttributeParameterSource>();

            var execution = new CreateInstancePerCase();

            customize?.Invoke(discovery);

            using var console = new RedirectedConsole();

            Utility.Run(listener, discovery, execution, candidateTypes);

            consoleLines = console.Lines();
        }

        class CreateInstancePerCase : Execution
        {
            public void Execute(TestClass testClass)
            {
                testClass.RunTests(test =>
                {
                    if (test.Method.Has<SkipAttribute>(out var skip))
                    {
                        test.Skip(skip.Reason);
                        return;
                    }

                    test.RunCases(@case =>
                    {
                        @case.Execute();
                    });
                });
            }
        }

        class InputAttributeParameterSource : ParameterSource
        {
            public IEnumerable<object?[]> GetParameters(MethodInfo method)
            {
                var inputAttributes = method.GetCustomAttributes<InputAttribute>(true)
                    .OrderBy(x => x.Order)
                    .ToArray();

                if (inputAttributes.Any())
                    foreach (var input in inputAttributes)
                        yield return input.Parameters;
            }
        }

        protected class Base
        {
            public void Pass()
            {
                WhereAmI();
            }

            protected static void WhereAmI([CallerMemberName] string member = default!)
            {
                System.Console.Out.WriteLine("Console.Out: " + member);
                System.Console.Error.WriteLine("Console.Error: " + member);
            }
        }

        class SampleTestClass : Base
        {
            public void Fail()
            {
                WhereAmI();
                throw new FailureException();
            }

            public void FailByAssertion()
            {
                WhereAmI();
                1.ShouldBe(2);
            }

            [Skip]
            public void SkipWithoutReason()
            {
                throw new ShouldBeUnreachableException();
            }

            const string alert = "\x26A0";
            [Skip(alert + " Skipped with reason.")]
            public void SkipWithReason()
            {
                throw new ShouldBeUnreachableException();
            }
        }

        protected class SampleGenericTestClass
        {
            [Input(1, "abc")]
            [Input(2, 123)]
            public void ShouldBeString<T>(T genericArgument)
            {
                genericArgument.ShouldBe<string>();
            }
        }

        class EmptyTestClass
        {
        }

        [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
        class InputAttribute : Attribute
        {
            public InputAttribute(int order, params object?[] parameters)
            {
                Order = order;
                Parameters = parameters;
            }

            public int Order { get; }
            public object?[] Parameters { get; }
        }

        protected static string At(string method)
            => At<SampleTestClass>(method);

        protected static string At<T>(string method)
            => Utility.At<T>(method);

        protected static string TestClassPath()
            => PathToThisFile();
    }
}