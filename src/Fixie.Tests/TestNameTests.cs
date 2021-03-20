namespace Fixie.Tests
{
    using Assertions;

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