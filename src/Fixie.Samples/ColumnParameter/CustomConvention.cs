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
                var columns = method.GetParameters();
                int columnCount = columns.Length;
                if (columnCount > 0)
                {
                    object[][] columnParameters = new object[columnCount][];
                    int[] columnParameterCount1s = new int[columnCount];
                    int[] columnParameterIndexes = new int[columnCount];

                    for (int i = 0; i < columnCount; i++)
                    {
                        columnParameters[i] = columns[i].GetCustomAttribute<ColumnAttribute>(true).Parameters;
                        columnParameterCount1s[i] = columnParameters[i].Length - 1;
                        columnParameterIndexes[i] = 0;
                    }

                    bool continueNextCombination;
                    do
                    {
                        object[] aCombination = new object[columnCount];
                        for (int i = 0; i < columnCount; i++)
                            aCombination[i] = columnParameters[i][columnParameterIndexes[i]];
                        yield return aCombination;

                        continueNextCombination = false;
                        for (int i = 0; i < columnCount; i++)
                        {
                            if (columnParameterIndexes[i] < columnParameterCount1s[i])
                            {
                                columnParameterIndexes[i]++;
                                continueNextCombination = true;
                                break;
                            }
                            else
                            {
                                columnParameterIndexes[i] = 0;
                            }
                        }
                    }
                    while (continueNextCombination);
                }
            }
        }
    }
}