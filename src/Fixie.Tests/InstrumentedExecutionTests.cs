using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Fixie.Tests.Assertions;

namespace Fixie.Tests;

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

    protected static void ProcessScriptedFailure([CallerMemberName] string member = default!)
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

    protected Task<Output> Run<TSampleTestClass1, TSampleTestClass2, TExecution>()
        where TExecution : IExecution, new()
        => Run(
            new[] {typeof(TSampleTestClass1), typeof(TSampleTestClass2)},
            new TExecution());

    protected Task<Output> Run<TSampleTestClass1, TSampleTestClass2>(IExecution execution)
        => Run(
            new[] {typeof(TSampleTestClass1), typeof(TSampleTestClass2)},
            execution);
   
    protected Task<Output> Run<TSampleTestClass>()
        => Run<DefaultExecution>(typeof(TSampleTestClass));

    protected Task<Output> Run<TSampleTestClass, TExecution>() where TExecution : IExecution, new()
        => Run<TExecution>(typeof(TSampleTestClass));

    protected Task<Output> Run<TSampleTestClass>(IExecution execution)
        => Run(typeof(TSampleTestClass), execution);

    protected Task<Output> Run<TExecution>(Type testClass) where TExecution : IExecution, new()
        => Run(testClass, new TExecution());

    async Task<Output> Run(Type testClass, IExecution execution)
    {
        using var console = new RedirectedConsole();

        var results = await Utility.Run(testClass, execution);

        return new Output(GetType().FullName!, console.Lines().ToArray(), results.ToArray());
    }

    async Task<Output> Run(Type[] testClasses, IExecution execution)
    {
        using var console = new RedirectedConsole();

        var results = await Utility.Run(testClasses, execution);

        return new Output(GetType().FullName!, console.Lines().ToArray(), results.ToArray());
    }
}