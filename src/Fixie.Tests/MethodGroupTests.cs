namespace Fixie.Tests
{
    using Should;

    public class MethodGroupTests
    {
        public void CanConstructMethodGroupFromMethodInfoWithRespectToTheReflectedType()
        {
            var methodDeclaredInChildClass = typeof(ChildClass).GetInstanceMethod("MethodDefinedWithinChildClass");
            var methodDeclaredInParentClass = typeof(ParentClass).GetInstanceMethod("MethodDefinedWithinParentClass");
            var parentMethodInheritedByChildClass = typeof(ChildClass).GetInstanceMethod("MethodDefinedWithinParentClass");

            AssertMethodGroup(
                new MethodGroup(methodDeclaredInChildClass),
                "Fixie.Tests.MethodGroupTests+ChildClass",
                "MethodDefinedWithinChildClass",
                "Fixie.Tests.MethodGroupTests+ChildClass.MethodDefinedWithinChildClass");

            AssertMethodGroup(
                new MethodGroup(methodDeclaredInParentClass),
                "Fixie.Tests.MethodGroupTests+ParentClass",
                "MethodDefinedWithinParentClass",
                "Fixie.Tests.MethodGroupTests+ParentClass.MethodDefinedWithinParentClass");

            AssertMethodGroup(
                new MethodGroup(parentMethodInheritedByChildClass),
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