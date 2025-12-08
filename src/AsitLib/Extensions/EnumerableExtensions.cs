using AsitLib.Numerics;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace AsitLib
{
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Attempts to get the first element.
        /// </summary>
        public static bool TryGetFirst<T>(this IEnumerable<T> source, [NotNullWhen(true)] out T? value)
            => TryGetFirst(source, v => true, out value);

        /// <summary>
        /// Attempts to get the first element matching a predicate.
        /// </summary>
        public static bool TryGetFirst<T>(this IEnumerable<T> source, Func<T, bool> predicate, [NotNullWhen(true)] out T? value)
        {
            foreach (T element in source)
            {
                if (!predicate(element)) continue;

                value = element!;
                return true;
            }

            value = default;
            return false;
        }

        /// <summary>
        /// Attempts to get the last element.
        /// </summary>
        public static bool TryGetLast<T>(this IEnumerable<T> source, [NotNullWhen(true)] out T? value)
            => TryGetLast<T>(source, v => true, out value);

        /// <summary>
        /// Attempts to get the last element matching a predicate.
        /// </summary>
        public static bool TryGetLast<T>(this IEnumerable<T> source, Func<T, bool> predicate, [NotNullWhen(true)] out T? value)
        {
            bool found = false;
            T? last = default;

            foreach (T element in source)
            {
                if (!predicate(element)) continue;

                last = element;
                found = true;
            }

            value = found ? last : default;
            return found;
        }

        /// <summary>
        /// Attempts to get the element at the specified index.
        /// </summary>
        public static bool TryGetAt<T>(this IEnumerable<T> source, int index, [NotNullWhen(true)] out T? value)
            => TryGetAt(source, index, v => true, out value);

        /// <summary>
        /// Attempts to get the first element at the specified index that matches a predicate.
        /// </summary>
        public static bool TryGetAt<T>(this IEnumerable<T> source, int index, Func<T, bool> predicate, [NotNullWhen(true)] out T? value)
        {
            if (index < 0)
            {
                value = default;
                return false;
            }

            int matchIndex = 0;

            foreach (T element in source)
            {
                if (!predicate(element)) continue;

                if (matchIndex == index)
                {
                    value = element!;
                    return true;
                }

                matchIndex++;
            }

            value = default;
            return false;
        }

        public static bool EndsWith<T>(this T[] array1, T[] array2)
        {
            if (array2.Length > array1.Length) throw new ArgumentException($"Lenght exeeds base array lenght.", nameof(array2));

            return array1[(array1.Length - array2.Length)..].SequenceEqual(array2);
        }

        public static T[] GetShallowCopy<T>(this T[] source)
        {
            T[] toret = new T[source.Length];
            Array.Copy(source, toret, source.Length);
            return source;
        }

        public static T[] SwitchIndexes<T>(this T[] source, int index1, int index2)
        {
            (source[index1], source[index2]) = (source[index2], source[index1]);
            return source;
        }

        public static T?[] SqueezeIndexes<T>(this T?[] source)
        {
            List<T?> squeezedList = new List<T?>();

            for (int i = 0; i < source.Length; i++)
                if (source[i] is not null) squeezedList.Add(source[i]);
            while (squeezedList.Count != source.Length) squeezedList.Add(default(T));

            return squeezedList.ToArray();
        }

        public static T[] ToSingleArray<T>(this T value) => [value]; // still needed in rare cases where [] doesn't work.

        /// <summary>
        /// Get the first index where the set conditions are met. If none are found a <see cref="Exception"/> is thrown.
        /// </summary>
        /// <typeparam name="T">The <paramref name="source"/> <see cref="Array"/> <see cref="Type"/>.</typeparam>
        /// <param name="source">The source array.</param>
        /// <param name="predicate"><see cref="Delegate"/></param>
        /// <returns>The index of the first item that meets the predicate.</returns>
        /// <exception cref="Exception"></exception>
        public static int GetFirstIndexWhere<T>(this IEnumerable<T> source, Func<T, bool> predicate)
        {
            int index = 0;
            foreach (T item in source)
                if (predicate.Invoke(item)) return index;
                else index++;
            throw new InvalidOperationException("No items match the given predicate.");
        }

        public static bool HasDuplicates<T>(this IEnumerable<T> source) => source.GroupBy(x => x).Any(g => g.Count() > 1);

        public static bool TryGetFirstIndexWhere<T>(this IEnumerable<T> source, Func<T, bool> predicate, [NotNullWhen(true)] out int? value)
        {
            try
            {
                value = source.GetFirstIndexWhere(predicate);
                return true;
            }
            catch
            {
                value = null;
                return false;
            }
        }

        public static IEnumerable<T> ElementsAt<T>(this IEnumerable<T> values, Range range)
        {
            NormalizedRange normalizedRange = NormalizedRange.CreateFromFactory(range, values);
            return values.Skip(normalizedRange.Start).Take(normalizedRange.End - normalizedRange.Start);
        }

        public static int[] GetIndexesWhere<T>(this IEnumerable<T> values, Func<T, bool> predicate)
        {
            List<int> indexes = new List<int>();
            int index = 0;
            foreach (T? item in values)
            {
                if (predicate.Invoke(item)) indexes.Add(index);
                index++;
            }
            return indexes.ToArray();
        }

        public static string ToJoinedString<T>(this IEnumerable<T> values, char joiner) => ToJoinedString(values, joiner.ToString());
        public static string ToJoinedString<T>(this IEnumerable<T> values, string? joiner = null)
        {
            joiner ??= string.Empty;
            if (values is null) return StringHelpers.NULL_STRING;
            return string.Join(joiner, values.Select(v => v?.ToString() ?? StringHelpers.NULL_STRING));
        }
    }
}
