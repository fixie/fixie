using System;
using System.Linq;
using System.Reflection;
using Fixie.Conventions;

namespace Fixie.Samples.Categories
{
    public class CustomConvention : Convention
    {
        public CustomConvention(RunContext runContext)
        {
            var desiredCategories = runContext.Options["include"].ToArray();
            var shouldRunAll = !desiredCategories.Any();

            Classes
                .Where(type => type.IsInNamespace(GetType().Namespace))
                .NameEndsWith("Tests");

            Cases = new MethodFilter()
                .Where(method => method.Void())
                .ZeroParameters()
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