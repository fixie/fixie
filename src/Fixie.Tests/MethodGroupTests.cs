namespace Fixie.Tests
{
    using Assertions;

    public class MethodGroupTests
    {
        public void CanRepresentMethodsDeclaredInChildClasses()
        {
            AssertMethodGroup(
                MethodGroup<ChildClass>("MethodDefinedWithinChildClass"),
                "Fixie.Tests.MethodGroupTests+ChildClass",
                "MethodDefinedWithinChildClass",
                "Fixie.Tests.MethodGroupTests+ChildClass.MethodDefinedWithinChildClass");
        }

        public void CanRepresentMethodsDeclaredInParentClasses()
        {
            AssertMethodGroup(
                MethodGroup<ParentClass>("MethodDefinedWithinParentClass"),
                "Fixie.Tests.MethodGroupTests+ParentClass",
                "MethodDefinedWithinParentClass",
                "Fixie.Tests.MethodGroupTests+ParentClass.MethodDefinedWithinParentClass");
        }

        public void CanRepresentParentMethodsInheritedByChildClasses()
        {
            AssertMethodGroup(
                MethodGroup<ChildClass>("MethodDefinedWithinParentClass"),
                "Fixie.Tests.MethodGroupTests+ChildClass",
                "MethodDefinedWithinParentClass",
                "Fixie.Tests.MethodGroupTests+ChildClass.MethodDefinedWithinParentClass");
        }

        public void CanParseFromFullNameStrings()
        {
            AssertMethodGroup(
                new MethodGroup("Fixie.Tests.MethodGroupTests+ChildClass.MethodDefinedWithinChildClass"),
                "Fixie.Tests.MethodGroupTests+ChildClass",
                "MethodDefinedWithinChildClass",
                "Fixie.Tests.MethodGroupTests+ChildClass.MethodDefinedWithinChildClass");

            AssertMethodGroup(
                new MethodGroup("Fixie.Tests.MethodGroupTests+ParentClass.MethodDefinedWithinParentClass"),
                "Fixie.Tests.MethodGroupTests+ParentClass",
                "MethodDefinedWithinParentClass",
                "Fixie.Tests.MethodGroupTests+ParentClass.MethodDefinedWithinParentClass");

            AssertMethodGroup(
                new MethodGroup("Fixie.Tests.MethodGroupTests+ChildClass.MethodDefinedWithinParentClass"),
                "Fixie.Tests.MethodGroupTests+ChildClass",
                "MethodDefinedWithinParentClass",
                "Fixie.Tests.MethodGroupTests+ChildClass.MethodDefinedWithinParentClass");
        }

        static void AssertMethodGroup(MethodGroup actual, string expectedClass, string expectedMethod, string expectedFullName)
        {
            actual.Class.ShouldEqual(expectedClass);
            actual.Method.ShouldEqual(expectedMethod);
            actual.FullName.ShouldEqual(expectedFullName);
        }

        static MethodGroup MethodGroup<TTestClass>(string method)
            => new MethodGroup(typeof(TTestClass).GetInstanceMethod(method));

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