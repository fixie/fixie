namespace Fixie.Tests
{
    using System;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using Assertions;
    using Fixie.Internal;

    public abstract class InstrumentedExecutionTests
    {
        static string? FailingMember;
        static int? FailingMemberOccurrence;

        protected InstrumentedExecutionTests()
        {
            FailingMember = null;
            FailingMemberOccurrence = null;
        }

        protected static void FailDuring(string failingMemberName, int? occurrence = null)
        {
            FailingMember = failingMemberName;
            FailingMemberOccurrence = occurrence;
        }

        protected class Output
        {
            readonly string namespaceQualifiedTestClass;
            readonly string[] lifecycle;
            readonly string[] results;

            public Output(string namespaceQualifiedTestClass, string[] lifecycle, string[] results)
            {
                this.namespaceQualifiedTestClass = namespaceQualifiedTestClass;
                this.lifecycle = lifecycle;
                this.results = results;
            }

            public void ShouldHaveLifecycle(params string[] expected)
            {
                lifecycle.ShouldBe(expected);
            }

            public void ShouldHaveResults(params string[] expected)
            {
                var namespaceQualifiedExpectation = expected.Select(x => namespaceQualifiedTestClass + "+" + x).ToArray();

                results.ShouldBe(namespaceQualifiedExpectation);
            }
        }

        protected static void WhereAmI(object parameter, [CallerMemberName] string member = default!)
        {
            System.Console.WriteLine($"{member}({parameter})");

            ProcessScriptedFailure(member);
        }
        
        protected static void WhereAmI([CallerMemberName] string member = default!)
        {
            System.Console.WriteLine(member);

            ProcessScriptedFailure(member);
        }

        static void ProcessScriptedFailure(string member)
        {
            if (FailingMember == member)
            {
                if (FailingMemberOccurrence == null)
                    throw new FailureException(member);

                if (FailingMemberOccurrence > 0)
                {
                    FailingMemberOccurrence--;

                    if (FailingMemberOccurrence == 0)
                        throw new FailureException(member);
                }
            }
        }

        protected Output Run<TSampleTestClass, TExecution>() where TExecution : Execution, new()
            => Run<TExecution>(typeof(TSampleTestClass));

        protected Output Run<TSampleTestClass>(Execution execution)
            => Run(typeof(TSampleTestClass), execution).GetAwaiter().GetResult();

        protected Output Run<TExecution>(Type testClass) where TExecution : Execution, new()
            => Run(testClass, new TExecution()).GetAwaiter().GetResult();

        protected async Task<Output> Run(Type testClass, Execution execution)
        {
            using var console = new RedirectedConsole();

            var results = await Utility.Run(testClass, execution);

            return new Output(GetType().FullName!, console.Lines().ToArray(), results.ToArray());
        }
    }
}