using Fixie.Internal;
using Fixie.Tests.Reports;

namespace Fixie.Tests.Internal;

public class PipeMessageSerializationTests : MessagingTests
{
    public void ShouldSerializeDiscoverTestsMessage()
    {
        Expect(new PipeMessage.DiscoverTests(), "{}");
    }

    public void ShouldSerializeExecuteTestsMessage()
    {
        Expect(new PipeMessage.ExecuteTests { Filter = [] },
            "{\"Filter\":[]}");

        Expect(new PipeMessage.ExecuteTests { Filter =
            [
                TestClass + ".Pass",
                GenericTestClass + ".ShouldBeString"
            ] },
            "{\"Filter\":[\"Fixie.Tests.Reports.MessagingTests\\u002BSampleTestClass.Pass\",\"Fixie.Tests.Reports.MessagingTests\\u002BSampleGenericTestClass.ShouldBeString\"]}");
    }

    public void ShouldSerializeTestDiscoveredMessage()
    {
        Expect(new PipeMessage.TestDiscovered { Test = TestClass + ".Pass" },
            "{\"Test\":\"Fixie.Tests.Reports.MessagingTests\\u002BSampleTestClass.Pass\"}");
    }

    public void ShouldSerializeTestStartedMessage()
    {
        Expect(new PipeMessage.TestStarted { Test = TestClass + ".Pass" },
            "{\"Test\":\"Fixie.Tests.Reports.MessagingTests\\u002BSampleTestClass.Pass\"}");
    }

    public void ShouldSerializeTestSkippedMessage()
    {
        Expect(new PipeMessage.TestSkipped
            {
                Reason = "⚠ Skipped!",
                Test = GenericTestClass + ".ShouldBeString",
                TestCase = GenericTestClass + ".ShouldBeString<System.Int32>(123)",
                DurationInMilliseconds = 123.456d
            },
            "{\"Reason\":\"\\u26A0 Skipped!\",\"Test\":\"Fixie.Tests.Reports.MessagingTests\\u002BSampleGenericTestClass.ShouldBeString\",\"TestCase\":\"Fixie.Tests.Reports.MessagingTests\\u002BSampleGenericTestClass.ShouldBeString\\u003CSystem.Int32\\u003E(123)\",\"DurationInMilliseconds\":123.456}");
    }

    public void ShouldSerializeTestPassedMessage()
    {
        Expect(new PipeMessage.TestPassed
            {
                Test = GenericTestClass + ".ShouldBeString",
                TestCase = GenericTestClass + ".ShouldBeString<System.Int32>(123)",
                DurationInMilliseconds = 123.456d
            },
            "{\"Test\":\"Fixie.Tests.Reports.MessagingTests\\u002BSampleGenericTestClass.ShouldBeString\",\"TestCase\":\"Fixie.Tests.Reports.MessagingTests\\u002BSampleGenericTestClass.ShouldBeString\\u003CSystem.Int32\\u003E(123)\",\"DurationInMilliseconds\":123.456}");
    }

    public void ShouldSerializeTestFailedMessage()
    {
        var at = At<SampleGenericTestClass>("ShouldBeString[T](T genericArgument)").Replace("\\", "\\\\");

        Expect(new PipeMessage.TestFailed
            {
                Reason = new PipeMessage.Exception
                {
                    Type = "Fixie.Assertions.AssertException",
                    Message = "Expected: System.String\nActual:   System.Int32",
                    StackTrace = At<SampleGenericTestClass>("ShouldBeString[T](T genericArgument)")
                },
                Test = GenericTestClass + ".ShouldBeString",
                TestCase = GenericTestClass + ".ShouldBeString<System.Int32>(123)",
                DurationInMilliseconds = 123.456d
            },
            "{\"Reason\":{\"Type\":\"Fixie.Assertions.AssertException\",\"Message\":\"Expected: System.String\\nActual:   System.Int32\",\"StackTrace\":\"" + at + "\"},\"Test\":\"Fixie.Tests.Reports.MessagingTests\\u002BSampleGenericTestClass.ShouldBeString\",\"TestCase\":\"Fixie.Tests.Reports.MessagingTests\\u002BSampleGenericTestClass.ShouldBeString\\u003CSystem.Int32\\u003E(123)\",\"DurationInMilliseconds\":123.456}");
    }

    public void ShouldSerializeExceptionMessage()
    {
        var at = At<SampleGenericTestClass>("ShouldBeString[T](T genericArgument)").Replace("\\", "\\\\");

        Expect(new PipeMessage.Exception
            {
                Type = "Fixie.Assertions.AssertException",
                Message = "Expected: System.String\nActual:   System.Int32",
                StackTrace = At<SampleGenericTestClass>("ShouldBeString[T](T genericArgument)")
            },
            "{\"Type\":\"Fixie.Assertions.AssertException\",\"Message\":\"Expected: System.String\\nActual:   System.Int32\",\"StackTrace\":\"" + at + "\"}");
    }

    public void ShouldSerializeEndOfPipeMessage()
    {
        Expect(new PipeMessage.EndOfPipe(), "{}");
    }

    public void ShouldThrowForNullDeserialization()
    {
        Action nullMessage = () => PipeMessage.Deserialize<PipeMessage.TestFailed>("null");
        nullMessage.ShouldThrow<Exception>("Message of type Fixie.Internal.PipeMessage+TestFailed was unexpectedly null.");
    }

    static void Expect<TMessage>(TMessage message, string expectedJson)
    {
        PipeMessage.Serialize(PipeMessage.Deserialize<TMessage>(expectedJson)).ShouldBe(expectedJson);
        PipeMessage.Serialize(message).ShouldBe(expectedJson);
    }
}