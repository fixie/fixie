namespace Fixie.Tests.Runner
{
    using Assertions;
    using Fixie.Runner;

    public class MethodGroupTests
    {
        public void CanRepresentMethodDeclaredInChildClass()
        {
            var methodDeclaredInChildClass = typeof(ChildClass).GetInstanceMethod("MethodDefinedWithinChildClass");

            AssertMethodGroup(
                new MethodGroup(typeof(ChildClass), methodDeclaredInChildClass),
                "Fixie.Tests.Runner.MethodGroupTests+ChildClass",
                "MethodDefinedWithinChildClass",
                "Fixie.Tests.Runner.MethodGroupTests+ChildClass.MethodDefinedWithinChildClass");
        }

        public void CanRepresentMethodDeclaredInParentClass()
        {
            var methodDeclaredInParentClass = typeof(ParentClass).GetInstanceMethod("MethodDefinedWithinParentClass");

            AssertMethodGroup(
                new MethodGroup(typeof(ParentClass), methodDeclaredInParentClass),
                "Fixie.Tests.Runner.MethodGroupTests+ParentClass",
                "MethodDefinedWithinParentClass",
                "Fixie.Tests.Runner.MethodGroupTests+ParentClass.MethodDefinedWithinParentClass");
        }

        public void CanRepresentMethodInheritedByChildClass()
        {
            var parentMethodInheritedByChildClass = typeof(ChildClass).GetInstanceMethod("MethodDefinedWithinParentClass");

            AssertMethodGroup(
                new MethodGroup(typeof(ChildClass), parentMethodInheritedByChildClass),
                "Fixie.Tests.Runner.MethodGroupTests+ChildClass",
                "MethodDefinedWithinParentClass",
                "Fixie.Tests.Runner.MethodGroupTests+ChildClass.MethodDefinedWithinParentClass");
        }

        public void CanParseFromFullNameStrings()
        {
            AssertMethodGroup(
                new MethodGroup("Fixie.Tests.Runner.MethodGroupTests+ChildClass.MethodDefinedWithinChildClass"),
                "Fixie.Tests.Runner.MethodGroupTests+ChildClass",
                "MethodDefinedWithinChildClass",
                "Fixie.Tests.Runner.MethodGroupTests+ChildClass.MethodDefinedWithinChildClass");

            AssertMethodGroup(
                new MethodGroup("Fixie.Tests.Runner.MethodGroupTests+ParentClass.MethodDefinedWithinParentClass"),
                "Fixie.Tests.Runner.MethodGroupTests+ParentClass",
                "MethodDefinedWithinParentClass",
                "Fixie.Tests.Runner.MethodGroupTests+ParentClass.MethodDefinedWithinParentClass");

            AssertMethodGroup(
                new MethodGroup("Fixie.Tests.Runner.MethodGroupTests+ChildClass.MethodDefinedWithinParentClass"),
                "Fixie.Tests.Runner.MethodGroupTests+ChildClass",
                "MethodDefinedWithinParentClass",
                "Fixie.Tests.Runner.MethodGroupTests+ChildClass.MethodDefinedWithinParentClass");
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