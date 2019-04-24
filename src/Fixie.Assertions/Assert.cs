namespace Fixie.Assertions
{
    using System;
    using System.Collections;

    static class Assert
    {
        public static bool Equal<T>(T x, T y)
        {
            var type = typeof(T);

            if (IsReferenceType(type) || IsNullableValueType(type))
            {
                if (Equals(x, default(T)))
                    return Equals(y, default(T));

                if (Equals(y, default(T)))
                    return false;
            }

            if (x is IEquatable<T> equatable)
                return equatable.Equals(y);

            if (x is IEnumerable enumerableX && y is IEnumerable enumerableY)
                return EnumerableEqual(enumerableX, enumerableY);

            return Equals(x, y);
        }

        static bool IsNullableValueType(Type type)
        {
            return Nullable.GetUnderlyingType(type) != null;
        }

        static bool IsReferenceType(Type type)
        {
            return !type.IsValueType;
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

                if (!Equal(enumeratorX.Current, enumeratorY.Current))
                    return false;
            }
        }
    }
}