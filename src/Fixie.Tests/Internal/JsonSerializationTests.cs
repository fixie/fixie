namespace Fixie.Tests.Internal;

using Assertions;
using Fixie.Internal;
using Reports;
using static System.Text.Json.JsonSerializer;

public class JsonSerializationTests : MessagingTests
{
    public void ShouldSerializeDiscoverTestsMessage()
    {
        Expect(new PipeMessage.DiscoverTests(), "{}");
    }

    public void ShouldSerializeExecuteTestsMessage()
    {
        // Unintended case allowed by the type system.
        Expect(new PipeMessage.ExecuteTests(),
            "{\"Filter\":null}");

        Expect(new PipeMessage.ExecuteTests { Filter = new string[] { } },
            "{\"Filter\":[]}");

        Expect(new PipeMessage.ExecuteTests { Filter = new[]
            {
                TestClass + ".Pass",
                GenericTestClass + ".ShouldBeString"
            } },
            "{\"Filter\":[\"Fixie.Tests.Reports.MessagingTests\\u002BSampleTestClass.Pass\",\"Fixie.Tests.Reports.MessagingTests\\u002BSampleGenericTestClass.ShouldBeString\"]}");
    }

    public void ShouldSerializeTestDiscoveredMessage()
    {
        // Unintended case allowed by the type system.
        Expect(new PipeMessage.TestDiscovered(), "{\"Test\":null}");

        Expect(new PipeMessage.TestDiscovered { Test = TestClass + ".Pass" },
            "{\"Test\":\"Fixie.Tests.Reports.MessagingTests\\u002BSampleTestClass.Pass\"}");
    }

    public void ShouldSerializeTestStartedMessage()
    {
        // Unintended case allowed by the type system.
        Expect(new PipeMessage.TestStarted(), "{\"Test\":null}");

        Expect(new PipeMessage.TestStarted { Test = TestClass + ".Pass" },
            "{\"Test\":\"Fixie.Tests.Reports.MessagingTests\\u002BSampleTestClass.Pass\"}");
    }

    public void ShouldSerializeTestSkippedMessage()
    {
        // Unintended case allowed by the type system.
        Expect(new PipeMessage.TestSkipped(),
            "{\"Reason\":null,\"Test\":null,\"TestCase\":null,\"DurationInMilliseconds\":0,\"Output\":null}");

        Expect(new PipeMessage.TestSkipped
            {
                Reason = "⚠ Skipped!",
                Test = GenericTestClass + ".ShouldBeString",
                TestCase = GenericTestClass + ".ShouldBeString<System.Int32>(123)",
                DurationInMilliseconds = 123.456d,
                Output = "Line 1\r\nLine 2"
            },
            "{\"Reason\":\"\\u26A0 Skipped!\",\"Test\":\"Fixie.Tests.Reports.MessagingTests\\u002BSampleGenericTestClass.ShouldBeString\",\"TestCase\":\"Fixie.Tests.Reports.MessagingTests\\u002BSampleGenericTestClass.ShouldBeString\\u003CSystem.Int32\\u003E(123)\",\"DurationInMilliseconds\":123.456,\"Output\":\"Line 1\\r\\nLine 2\"}");
    }

    public void ShouldSerializeTestPassedMessage()
    {
        // Unintended case allowed by the type system.
        Expect(new PipeMessage.TestPassed(),
            "{\"Test\":null,\"TestCase\":null,\"DurationInMilliseconds\":0,\"Output\":null}");

        Expect(new PipeMessage.TestPassed
            {
                Test = GenericTestClass + ".ShouldBeString",
                TestCase = GenericTestClass + ".ShouldBeString<System.Int32>(123)",
                DurationInMilliseconds = 123.456d,
                Output = "Line 1\rLine 2"
            },
            "{\"Test\":\"Fixie.Tests.Reports.MessagingTests\\u002BSampleGenericTestClass.ShouldBeString\",\"TestCase\":\"Fixie.Tests.Reports.MessagingTests\\u002BSampleGenericTestClass.ShouldBeString\\u003CSystem.Int32\\u003E(123)\",\"DurationInMilliseconds\":123.456,\"Output\":\"Line 1\\rLine 2\"}");
    }

    public void ShouldSerializeTestFailedMessage()
    {
        // Unintended case allowed by the type system.
        Expect(new PipeMessage.TestFailed(),
            "{\"Reason\":null,\"Test\":null,\"TestCase\":null,\"DurationInMilliseconds\":0,\"Output\":null}");

        var at = At<SampleGenericTestClass>("ShouldBeString[T](T genericArgument)").Replace("\\", "\\\\");

        Expect(new PipeMessage.TestFailed
            {
                Reason = new PipeMessage.Exception
                {
                    Type = "Fixie.Tests.Assertions.AssertException",
                    Message = "Expected: System.String\nActual:   System.Int32",
                    StackTrace = At<SampleGenericTestClass>("ShouldBeString[T](T genericArgument)")
                },
                Test = GenericTestClass + ".ShouldBeString",
                TestCase = GenericTestClass + ".ShouldBeString<System.Int32>(123)",
                DurationInMilliseconds = 123.456d,
                Output = "Line 1\nLine 2"
            },
            "{\"Reason\":{\"Type\":\"Fixie.Tests.Assertions.AssertException\",\"Message\":\"Expected: System.String\\nActual:   System.Int32\",\"StackTrace\":\"" + at + "\"},\"Test\":\"Fixie.Tests.Reports.MessagingTests\\u002BSampleGenericTestClass.ShouldBeString\",\"TestCase\":\"Fixie.Tests.Reports.MessagingTests\\u002BSampleGenericTestClass.ShouldBeString\\u003CSystem.Int32\\u003E(123)\",\"DurationInMilliseconds\":123.456,\"Output\":\"Line 1\\nLine 2\"}");
    }

    public void ShouldSerializeExceptionMessage()
    {
        // Unintended case allowed by the type system.
        Expect(new PipeMessage.Exception(),
            "{\"Type\":null,\"Message\":null,\"StackTrace\":null}");

        var at = At<SampleGenericTestClass>("ShouldBeString[T](T genericArgument)").Replace("\\", "\\\\");

        Expect(new PipeMessage.Exception
            {
                Type = "Fixie.Tests.Assertions.AssertException",
                Message = "Expected: System.String\nActual:   System.Int32",
                StackTrace = At<SampleGenericTestClass>("ShouldBeString[T](T genericArgument)")
            },
            "{\"Type\":\"Fixie.Tests.Assertions.AssertException\",\"Message\":\"Expected: System.String\\nActual:   System.Int32\",\"StackTrace\":\"" + at + "\"}");
    }

    public void ShouldSerializeEndOfPipeMessage()
    {
        Expect(new PipeMessage.EndOfPipe(), "{}");
    }

    static void Expect<TMessage>(TMessage message, string expectedJson)
    {
        Serialize(Deserialize<TMessage>(expectedJson)).ShouldBe(expectedJson);
        Serialize(message).ShouldBe(expectedJson);
    }
}