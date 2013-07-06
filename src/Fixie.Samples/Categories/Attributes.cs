using System;

namespace Fixie.Samples.Categories
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public abstract class CategoryAttribute : Attribute
    {
        public string Name
        {
            get { return GetType().Name.Replace("Attribute", ""); }
        }
    }

    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public class CategoryAAttribute : CategoryAttribute { }

    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public class CategoryBAttribute : CategoryAttribute { }
}