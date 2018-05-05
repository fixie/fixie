namespace Fixie.Tests
{
    using Assertions;
    using static Utility;

    public class TestTests
    {
        public void CanRepresentMethodsDeclaredInChildClasses()
        {
            AssertTest(
                Test<ChildClass>("MethodDefinedWithinChildClass"),
                "Fixie.Tests.TestTests+ChildClass",
                "MethodDefinedWithinChildClass",
                "Fixie.Tests.TestTests+ChildClass.MethodDefinedWithinChildClass");
        }

        public void CanRepresentMethodsDeclaredInParentClasses()
        {
            AssertTest(
                Test<ParentClass>("MethodDefinedWithinParentClass"),
                "Fixie.Tests.TestTests+ParentClass",
                "MethodDefinedWithinParentClass",
                "Fixie.Tests.TestTests+ParentClass.MethodDefinedWithinParentClass");
        }

        public void CanRepresentParentMethodsInheritedByChildClasses()
        {
            AssertTest(
                Test<ChildClass>("MethodDefinedWithinParentClass"),
                "Fixie.Tests.TestTests+ChildClass",
                "MethodDefinedWithinParentClass",
                "Fixie.Tests.TestTests+ChildClass.MethodDefinedWithinParentClass");
        }

        public void CanParseFromFullNameStrings()
        {
            AssertTest(
                new Test("Fixie.Tests.TestTests+ChildClass.MethodDefinedWithinChildClass"),
                "Fixie.Tests.TestTests+ChildClass",
                "MethodDefinedWithinChildClass",
                "Fixie.Tests.TestTests+ChildClass.MethodDefinedWithinChildClass");

            AssertTest(
                new Test("Fixie.Tests.TestTests+ParentClass.MethodDefinedWithinParentClass"),
                "Fixie.Tests.TestTests+ParentClass",
                "MethodDefinedWithinParentClass",
                "Fixie.Tests.TestTests+ParentClass.MethodDefinedWithinParentClass");

            AssertTest(
                new Test("Fixie.Tests.TestTests+ChildClass.MethodDefinedWithinParentClass"),
                "Fixie.Tests.TestTests+ChildClass",
                "MethodDefinedWithinParentClass",
                "Fixie.Tests.TestTests+ChildClass.MethodDefinedWithinParentClass");
        }

        public void CanBeConstructedFromTrustedClassNameAndMethodName()
        {
            AssertTest(
                new Test(FullName<ChildClass>(), "MethodDefinedWithinChildClass"),
                "Fixie.Tests.TestTests+ChildClass",
                "MethodDefinedWithinChildClass",
                "Fixie.Tests.TestTests+ChildClass.MethodDefinedWithinChildClass");

            AssertTest(
                new Test(FullName<ParentClass>(), "MethodDefinedWithinParentClass"),
                "Fixie.Tests.TestTests+ParentClass",
                "MethodDefinedWithinParentClass",
                "Fixie.Tests.TestTests+ParentClass.MethodDefinedWithinParentClass");

            AssertTest(
                new Test(FullName<ChildClass>(), "MethodDefinedWithinParentClass"),
                "Fixie.Tests.TestTests+ChildClass",
                "MethodDefinedWithinParentClass",
                "Fixie.Tests.TestTests+ChildClass.MethodDefinedWithinParentClass");
        }

        static void AssertTest(Test actual, string expectedClass, string expectedMethod, string expectedName)
        {
            actual.Class.ShouldEqual(expectedClass);
            actual.Method.ShouldEqual(expectedMethod);
            actual.Name.ShouldEqual(expectedName);
        }

        static Test Test<TTestClass>(string method)
            => new Test(typeof(TTestClass).GetInstanceMethod(method));

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