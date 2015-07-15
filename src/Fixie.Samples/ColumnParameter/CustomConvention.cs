using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Fixie.Samples.ColumnParameter
{
    public class CustomConvention : Convention
    {
        public CustomConvention()
        {
            Classes
                .InTheSameNamespaceAs(typeof(CustomConvention))
                .NameEndsWith("Tests");

            Methods
                .Where(method => method.IsVoid());

            ClassExecution
                .CreateInstancePerClass()
                .SortCases((caseA, caseB) => String.Compare(caseA.Name, caseB.Name, StringComparison.Ordinal));

            Parameters
                .Add<ColumnAttributeParameterSource>();
        }

        class ColumnAttributeParameterSource : ParameterSource
        {
            public IEnumerable<object[]> GetParameters(MethodInfo method)
            {
                return CartesianProduct(Columns(method));
            }

            static IEnumerable<object[]> Columns(MethodInfo method)
            {
                return method
                    .GetParameters()
                    .Select(parameter =>
                        parameter.GetCustomAttributes<ColumnAttribute>(true).Single().Parameters);
            }

            static IEnumerable<object[]> CartesianProduct(IEnumerable<object[]> sequences)
            {
                //See http://blogs.msdn.com/b/ericlippert/archive/2010/06/28/computing-a-cartesian-product-with-linq.aspx

                IEnumerable<object[]> emptyProduct = new[] { new object[] { } };
                return sequences.Aggregate(
                    emptyProduct,
                    (accumulator, sequence) =>
                        from accseq in accumulator
                        from item in sequence
                        select accseq.Concat(new[] { item }).ToArray());
            }
        }
    }
}