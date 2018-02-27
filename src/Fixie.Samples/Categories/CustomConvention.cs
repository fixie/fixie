namespace Fixie.Samples.Categories
{
    using System;
    using System.Linq;
    using System.Reflection;

    public class CustomConvention : Convention
    {
        public CustomConvention(string[] include)
        {
            var desiredCategories = include;
            var shouldRunAll = !desiredCategories.Any();

            Classes
                .Where(x => x.IsInNamespace(GetType().Namespace))
                .Where(x => x.Name.EndsWith("Tests"));

            Methods
                .Where(x => shouldRunAll || MethodHasAnyDesiredCategory(x, desiredCategories));

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