using System;
using System.Collections;
using System.Collections.Generic;
using Should.Core.Exceptions;

namespace Should.Core.Assertions
{
    public class Assert
    {
        public static void Contains<T>(T expected,
                                       IEnumerable<T> collection)
        {
            Contains(expected, collection, GetEqualityComparer<T>());
        }

        public static void Contains<T>(T expected, IEnumerable<T> collection, IEqualityComparer<T> comparer)
        {
            foreach (T item in collection)
                if (comparer.Equals(expected, item))
                    return;

            throw new ContainsException(expected);
        }

        public static void Empty(IEnumerable collection)
        {
            if (collection == null) throw new ArgumentNullException("collection", "cannot be null");

#pragma warning disable 168
            foreach (object @object in collection)
                throw new EmptyException();
#pragma warning restore 168
        }

        public static void Equal<T>(T expected,
                                    T actual)
        {
            Equal(expected, actual, GetEqualityComparer<T>());
        }

        public static void Equal<T>(T expected,
                                    T actual,
                                    string userMessage)
        {
            Equal(expected, actual, GetEqualityComparer<T>(), userMessage);
        }

        public static void Equal<T>(T expected,
                                    T actual,
                                    IEqualityComparer<T> comparer)
        {
            if (!comparer.Equals(expected, actual))
                throw new EqualException(expected, actual);
        }

        public static void Equal<T>(T expected,
                                    T actual,
                                    IEqualityComparer<T> comparer,
                                    string userMessage)
        {
            if (!comparer.Equals(expected, actual))
                throw new EqualException(expected, actual, userMessage);
        }

        public static void False(bool condition)
        {
            False(condition, null);
        }

        public static void False(bool condition,
                                 string userMessage)
        {
            if (condition)
                throw new FalseException(userMessage);
        }

        static IEqualityComparer<T> GetEqualityComparer<T>()
        {
            return new AssertEqualityComparer<T>();
        }

        static IComparer<T> GetComparer<T>()
        {
            return new AssertComparer<T>();
        }

        public static void GreaterThan<T>(T left, T right)
        {
            GreaterThan(left, right, GetComparer<T>());
        }

        public static void GreaterThan<T>(T left, T right, IComparer<T> comparer)
        {
            if (comparer.Compare(left, right) <= 0)
                throw new GreaterThanException(left, right);
        }

        public static void GreaterThanOrEqual<T>(T left, T right)
        {
            GreaterThanOrEqual(left, right, GetComparer<T>());
        }

        public static void GreaterThanOrEqual<T>(T left, T right, IComparer<T> comparer)
        {
            if (comparer.Compare(left, right) < 0)
                throw new GreaterThanOrEqualException(left, right);
        }

        public static T IsType<T>(object @object)
        {
            IsType(typeof(T), @object);
            return (T)@object;
        }

        public static void IsType(Type expectedType,
                                  object @object)
        {
            if (@object == null || !expectedType.Equals(@object.GetType()))
                throw new IsTypeException(expectedType, @object);
        }

        public static void NotEqual<T>(T expected,
                                       T actual)
        {
            NotEqual(expected, actual, GetEqualityComparer<T>());
        }

        public static void NotEqual<T>(T expected,
                                       T actual,
                                       IEqualityComparer<T> comparer)
        {
            if (comparer.Equals(expected, actual))
                throw new NotEqualException(expected, actual);
        }

        public static void NotNull(object @object)
        {
            if (@object == null)
                throw new NotNullException();
        }

        public static void Null(object @object)
        {
            if (@object != null)
                throw new NullException(@object);
        }

        public static void True(bool condition)
        {
            True(condition, null);
        }

        public static void True(bool condition,
                                string userMessage)
        {
            if (!condition)
                throw new TrueException(userMessage);
        }
    }
}