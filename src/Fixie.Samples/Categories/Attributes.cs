namespace Fixie.Samples.Categories
{
    using System;

    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public abstract class CategoryAttribute : Attribute
    {
        public string Name => GetType().Name.Replace("Attribute", "");
    }

    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public class CategoryAAttribute : CategoryAttribute { }

    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public class CategoryBAttribute : CategoryAttribute { }
}