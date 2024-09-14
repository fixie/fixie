namespace Fixie.Tests;
using Fixie.Assertions;
public class TestNameTests
{
    public void CanRepresentMethodsDeclaredInChildClasses()
    {
        Test<ChildClass>("MethodDefinedWithinChildClass")
            .ShouldBe("Fixie.Tests.TestNameTests+ChildClass.MethodDefinedWithinChildClass");
    }

    public void CanRepresentMethodsDeclaredInParentClasses()
    {
        Test<ParentClass>("MethodDefinedWithinParentClass")
            .ShouldBe("Fixie.Tests.TestNameTests+ParentClass.MethodDefinedWithinParentClass");
    }

    public void CanRepresentParentMethodsInheritedByChildClasses()
    {
        Test<ChildClass>("MethodDefinedWithinParentClass")
            .ShouldBe("Fixie.Tests.TestNameTests+ChildClass.MethodDefinedWithinParentClass");
    }

    static string Test<TTestClass>(string method)
        => typeof(TTestClass).GetInstanceMethod(method).TestName();

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