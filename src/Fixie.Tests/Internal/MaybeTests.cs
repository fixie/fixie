namespace Fixie.Tests.Internal;

using System;
using Assertions;
using static Fixie.Internal.Maybe;

public class MaybeTests
{
    public void ShouldProvideTryPatternShorthandForFuncWithZeroParameters()
    {
        Func<string?> returnNull = () => null;
        Func<string?> returnNotNull = () => "";

        var hasValue = Try(returnNull, out var value);
        hasValue.ShouldBe(false);
        value.ShouldBe(null);

        hasValue = Try(returnNotNull, out value);
        hasValue.ShouldBe(true);
        value.ShouldBe("");
    }

    public void ShouldProvideTryPatternShorthandForFuncWithOneParameter()
    {
        Func<string?, string?> returnThis = argument => argument;

        var hasValue = Try(returnThis, null, out var value);
        hasValue.ShouldBe(false);
        value.ShouldBe(null);

        hasValue = Try(returnThis, "Value", out value);
        hasValue.ShouldBe(true);
        value.ShouldBe("Value");
    }
}