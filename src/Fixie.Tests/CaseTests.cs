using System;
using Should;

namespace Fixie.Tests
{
    public class CaseTests
    {
        public void ShouldBeNamedAfterTheUnderlyingMethod()
        {
            var @case = Case("Returns");

            @case.Name.ShouldEqual("Fixie.Tests.CaseTests.Returns");
        }

        public void ShouldIncludeParmeterValuesInNameWhenTheUnderlyingMethodHasParameters()
        {
            var @case = Case("Parameterized", 123, true, 'a', "with \"quotes\"", "long \"string\" gets truncated", null, this);

            @case.Name.ShouldEqual("Fixie.Tests.CaseTests.Parameterized(123, True, 'a', \"with \\\"quotes\\\"\", \"long \\\"string\\\" g\"..., null, Fixie.Tests.CaseTests)");
        }

        public void ShouldIncludeEscapeSequencesInNameWhenTheUnderlyingMethodHasParameters()
        {
            Case("String", "\"").Name.ShouldEqual("Fixie.Tests.CaseTests.String(\"\\\"\")");
            Case("String", "\\").Name.ShouldEqual("Fixie.Tests.CaseTests.String(\"\\\\\")");
            Case("String", "\0").Name.ShouldEqual("Fixie.Tests.CaseTests.String(\"\\0\")");
            Case("String", "\a").Name.ShouldEqual("Fixie.Tests.CaseTests.String(\"\\a\")");
            Case("String", "\b").Name.ShouldEqual("Fixie.Tests.CaseTests.String(\"\\b\")");
            Case("String", "\f").Name.ShouldEqual("Fixie.Tests.CaseTests.String(\"\\f\")");
            Case("String", "\n").Name.ShouldEqual("Fixie.Tests.CaseTests.String(\"\\n\")");
            Case("String", "\r").Name.ShouldEqual("Fixie.Tests.CaseTests.String(\"\\r\")");
            Case("String", "\t").Name.ShouldEqual("Fixie.Tests.CaseTests.String(\"\\t\")");
            Case("String", "\v").Name.ShouldEqual("Fixie.Tests.CaseTests.String(\"\\v\")");

            // Unicode characters 0085, 2028, and 2029 represent line endings Next Line, Line Separator, and Paragraph Separator, respectively.
            // Just like \r and \n, we escape these in order to present a readable string literal.  All other unicode sequences pass through
            // with no additional special treatment.

            // \uxxxx - Unicode escape sequence for character with hex value xxxx.
            Case("String", "\u0000").Name.ShouldEqual("Fixie.Tests.CaseTests.String(\"\\0\")");
            Case("String", "\u0085").Name.ShouldEqual("Fixie.Tests.CaseTests.String(\"\\u0085\")");
            Case("String", "\u2028").Name.ShouldEqual("Fixie.Tests.CaseTests.String(\"\\u2028\")");
            Case("String", "\u2029").Name.ShouldEqual("Fixie.Tests.CaseTests.String(\"\\u2029\")");
            Case("String", "\u263A").Name.ShouldEqual("Fixie.Tests.CaseTests.String(\"☺\")");

            // \xn[n][n][n] - Unicode escape sequence for character with hex value nnnn (variable length version of \uxxxx).
            Case("String", "\x0000").Name.ShouldEqual("Fixie.Tests.CaseTests.String(\"\\0\")");
            Case("String", "\x000").Name.ShouldEqual("Fixie.Tests.CaseTests.String(\"\\0\")");
            Case("String", "\x00").Name.ShouldEqual("Fixie.Tests.CaseTests.String(\"\\0\")");
            Case("String", "\x0").Name.ShouldEqual("Fixie.Tests.CaseTests.String(\"\\0\")");
            Case("String", "\x0085").Name.ShouldEqual("Fixie.Tests.CaseTests.String(\"\\u0085\")");
            Case("String", "\x085").Name.ShouldEqual("Fixie.Tests.CaseTests.String(\"\\u0085\")");
            Case("String", "\x85").Name.ShouldEqual("Fixie.Tests.CaseTests.String(\"\\u0085\")");
            Case("String", "\x2028").Name.ShouldEqual("Fixie.Tests.CaseTests.String(\"\\u2028\")");
            Case("String", "\x2029").Name.ShouldEqual("Fixie.Tests.CaseTests.String(\"\\u2029\")");
            Case("String", "\x263A").Name.ShouldEqual("Fixie.Tests.CaseTests.String(\"☺\")");

            //\Uxxxxxxxx - Unicode escape sequence for character with hex value xxxxxxxx (for generating surrogates).
            Case("String", "\U00000000").Name.ShouldEqual("Fixie.Tests.CaseTests.String(\"\\0\")");
            Case("String", "\U00000085").Name.ShouldEqual("Fixie.Tests.CaseTests.String(\"\\u0085\")");
            Case("String", "\U00002028").Name.ShouldEqual("Fixie.Tests.CaseTests.String(\"\\u2028\")");
            Case("String", "\U00002029").Name.ShouldEqual("Fixie.Tests.CaseTests.String(\"\\u2029\")");
            Case("String", "\U0000263A").Name.ShouldEqual("Fixie.Tests.CaseTests.String(\"☺\")");
        }

        public void ShouldIncludeResolvedGenericArgumentsInNameWhenTheUnderlyingMethodIsGeneric()
        {
            Case("Generic", 123, true, "a", "b")
                .Name.ShouldEqual("Fixie.Tests.CaseTests.Generic<System.Boolean, System.String>(123, True, \"a\", \"b\")");

            Case("Generic", 123, true, 1, null)
                .Name.ShouldEqual("Fixie.Tests.CaseTests.Generic<System.Boolean, System.Object>(123, True, 1, null)");

            Case("Generic", 123, 1.23m, "a", null)
                .Name.ShouldEqual("Fixie.Tests.CaseTests.Generic<System.Decimal, System.String>(123, 1.23, \"a\", null)");
        }

        public void ShouldHaveMethodGroupComposedOfClassNameAndMethodNameWithNoSignature()
        {
            Case("Returns").MethodGroup.FullName.ShouldEqual("Fixie.Tests.CaseTests.Returns");
            Case("Parameterized", 123, true, 'a', "s", null, this).MethodGroup.FullName.ShouldEqual("Fixie.Tests.CaseTests.Parameterized");
            Case("Generic", 123, true, "a", "b").MethodGroup.FullName.ShouldEqual("Fixie.Tests.CaseTests.Generic");
            Case("Generic", 123, true, 1, null).MethodGroup.FullName.ShouldEqual("Fixie.Tests.CaseTests.Generic");
            Case("Generic", 123, 1.23m, "a", null).MethodGroup.FullName.ShouldEqual("Fixie.Tests.CaseTests.Generic");
        }

        public void ShouldInferAppropriateClassGivenCaseMethod()
        {
            var methodDeclaredInChildClass = new Case(typeof(SampleChildTestClass).GetInstanceMethod("TestMethodDefinedWithinChildClass"));
            methodDeclaredInChildClass.Class.ShouldEqual(typeof(SampleChildTestClass));

            var methodDeclaredInParentClass = new Case(typeof(SampleParentTestClass).GetInstanceMethod("TestMethodDefinedWithinParentClass"));
            methodDeclaredInParentClass.Class.ShouldEqual(typeof(SampleParentTestClass));

            var parentMethodInheritedByChildClass = new Case(typeof(SampleChildTestClass).GetInstanceMethod("TestMethodDefinedWithinParentClass"));
            parentMethodInheritedByChildClass.Class.ShouldEqual(typeof(SampleChildTestClass));
        }

        public void ShouldTrackExceptionsAsFailureReasons()
        {
            var exceptionA = new InvalidOperationException();
            var exceptionB = new DivideByZeroException();

            var @case = Case("Returns");

            @case.Exceptions.ShouldBeEmpty();
            @case.Fail(exceptionA);
            @case.Fail(exceptionB);
            @case.Exceptions.ShouldEqual(exceptionA, exceptionB);
        }

        public void CanSuppressFailuresByClearingExceptionLog()
        {
            var exceptionA = new InvalidOperationException();
            var exceptionB = new DivideByZeroException();

            var @case = Case("Returns");

            @case.Exceptions.ShouldBeEmpty();
            @case.Fail(exceptionA);
            @case.Fail(exceptionB);
            @case.ClearExceptions();
            @case.Exceptions.ShouldBeEmpty();
        }

        class SampleParentTestClass
        {
            public void TestMethodDefinedWithinParentClass()
            {
            }
        }

        class SampleChildTestClass : SampleParentTestClass
        {
            public void TestMethodDefinedWithinChildClass()
            {
            }
        }

        static Case Case(string methodName, params object[] parameters)
        {
            return new Case(typeof(CaseTests).GetInstanceMethod(methodName), parameters);
        }

        void Returns()
        {
        }

        void Throws()
        {
            throw new FailureException();
        }

        void Parameterized(int i, bool b, char ch, string s1, string s2, object obj, CaseTests complex)
        {
        }

        void String(string s)
        {
        }

        void Generic<T1, T2>(int i, T1 t1, T2 t2a, T2 t2b)
        {
        }
    }
}
