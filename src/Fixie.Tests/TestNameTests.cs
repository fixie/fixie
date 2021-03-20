namespace Fixie.Tests
{
    using Assertions;
    using static Utility;

    public class TestNameTests
    {
        public void CanRepresentMethodsDeclaredInChildClasses()
        {
            AssertTest(
                Test<ChildClass>("MethodDefinedWithinChildClass"),
                "Fixie.Tests.TestNameTests+ChildClass",
                "MethodDefinedWithinChildClass",
                "Fixie.Tests.TestNameTests+ChildClass.MethodDefinedWithinChildClass");
        }

        public void CanRepresentMethodsDeclaredInParentClasses()
        {
            AssertTest(
                Test<ParentClass>("MethodDefinedWithinParentClass"),
                "Fixie.Tests.TestNameTests+ParentClass",
                "MethodDefinedWithinParentClass",
                "Fixie.Tests.TestNameTests+ParentClass.MethodDefinedWithinParentClass");
        }

        public void CanRepresentParentMethodsInheritedByChildClasses()
        {
            AssertTest(
                Test<ChildClass>("MethodDefinedWithinParentClass"),
                "Fixie.Tests.TestNameTests+ChildClass",
                "MethodDefinedWithinParentClass",
                "Fixie.Tests.TestNameTests+ChildClass.MethodDefinedWithinParentClass");
        }

        public void CanParseFromFullNameStrings()
        {
            AssertTest(
                new TestName("Fixie.Tests.TestNameTests+ChildClass.MethodDefinedWithinChildClass"),
                "Fixie.Tests.TestNameTests+ChildClass",
                "MethodDefinedWithinChildClass",
                "Fixie.Tests.TestNameTests+ChildClass.MethodDefinedWithinChildClass");

            AssertTest(
                new TestName("Fixie.Tests.TestNameTests+ParentClass.MethodDefinedWithinParentClass"),
                "Fixie.Tests.TestNameTests+ParentClass",
                "MethodDefinedWithinParentClass",
                "Fixie.Tests.TestNameTests+ParentClass.MethodDefinedWithinParentClass");

            AssertTest(
                new TestName("Fixie.Tests.TestNameTests+ChildClass.MethodDefinedWithinParentClass"),
                "Fixie.Tests.TestNameTests+ChildClass",
                "MethodDefinedWithinParentClass",
                "Fixie.Tests.TestNameTests+ChildClass.MethodDefinedWithinParentClass");
        }

        public void CanBeConstructedFromTrustedClassNameAndMethodName()
        {
            AssertTest(
                new TestName(FullName<ChildClass>(), "MethodDefinedWithinChildClass"),
                "Fixie.Tests.TestNameTests+ChildClass",
                "MethodDefinedWithinChildClass",
                "Fixie.Tests.TestNameTests+ChildClass.MethodDefinedWithinChildClass");

            AssertTest(
                new TestName(FullName<ParentClass>(), "MethodDefinedWithinParentClass"),
                "Fixie.Tests.TestNameTests+ParentClass",
                "MethodDefinedWithinParentClass",
                "Fixie.Tests.TestNameTests+ParentClass.MethodDefinedWithinParentClass");

            AssertTest(
                new TestName(FullName<ChildClass>(), "MethodDefinedWithinParentClass"),
                "Fixie.Tests.TestNameTests+ChildClass",
                "MethodDefinedWithinParentClass",
                "Fixie.Tests.TestNameTests+ChildClass.MethodDefinedWithinParentClass");
        }

        static void AssertTest(TestName actual, string expectedClass, string expectedMethod, string expectedName)
        {
            actual.Class.ShouldBe(expectedClass);
            actual.Method.ShouldBe(expectedMethod);
            actual.FullName.ShouldBe(expectedName);
        }

        static TestName Test<TTestClass>(string method)
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