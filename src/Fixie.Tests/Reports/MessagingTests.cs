using System.Runtime.CompilerServices;
using Fixie.Reports;
using static Fixie.Tests.Utility;

namespace Fixie.Tests.Reports;

public abstract class MessagingTests
{
    protected MessagingTests()
    {
        TestClass = FullName<SampleTestClass>();
        GenericTestClass = FullName<SampleGenericTestClass>();
    }

    protected string TestClass { get; }
    protected string GenericTestClass { get; }
    protected static Type TestClassType => typeof(SampleTestClass);

    readonly Type[] candidateTypes =
    {
        typeof(SampleTestClass),
        typeof(SampleGenericTestClass),
        typeof(EmptyTestClass)
    };

    protected class Output
    {
        public Output(string[] console)
            => Console = console;

        public string[] Console { get; }
    }

    protected async Task Discover(IReport report)
    {
        var discovery = new SelfTestDiscovery();

        using var console = new RedirectedConsole();

        await Utility.Discover(report, discovery, candidateTypes);

        console.Lines().ShouldBeEmpty();
    }

    protected Task<Output> Run(IReport report)
    {
        return Run(_ => report);
    }

    protected Task<Output> Run(IReport report, IDiscovery discovery)
    {
        return Run(_ => report, discovery);
    }

    protected Task<Output> Run(Func<TestEnvironment, IReport> getReport)
    {
        return Run(getReport, new SelfTestDiscovery());
    }

    protected async Task<Output> Run(Func<TestEnvironment, IReport> getReport, IDiscovery discovery)
    {
        var execution = new MessagingTestsExecution();

        using var console = new RedirectedConsole();

        await Utility.Run(getReport(GetTestEnvironment()), discovery, execution, candidateTypes);

        return new Output(console.Lines().ToArray());
    }

    class MessagingTestsExecution : IExecution
    {
        public async Task Run(TestSuite testSuite)
        {
            foreach (var test in testSuite.Tests)
            {
                if (test.Has<SkipAttribute>(out var skip))
                {
                    await test.Skip(skip.Reason);
                    continue;
                }

                foreach (var parameters in FromInputAttributes(test))
                    await test.Run(parameters);
            }
        }
    }

    protected class Base
    {
        public void Pass()
        {
            WhereAmI();
        }

        protected static void WhereAmI([CallerMemberName] string member = default!)
            => System.Console.WriteLine("Standard Out: " + member);
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

        const string alert = "\x26A0";
        [Skip(alert + " Skipped with attribute.")]
        public void Skip()
        {
            throw new ShouldBeUnreachableException();
        }
    }

    protected class SampleGenericTestClass
    {
        [Input("A")]
        [Input("B")]
        [Input(123)]
        public void ShouldBeString<T>(T genericArgument)
        {
            genericArgument.ShouldBe<string>();
        }
    }

    class EmptyTestClass
    {
    }

    protected static string At(string method)
        => At<SampleTestClass>(method);

    protected static string At<T>(string method)
        => Utility.At<T>(method);
}