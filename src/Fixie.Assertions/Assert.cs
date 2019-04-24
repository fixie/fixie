namespace Fixie.Assertions
{
    using System;
    using System.Collections;

    static class Assert
    {
        public static bool Equal<T>(T x, T y)
        {
            var type = typeof(T);

            // Null?
            if (!type.IsValueType || (type.IsGenericType && type.GetGenericTypeDefinition().IsAssignableFrom(typeof(Nullable<>))))
            {
                if (object.Equals(x, default(T)))
                    return object.Equals(y, default(T));

                if (object.Equals(y, default(T)))
                    return false;
            }

            //x implements IEquatable<T> and is assignable from y?
            var xIsAssignableFromY = x.GetType().IsInstanceOfType(y);
            if (xIsAssignableFromY && x is IEquatable<T> equatable1)
                return equatable1.Equals(y);

            //y implements IEquatable<T> and is assignable from x?
            var yIsAssignableFromX = y.GetType().IsInstanceOfType(x);
            if (yIsAssignableFromX && y is IEquatable<T> equatable2)
                return equatable2.Equals(x);

            // Enumerable?
            if (x is IEnumerable enumerableX && y is IEnumerable enumerableY)
                return EnumerableEqual(enumerableX, enumerableY);

            // Last case, rely on object.Equals
            return object.Equals(x, y);
        }

        static bool EnumerableEqual(IEnumerable x, IEnumerable y)
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
            var equal = typeof(Assert).GetMethod("Equal");
            var specificEqual = equal.MakeGenericMethod(baseType);
            return (bool)specificEqual.Invoke(null, new[] { a, b });
        }
    }
}