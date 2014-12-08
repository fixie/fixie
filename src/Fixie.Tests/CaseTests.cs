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
            Case("Returns").MethodGroup.ShouldEqual("Fixie.Tests.CaseTests.Returns");
            Case("Parameterized", 123, true, 'a', "s", null, this).MethodGroup.ShouldEqual("Fixie.Tests.CaseTests.Parameterized");
            Case("Generic", 123, true, "a", "b").MethodGroup.ShouldEqual("Fixie.Tests.CaseTests.Generic");
            Case("Generic", 123, true, 1, null).MethodGroup.ShouldEqual("Fixie.Tests.CaseTests.Generic");
            Case("Generic", 123, 1.23m, "a", null).MethodGroup.ShouldEqual("Fixie.Tests.CaseTests.Generic");
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

        void Generic<T1, T2>(int i, T1 t1, T2 t2a, T2 t2b)
        {
        }
    }
}
