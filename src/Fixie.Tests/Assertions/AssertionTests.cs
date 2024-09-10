﻿using static Fixie.Tests.Assertions.Utility;
using static Fixie.Tests.Utility;

namespace Fixie.Tests.Assertions;

public class AssertionTests
{
    public void ShouldAssertEquatables()
    {
        HttpMethod.Post.ShouldBe(HttpMethod.Post);
        Contradiction(HttpMethod.Post, x => x.ShouldBe(HttpMethod.Get), "x should be GET but was POST");
    }

    public void ShouldAssertTypes()
    {
        typeof(int).ShouldBe(typeof(int));
        typeof(char).ShouldBe(typeof(char));
        Contradiction(typeof(Utility), x => x.ShouldBe(typeof(AssertionTests)), "x should be typeof(Fixie.Tests.Assertions.AssertionTests) but was typeof(Fixie.Tests.Assertions.Utility)");
        Contradiction(typeof(bool), x => x.ShouldBe(typeof(AssertionTests)), "x should be typeof(Fixie.Tests.Assertions.AssertionTests) but was typeof(bool)");
        Contradiction(typeof(sbyte), x => x.ShouldBe(typeof(AssertionTests)), "x should be typeof(Fixie.Tests.Assertions.AssertionTests) but was typeof(sbyte)");
        Contradiction(typeof(byte), x => x.ShouldBe(typeof(AssertionTests)), "x should be typeof(Fixie.Tests.Assertions.AssertionTests) but was typeof(byte)");
        Contradiction(typeof(short), x => x.ShouldBe(typeof(AssertionTests)), "x should be typeof(Fixie.Tests.Assertions.AssertionTests) but was typeof(short)");
        Contradiction(typeof(ushort), x => x.ShouldBe(typeof(AssertionTests)), "x should be typeof(Fixie.Tests.Assertions.AssertionTests) but was typeof(ushort)");
        Contradiction(typeof(int), x => x.ShouldBe(typeof(AssertionTests)), "x should be typeof(Fixie.Tests.Assertions.AssertionTests) but was typeof(int)");
        Contradiction(typeof(uint), x => x.ShouldBe(typeof(AssertionTests)), "x should be typeof(Fixie.Tests.Assertions.AssertionTests) but was typeof(uint)");
        Contradiction(typeof(long), x => x.ShouldBe(typeof(AssertionTests)), "x should be typeof(Fixie.Tests.Assertions.AssertionTests) but was typeof(long)");
        Contradiction(typeof(ulong), x => x.ShouldBe(typeof(AssertionTests)), "x should be typeof(Fixie.Tests.Assertions.AssertionTests) but was typeof(ulong)");
        Contradiction(typeof(nint), x => x.ShouldBe(typeof(AssertionTests)), "x should be typeof(Fixie.Tests.Assertions.AssertionTests) but was typeof(nint)");
        Contradiction(typeof(nuint), x => x.ShouldBe(typeof(AssertionTests)), "x should be typeof(Fixie.Tests.Assertions.AssertionTests) but was typeof(nuint)");
        Contradiction(typeof(decimal), x => x.ShouldBe(typeof(AssertionTests)), "x should be typeof(Fixie.Tests.Assertions.AssertionTests) but was typeof(decimal)");
        Contradiction(typeof(double), x => x.ShouldBe(typeof(AssertionTests)), "x should be typeof(Fixie.Tests.Assertions.AssertionTests) but was typeof(double)");
        Contradiction(typeof(float), x => x.ShouldBe(typeof(AssertionTests)), "x should be typeof(Fixie.Tests.Assertions.AssertionTests) but was typeof(float)");
        Contradiction(typeof(char), x => x.ShouldBe(typeof(AssertionTests)), "x should be typeof(Fixie.Tests.Assertions.AssertionTests) but was typeof(char)");
        Contradiction(typeof(string), x => x.ShouldBe(typeof(AssertionTests)), "x should be typeof(Fixie.Tests.Assertions.AssertionTests) but was typeof(string)");
        Contradiction(typeof(object), x => x.ShouldBe(typeof(AssertionTests)), "x should be typeof(Fixie.Tests.Assertions.AssertionTests) but was typeof(object)");

        1.ShouldBe<int>();
        'A'.ShouldBe<char>();
        Exception exception = new DivideByZeroException();
        DivideByZeroException typedException = exception.ShouldBe<DivideByZeroException>();
        Contradiction(new StubReport(), x => x.ShouldBe<AssertionTests>(), "x should be typeof(Fixie.Tests.Assertions.AssertionTests) but was typeof(Fixie.Tests.StubReport)");
        Contradiction(true, x => x.ShouldBe<AssertionTests>(), "x should be typeof(Fixie.Tests.Assertions.AssertionTests) but was typeof(bool)");
        Contradiction((sbyte)1, x => x.ShouldBe<AssertionTests>(), "x should be typeof(Fixie.Tests.Assertions.AssertionTests) but was typeof(sbyte)");
        Contradiction((byte)1, x => x.ShouldBe<AssertionTests>(), "x should be typeof(Fixie.Tests.Assertions.AssertionTests) but was typeof(byte)");
        Contradiction((short)1, x => x.ShouldBe<AssertionTests>(), "x should be typeof(Fixie.Tests.Assertions.AssertionTests) but was typeof(short)");
        Contradiction((ushort)1, x => x.ShouldBe<AssertionTests>(), "x should be typeof(Fixie.Tests.Assertions.AssertionTests) but was typeof(ushort)");
        Contradiction((int)1, x => x.ShouldBe<AssertionTests>(), "x should be typeof(Fixie.Tests.Assertions.AssertionTests) but was typeof(int)");
        Contradiction((uint)1, x => x.ShouldBe<AssertionTests>(), "x should be typeof(Fixie.Tests.Assertions.AssertionTests) but was typeof(uint)");
        Contradiction((long)1, x => x.ShouldBe<AssertionTests>(), "x should be typeof(Fixie.Tests.Assertions.AssertionTests) but was typeof(long)");
        Contradiction((ulong)1, x => x.ShouldBe<AssertionTests>(), "x should be typeof(Fixie.Tests.Assertions.AssertionTests) but was typeof(ulong)");
        Contradiction((nint)1, x => x.ShouldBe<AssertionTests>(), "x should be typeof(Fixie.Tests.Assertions.AssertionTests) but was typeof(nint)");
        Contradiction((nuint)1, x => x.ShouldBe<AssertionTests>(), "x should be typeof(Fixie.Tests.Assertions.AssertionTests) but was typeof(nuint)");
        Contradiction((decimal)1, x => x.ShouldBe<AssertionTests>(), "x should be typeof(Fixie.Tests.Assertions.AssertionTests) but was typeof(decimal)");
        Contradiction((double)1, x => x.ShouldBe<AssertionTests>(), "x should be typeof(Fixie.Tests.Assertions.AssertionTests) but was typeof(double)");
        Contradiction((float)1, x => x.ShouldBe<AssertionTests>(), "x should be typeof(Fixie.Tests.Assertions.AssertionTests) but was typeof(float)");
        Contradiction((char)1, x => x.ShouldBe<AssertionTests>(), "x should be typeof(Fixie.Tests.Assertions.AssertionTests) but was typeof(char)");
        Contradiction("A", x => x.ShouldBe<AssertionTests>(), "x should be typeof(Fixie.Tests.Assertions.AssertionTests) but was typeof(string)");
        Contradiction(new object(), x => x.ShouldBe<AssertionTests>(), "x should be typeof(Fixie.Tests.Assertions.AssertionTests) but was typeof(object)");
        Contradiction((Exception?)null, x => x.ShouldBe<AssertionTests>(), "x should be typeof(Fixie.Tests.Assertions.AssertionTests) but was null");
        Contradiction((AssertionTests?)null, x => x.ShouldBe<AssertionTests>(), "x should be typeof(Fixie.Tests.Assertions.AssertionTests) but was null");
    }

    public void ShouldAssertObjects()
    {
        var objectA = new SampleA();
        var objectB = new SampleB();

        objectA.ShouldBe(objectA);
        objectB.ShouldBe(objectB);

        Contradiction(objectB, x => x.ShouldBe((object?)null),
            $"x should be null but was {FullName<SampleB>()}");
        Contradiction(objectB, x => x.ShouldBe(objectA),
            $"x should be {FullName<SampleA>()} but was {FullName<SampleB>()}");
        Contradiction(objectA, x => x.ShouldBe(objectB),
            $"x should be {FullName<SampleB>()} but was {FullName<SampleA>()}");
    }

    public void ShouldAssertNulls()
    {
        object? nullObject = null;
        object nonNullObject = new SampleA();

        nullObject.ShouldBe(null);
        nonNullObject.ShouldNotBeNull();

        Contradiction((object?)null, x => x.ShouldBe(nonNullObject),
            $"x should be {FullName<SampleA>()} but was null");
        Contradiction(nonNullObject, x => x.ShouldBe(null),
            $"x should be null but was {FullName<SampleA>()}");
        Contradiction((object?)null, x => x.ShouldNotBeNull(),
            "x should not be null but was null");
    }

    public void ShouldAssertLists()
    {
        new int[]{}.ShouldBe([]);

        Contradiction(new[] { 0 }, x => x.ShouldBe([]),
            """
            x should be
                [
                
                ]

            but was
                [
                    0
                ]
            """);

        Contradiction(new int[] { }, x => x.ShouldBe([0]),
            """
            x should be
                [
                    0
                ]

            but was
                [
                
                ]
            """);

        new[] { false, true, false }.ShouldBe([false, true, false]);

        Contradiction(new[] { false, true, false }, x => x.ShouldBe([false, true]),
            """
            x should be
                [
                    false,
                    true
                ]

            but was
                [
                    false,
                    true,
                    false
                ]
            """);
        
        new[] { 'A', 'B', 'C' }.ShouldBe(['A', 'B', 'C']);
        
        Contradiction(new[] { 'A', 'B', 'C' }, x => x.ShouldBe(['A', 'C']),
            """
            x should be
                [
                    'A',
                    'C'
                ]

            but was
                [
                    'A',
                    'B',
                    'C'
                ]
            """);

        new[] { "A", "B", "C" }.ShouldBe(["A", "B", "C"]);

        Contradiction(new[] { "A", "B", "C" }, x => x.ShouldBe(["A", "C"]),
            """
            x should be
                [
                    "A",
                    "C"
                ]

            but was
                [
                    "A",
                    "B",
                    "C"
                ]
            """);

        new[] { typeof(int), typeof(bool) }.ShouldBe([typeof(int), typeof(bool)]);

        Contradiction(new[] { typeof(int), typeof(bool) }, x => x.ShouldBe([typeof(bool), typeof(int)]),
            """
            x should be
                [
                    typeof(bool),
                    typeof(int)
                ]
            
            but was
                [
                    typeof(int),
                    typeof(bool)
                ]
            """);

        var sampleA = new Sample("A");
        var sampleB = new Sample("B");

        new[] { sampleA, sampleB }.ShouldBe([sampleA, sampleB]);

        Contradiction(new[] { sampleA, sampleB }, x => x.ShouldBe([sampleB, sampleA]),
            """
            x should be
                [
                    Sample B,
                    Sample A
                ]

            but was
                [
                    Sample A,
                    Sample B
                ]
            """);
    }

    public void ShouldAssertExpressions()
    {
        Contradiction(3, value => value.Should(x => x > 4), "value should be > 4 but was 3");
        Contradiction(4, value => value.Should(y => y > 4), "value should be > 4 but was 4");
        5.Should(x => x > 4);

        Contradiction(3, value => value.Should(abc => abc >= 4), "value should be >= 4 but was 3");
        4.Should(x => x >= 4);
        5.Should(x => x >= 4);

        Func<int, bool> someExpression = x => x >= 4;
        Contradiction(3, value => value.Should(someExpression), "value should be someExpression but was 3");

        var a1 = new SampleA();
        var a2 = new SampleA();

        a1.Should(x => x == a1);
        Contradiction(a1, value => value.Should(x => x == a2), "value should be == a2 but was Fixie.Tests.Assertions.AssertionTests+SampleA");
    }

    public void ShouldAssertExpectedExceptions()
    {
        var doNothing = () => { };
        Action divideByZero = () => throw new DivideByZeroException("Divided By Zero");

        divideByZero.ShouldThrow<DivideByZeroException>("Divided By Zero")
            .ShouldBe<DivideByZeroException>();

        divideByZero.ShouldThrow<Exception>("Divided By Zero")
            .ShouldBe<DivideByZeroException>();

        Contradiction(doNothing, noop => noop.ShouldThrow<DivideByZeroException>("Divided By Zero"),
            """
            noop should have thrown System.DivideByZeroException but did not
            """);

        Contradiction(divideByZero, divide => divide.ShouldThrow<DivideByZeroException>("Divided By One Minus One"),
            """
            divide should have thrown System.DivideByZeroException with message
            
                "Divided By One Minus One"
            
            but instead the message was
            
                "Divided By Zero"
            """);

        Contradiction(divideByZero, divide => divide.ShouldThrow<ArgumentNullException>("Argument Null"),
            """
            divide should have thrown System.ArgumentNullException with message
            
                "Argument Null"
            
            but instead it threw System.DivideByZeroException with message
            
                "Divided By Zero"
            """);
    }

    public async Task ShouldAssertExpectedAsyncExceptions()
    {
        var doNothing = async () => { await Task.CompletedTask; };
        var divideByZero = async () =>
        {
            await Task.CompletedTask;
            throw new DivideByZeroException("Divided By Zero");
        };

        var actualDivideByZeroException = await divideByZero.ShouldThrow<DivideByZeroException>("Divided By Zero");
        actualDivideByZeroException.ShouldBe<DivideByZeroException>();

        var actualException = await divideByZero.ShouldThrow<Exception>("Divided By Zero");
        actualException.ShouldBe<DivideByZeroException>();

        await Contradiction(doNothing, noop => noop.ShouldThrow<DivideByZeroException>("Divided By Zero"),
            """
            noop should have thrown System.DivideByZeroException but did not
            """);

        await Contradiction(divideByZero, divide => divide.ShouldThrow<DivideByZeroException>("Divided By One Minus One"),
            """
            divide should have thrown System.DivideByZeroException with message
            
                "Divided By One Minus One"

            but instead the message was
            
                "Divided By Zero"
            """);

        await Contradiction(divideByZero, divide => divide.ShouldThrow<ArgumentNullException>("Argument Null"),
            """
            divide should have thrown System.ArgumentNullException with message
            
                "Argument Null"

            but instead it threw System.DivideByZeroException with message
            
                "Divided By Zero"
            """);
    }

    class SampleA;
    class SampleB;

    class Sample(string name)
    {
        public override string ToString() => $"Sample {name}";
    }
}