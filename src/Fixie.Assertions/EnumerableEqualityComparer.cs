namespace Fixie.Assertions
{
    using System;
    using System.Collections;

    class EnumerableEqualityComparer
    {
        public bool EnumerableEqual(IEnumerable x, IEnumerable y)
        {
            var enumeratorX = x.GetEnumerator();
            var enumeratorY = y.GetEnumerator();

            while (true)
            {
                bool hasNextX = enumeratorX.MoveNext();
                bool hasNextY = enumeratorY.MoveNext();

                if (!hasNextX || !hasNextY)
                    return hasNextX == hasNextY;

                if (enumeratorX.Current != null || enumeratorY.Current != null)
                {
                    if (enumeratorX.Current != null && enumeratorY.Current == null)
                        return false;

                    if (enumeratorX.Current == null)
                        return false;

                    var xType = enumeratorX.Current.GetType();
                    var yType = enumeratorY.Current.GetType();

                    if (xType.IsAssignableFrom(yType))
                    {
                        if (!ItemsEqual(enumeratorX.Current, enumeratorY.Current, xType))
                            return false;
                    }
                    else if (yType.IsAssignableFrom(xType))
                    {
                        if (!ItemsEqual(enumeratorY.Current, enumeratorX.Current, yType))
                            return false;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }

        static bool ItemsEqual(object a, object b, Type baseType)
        {
            var assertComparerType = typeof(AssertEqualityComparer);
            var equal = assertComparerType.GetMethod("Equal");
            var specificEqual = equal.MakeGenericMethod(baseType);
            return (bool)specificEqual.Invoke(null, new[] { a, b });
        }
    }
}