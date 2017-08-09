namespace Fixie.Tests.Execution
{
    using Assertions;
    using Fixie.Execution;

    public class FilterTests
    {
        public void ShouldMatchAnythingByDefault()
        {
            var filter = new Filter();

            filter.IsSatisfiedBy(new MethodGroup("ClassName.MethodName"))
                .ShouldBeTrue();
        }

        public void ShouldMatchBySubstring()
        {
            var filter = new Filter();

            filter.ByPatterns("Abc", "Def");

            filter.IsSatisfiedBy(new MethodGroup("ClassName.AbcMethodName"))
                .ShouldBeTrue();

            filter.IsSatisfiedBy(new MethodGroup("ClassName.DefMethodName"))
                .ShouldBeTrue();

            filter.IsSatisfiedBy(new MethodGroup("ClassName.NotMatchingMethodName"))
                .ShouldBeFalse();
        }

        public void ShouldMatchByWildcard()
        {
            var filter = new Filter();

            filter.ByPatterns("Abc*Def");

            filter.IsSatisfiedBy(new MethodGroup("ClassName.AbcDef.MethodName"))
                .ShouldBeTrue();

            filter.IsSatisfiedBy(new MethodGroup("ClassName.AbcMiddleDef.MethodName"))
                .ShouldBeTrue();

            filter.IsSatisfiedBy(new MethodGroup("ClassName.NotMatchingMethodName"))
                .ShouldBeFalse();
        }
    }
}