namespace Fixie.Samples.Categories
{
    using System;
    using System.Linq;
    using System.Reflection;

    public class CustomOptions
    {
        public string[] Include { get; set; }
    }

    public class CustomConvention : Convention
    {
        public CustomConvention(CustomOptions customOptions)
        {
            var desiredCategories = customOptions.Include;
            var shouldRunAll = !desiredCategories.Any();

            Classes
                .InTheSameNamespaceAs(typeof(CustomConvention))
                .NameEndsWith("Tests");

            Methods
                .Where(method => method.IsVoid())
                .Where(method => shouldRunAll || MethodHasAnyDesiredCategory(method, desiredCategories));

            if (!shouldRunAll)
            {
                Console.WriteLine("Categories: " + string.Join(", ", desiredCategories));
                Console.WriteLine();
            }
        }

        static bool MethodHasAnyDesiredCategory(MethodInfo method, string[] desiredCategories)
        {
            return Categories(method).Any(testCategory => desiredCategories.Contains(testCategory.Name));
        }

        static CategoryAttribute[] Categories(MethodInfo method)
        {
            return method.GetCustomAttributes<CategoryAttribute>(true).ToArray();
        }
    }
}