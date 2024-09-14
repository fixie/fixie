using System.Runtime.CompilerServices;
using Fixie.Assertions;
namespace Fixie.Tests;

public abstract class InstrumentedExecutionTests
{
    class ScenarioState
    {
        public string? FailingMember { get; set; }
        public int? FailingMemberOccurrence { get; set; }
        public TextWriter? Console { get; set; }
    }

    static readonly AsyncLocal<ScenarioState> State = new();

    protected InstrumentedExecutionTests()
    {
        State.Value = new ScenarioState();
    }

    protected static void FailDuring(string failingMemberName, int? occurrence = null)
    {
        State.Value.ShouldNotBeNull();
        State.Value.FailingMember = failingMemberName;
        State.Value.FailingMemberOccurrence = occurrence;
    }

    protected static void ConsoleWriteLine(string text)
    {
        State.Value.ShouldNotBeNull();
        State.Value.Console.ShouldNotBeNull();
        State.Value.Console.WriteLine(text);
    }

    protected class Output
    {
        readonly string namespaceQualifiedTestClass;
        readonly string lifecycle;
        readonly string[] results;

        public Output(string namespaceQualifiedTestClass, string lifecycle, string[] results)
        {
            this.namespaceQualifiedTestClass = namespaceQualifiedTestClass;
            this.lifecycle = lifecycle;
            this.results = results;
        }

        public void ShouldHaveLifecycle(params string[] expected)
        {
            lifecycle.ShouldBe(string.Join("", expected.Select(x => x + Environment.NewLine)));
        }

        public void ShouldHaveResults(params string[] expected)
        {
            var namespaceQualifiedExpectation = expected.Select(x => namespaceQualifiedTestClass + "+" + x).ToArray();

            results.ShouldMatch(namespaceQualifiedExpectation);
        }
    }

    protected static void WhereAmI(object parameter, [CallerMemberName] string member = default!)
    {
        ConsoleWriteLine($"{member}({parameter})");

        ProcessScriptedFailure(member);
    }
        
    protected static void WhereAmI([CallerMemberName] string member = default!)
    {
        ConsoleWriteLine(member);

        ProcessScriptedFailure(member);
    }

    protected static void ProcessScriptedFailure([CallerMemberName] string member = default!)
    {
        var state = State.Value;
        state.ShouldNotBeNull();

        if (state.FailingMember == member)
        {
            if (state.FailingMemberOccurrence == null)
                throw new FailureException(member);

            if (state.FailingMemberOccurrence > 0)
            {
                state.FailingMemberOccurrence--;

                if (state.FailingMemberOccurrence == 0)
                    throw new FailureException(member);
            }
        }
    }

    protected Task<Output> Run<TSampleTestClass1, TSampleTestClass2, TExecution>()
        where TExecution : IExecution, new()
        => Run(
            [typeof(TSampleTestClass1), typeof(TSampleTestClass2)],
            new TExecution());

    protected Task<Output> Run<TSampleTestClass1, TSampleTestClass2>(IExecution execution)
        => Run(
            [typeof(TSampleTestClass1), typeof(TSampleTestClass2)],
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
        var state = State.Value;
        state.ShouldNotBeNull();

        await using (state.Console = new StringWriter())
        {
            var results = await Utility.Run(testClass, execution, state.Console);

            return new Output(GetType().FullName!, state.Console.ToString() ?? "", results.ToArray());
        }
    }

    async Task<Output> Run(Type[] testClasses, IExecution execution)
    {
        var state = State.Value;
        state.ShouldNotBeNull();

        await using (state.Console = new StringWriter())
        {
            var results = await Utility.Run(testClasses, execution, state.Console);

            return new Output(GetType().FullName!, state.Console.ToString() ?? "", results.ToArray());
        }
    }
}