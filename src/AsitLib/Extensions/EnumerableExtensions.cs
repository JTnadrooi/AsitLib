using AsitLib.Numerics;
using System.Diagnostics.CodeAnalysis;

namespace AsitLib
{
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Determines whether a span contains any duplicate elements.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of the span.</typeparam>
        /// <param name="source">The span to check for duplicates.</param>
        /// <returns><see langword="true"/> if the source span contains any duplicate elements; otherwise, <see langword="false"/>.</returns>
        public static bool HasDuplicates<TSource>(this Span<TSource> source)
            where TSource : notnull
        {
            return HasDuplicates((ReadOnlySpan<TSource>)source, static x => x);
        }

        /// <summary>
        /// Determines whether a span contains any duplicate elements based on a specified key selector function.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of the span.</typeparam>
        /// <typeparam name="TKey">The type of the key returned by keySelector.</typeparam>
        /// <param name="source">The span to check for duplicates.</param>
        /// <param name="keySelector">A function to extract the key for each element.</param>
        /// <returns><see langword="true"/> if the source span contains any duplicate elements based on the key; otherwise, <see langword="false"/>.</returns>
        public static bool HasDuplicates<TSource, TKey>(
            this Span<TSource> source,
            Func<TSource, TKey> keySelector)
            where TSource : notnull
        {
            return HasDuplicates((ReadOnlySpan<TSource>)source, keySelector, null);
        }

        /// <summary>
        /// Determines whether a span contains any duplicate elements based on a specified key selector function
        /// and using a specified equality comparer.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of the span.</typeparam>
        /// <typeparam name="TKey">The type of the key returned by keySelector.</typeparam>
        /// <param name="source">The span to check for duplicates.</param>
        /// <param name="keySelector">A function to extract the key for each element.</param>
        /// <param name="comparer">An equality comparer to compare keys.</param>
        /// <returns><see langword="true"/> if the source span contains any duplicate elements based on the key; otherwise, <see langword="false"/>.</returns>
        public static bool HasDuplicates<TSource, TKey>(
            this Span<TSource> source,
            Func<TSource, TKey> keySelector,
            IEqualityComparer<TKey>? comparer)
            where TSource : notnull
        {
            return HasDuplicates((ReadOnlySpan<TSource>)source, keySelector, comparer);
        }

        /// <summary>
        /// Determines whether a read-only span contains any duplicate elements.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of the span.</typeparam>
        /// <param name="source">The read-only span to check for duplicates.</param>
        /// <returns><see langword="true"/> if the source span contains any duplicate elements; otherwise, <see langword="false"/>.</returns>
        public static bool HasDuplicates<TSource>(this ReadOnlySpan<TSource> source)
            where TSource : notnull
        {
            return HasDuplicates(source, static x => x);
        }

        /// <summary>
        /// Determines whether a read-only span contains any duplicate elements based on a specified key selector function.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of the span.</typeparam>
        /// <typeparam name="TKey">The type of the key returned by keySelector.</typeparam>
        /// <param name="source">The read-only span to check for duplicates.</param>
        /// <param name="keySelector">A function to extract the key for each element.</param>
        /// <returns><see langword="true"/> if the source span contains any duplicate elements based on the key; otherwise, <see langword="false"/>.</returns>
        public static bool HasDuplicates<TSource, TKey>(
            this ReadOnlySpan<TSource> source,
            Func<TSource, TKey> keySelector)
            where TSource : notnull
        {
            return HasDuplicates(source, keySelector, null);
        }

        /// <summary>
        /// Determines whether a read-only span contains any duplicate elements based on a specified key selector function
        /// and using a specified equality comparer.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of the span.</typeparam>
        /// <typeparam name="TKey">The type of the key returned by keySelector.</typeparam>
        /// <param name="source">The read-only span to check for duplicates.</param>
        /// <param name="keySelector">A function to extract the key for each element.</param>
        /// <param name="comparer">An equality comparer to compare keys.</param>
        /// <returns><see langword="true"/> if the source span contains any duplicate elements based on the key; otherwise, <see langword="false"/>.</returns>
        public static bool HasDuplicates<TSource, TKey>(
            this ReadOnlySpan<TSource> source,
            Func<TSource, TKey> keySelector,
            IEqualityComparer<TKey>? comparer)
            where TSource : notnull
        {
            if (keySelector is null) throw new ArgumentNullException(nameof(keySelector));

            HashSet<TKey> seen = new(comparer ?? EqualityComparer<TKey>.Default);
            foreach (TSource item in source)
            {
                TKey key = keySelector.Invoke(item);
                if (!seen.Add(key))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Determines whether a sequence contains any duplicate elements.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <param name="source">The sequence to check for duplicates.</param>
        /// <returns><see langword="true"/> if the source sequence contains any duplicate elements; otherwise, <see langword="false"/>.</returns>
        public static bool HasDuplicates<TSource>(this IEnumerable<TSource> source)
        {
            return source.HasDuplicates(x => x);
        }

        /// <summary>
        /// Determines whether a sequence contains any duplicate elements based on a specified key selector function.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <typeparam name="TKey">The type of the key returned by keySelector.</typeparam>
        /// <param name="source">The sequence to check for duplicates.</param>
        /// <param name="keySelector">A function to extract the key for each element.</param>
        /// <returns><see langword="true"/> if the source sequence contains any duplicate elements based on the key; otherwise, <see langword="false"/>.</returns>
        public static bool HasDuplicates<TSource, TKey>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector)
        {
            return source.HasDuplicates(keySelector, null);
        }

        /// <summary>
        /// Determines whether a sequence contains any duplicate elements based on a specified key selector function
        /// and using a specified equality comparer.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <typeparam name="TKey">The type of the key returned by keySelector.</typeparam>
        /// <param name="source">The sequence to check for duplicates.</param>
        /// <param name="keySelector">A function to extract the key for each element.</param>
        /// <param name="comparer">An equality comparer to compare keys.</param>
        /// <returns><see langword="true"/> if the source sequence contains any duplicate elements based on the key; otherwise, <see langword="false"/>.</returns>
        public static bool HasDuplicates<TSource, TKey>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            IEqualityComparer<TKey>? comparer)
        {
            if (source is null) throw new ArgumentNullException(nameof(source));
            if (keySelector is null) throw new ArgumentNullException(nameof(keySelector));

            HashSet<TKey> seen = new(comparer ?? EqualityComparer<TKey>.Default);
            foreach (TSource item in source)
            {
                TKey key = keySelector.Invoke(item);
                if (!seen.Add(key))
                    return true;
            }
            return false;
        }

        public static int IndexOf<T>(this IEnumerable<T> source, T value)
        {
            int index = 0;
            EqualityComparer<T> comparer = EqualityComparer<T>.Default;
            foreach (T item in source)
            {
                if (comparer.Equals(item, value)) return index;
                index++;
            }

            throw new InvalidOperationException("Item not found.");
        }

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
                if (!predicate.Invoke(element)) continue;

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
                if (!predicate.Invoke(element)) continue;

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
                if (!predicate.Invoke(element)) continue;

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
            NormalizedRange normalizedRange = NormalizedRange.CreateFrom(range, values);
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

        public static string ToJoinedString<T>(this IEnumerable<T> values, char joiner)
            => ToJoinedString(values, joiner.ToString());

        public static string ToJoinedString<T>(this IEnumerable<T> values, string? joiner = null)
        {
            joiner ??= string.Empty;
            if (values is null) return StringHelpers.NULL_STRING;
            return string.Join(joiner, values.Select(v => v?.ToString() ?? StringHelpers.NULL_STRING));
        }

        public static string ToJoinedString<T>(this ReadOnlySpan<T> values, char joiner)
            => ToJoinedString(values, joiner.ToString());

        public static string ToJoinedString<T>(this ReadOnlySpan<T> values, string? joiner = null)
        {
            joiner ??= string.Empty;
            if (values.IsEmpty) return string.Empty;

            StringBuilder sb = new StringBuilder();

            T first = values[0];
            sb.Append(first?.ToString() ?? StringHelpers.NULL_STRING);

            for (int i = 1; i < values.Length; i++)
            {
                sb.Append(joiner);
                T value = values[i];
                sb.Append(value?.ToString() ?? StringHelpers.NULL_STRING);
            }

            return sb.ToString();
        }

        public static string ToJoinedString<T>(this Span<T> values, char joiner)
            => ((ReadOnlySpan<T>)values).ToJoinedString(joiner);

        public static string ToJoinedString<T>(this Span<T> values, string? joiner = null)
            => ((ReadOnlySpan<T>)values).ToJoinedString(joiner);
    }
}
