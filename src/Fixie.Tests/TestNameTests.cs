namespace Fixie.Tests
{
    using Assertions;

    public class TestNameTests
    {
        public void CanRepresentMethodsDeclaredInChildClasses()
        {
            AssertTestName(
                TestName<ChildClass>("MethodDefinedWithinChildClass"),
                "Fixie.Tests.TestNameTests+ChildClass",
                "MethodDefinedWithinChildClass",
                "Fixie.Tests.TestNameTests+ChildClass.MethodDefinedWithinChildClass");
        }

        public void CanRepresentMethodsDeclaredInParentClasses()
        {
            AssertTestName(
                TestName<ParentClass>("MethodDefinedWithinParentClass"),
                "Fixie.Tests.TestNameTests+ParentClass",
                "MethodDefinedWithinParentClass",
                "Fixie.Tests.TestNameTests+ParentClass.MethodDefinedWithinParentClass");
        }

        public void CanRepresentParentMethodsInheritedByChildClasses()
        {
            AssertTestName(
                TestName<ChildClass>("MethodDefinedWithinParentClass"),
                "Fixie.Tests.TestNameTests+ChildClass",
                "MethodDefinedWithinParentClass",
                "Fixie.Tests.TestNameTests+ChildClass.MethodDefinedWithinParentClass");
        }

        public void CanParseFromFullNameStrings()
        {
            AssertTestName(
                new TestName("Fixie.Tests.TestNameTests+ChildClass.MethodDefinedWithinChildClass"),
                "Fixie.Tests.TestNameTests+ChildClass",
                "MethodDefinedWithinChildClass",
                "Fixie.Tests.TestNameTests+ChildClass.MethodDefinedWithinChildClass");

            AssertTestName(
                new TestName("Fixie.Tests.TestNameTests+ParentClass.MethodDefinedWithinParentClass"),
                "Fixie.Tests.TestNameTests+ParentClass",
                "MethodDefinedWithinParentClass",
                "Fixie.Tests.TestNameTests+ParentClass.MethodDefinedWithinParentClass");

            AssertTestName(
                new TestName("Fixie.Tests.TestNameTests+ChildClass.MethodDefinedWithinParentClass"),
                "Fixie.Tests.TestNameTests+ChildClass",
                "MethodDefinedWithinParentClass",
                "Fixie.Tests.TestNameTests+ChildClass.MethodDefinedWithinParentClass");
        }

        static void AssertTestName(TestName actual, string expectedClass, string expectedMethod, string expectedFullName)
        {
            actual.Class.ShouldEqual(expectedClass);
            actual.Method.ShouldEqual(expectedMethod);
            actual.FullName.ShouldEqual(expectedFullName);
        }

        static TestName TestName<TTestClass>(string method)
            => new TestName(typeof(TTestClass).GetInstanceMethod(method));

        class ParentClass
        {
            public void MethodDefinedWithinParentClass()
            {
            }
        }

        class ChildClass : ParentClass
        {
            public void MethodDefinedWithinChildClass()
            {
            }
        }
    }
}