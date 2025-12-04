using AsitLib.Numerics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace AsitLib
{
    public static class EnumerableExtensions
    {
        public static bool EndsWith<T>(this T[] source, T[] value)
        {
            if (source is null) throw new ArgumentNullException(nameof(source));
            if (value is null) throw new ArgumentNullException(nameof(value));
            if (value.Length > source.Length) throw new ArgumentException(nameof(value) + " lenght exeeds base array lenght.");

            return source[(source.Length - value.Length)..].SequenceEqual(value);
        }

        public static T[] Copy<T>(this T[] source)
        {
            T[] toret = new T[source.Length];
            Array.Copy(source, toret, source.Length);
            return source;
        }

        public static T[] SwitchIndexes<T>(this T[] source, int index1, int index2)
        {
            T item1 = source[index1];
            T item2 = source[index2];

            source[index1] = item2;
            source[index2] = item1;

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

        public static T?[] SetSize<T>(this T[] source, int newSize)
        {
            if (source.Length == newSize) return source;
            else if (source.Length > newSize)
            {
                source = source[..newSize];
                return source;
            }
            else
            {
                List<T?> values = new List<T?>(source);
                int diff = (int)MathFI.Diff(source.Length, newSize);
                for (int i = 0; i < diff; i++) values.Add(default(T));
                return values.ToArray();
            }
        }

        public static T[] Shift<T>(this T[] source, int amount)
        {
            T[] shiftedArray = new T[source.Length];
            int newIndex;

            for (int i = 0; i < source.Length; i++)
            {
                newIndex = i + amount;
                if (newIndex >= 0 && newIndex < source.Length) shiftedArray[newIndex] = source[i];
            }

            return shiftedArray;
        }

        public static T[] ToSingleArray<T>(this T value) => [value]; // still needed in rare cases where [] doesn't work.
        public static T[,] CreateRectangularArray<T>(this T[][] arrays)
        {
            // TODO: Validation and special-casing for arrays.Count == 0
            int lenght0 = arrays.GetLength(0);
            int minorLength = arrays[0].Length;
            T[,] ret = new T[lenght0, minorLength];
            for (int i = 0; i < lenght0; i++)
            {
                T[] array = arrays[i];
                if (array.Length != minorLength)
                {
                    throw new ArgumentException
                        ("All arrays must be the same length");
                }
                for (int j = 0; j < minorLength; j++)
                {
                    ret[i, j] = array[j];
                }
            }
            return ret;
        }

        public static object?[] GetItems(this ITuple tuple)
        {
            List<object?> toret2 = new List<object?>();
            for (int i = 0; i < tuple.Length; i++)
                toret2.Add(tuple[i]);
            return toret2.ToArray();
        }

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

        public static bool TryGetFirstIndexWhere<T>(this IEnumerable<T> source, Func<T, bool> predicate, out int value)
        {
            try
            {
                value = source.GetFirstIndexWhere(predicate);
                return true;
            }
            catch
            {
                value = default;
                return false;
            }
        }

        public static IEnumerable<T> ElementsAt<T>(this IEnumerable<T> values, Range range) => values.ToArray()[range];
        public static int[] GetIndexes<T>(this IEnumerable<T> values, Func<T, bool> validator)
            => Enumerable.Range(0, values.Count()).Where(i => validator.Invoke(values.ToArray()[i])).ToArray();
        public static Range[] ToBetweenRanges(this IEnumerable<int> ints)
        {
            if (ints.Any(i => i < 0) || ints.Count() != ints.Distinct().Count()) throw new ArgumentException("Invalid int array.");
            List<Range> toret = new List<Range>();
            int last = 0;
            foreach (int i in ints)
            {
                toret.Add(new Range(last, i));
                last = i;
            }
            return toret.ToArray()[1..];
        }

        public static IEnumerable<T> ConcatToStart<T>(this IEnumerable<T> values, T value)
        {
            T[] newValues = new T[values.Count() + 1];
            newValues[0] = value;
            Array.Copy(values.ToArray(), 0, newValues, 1, values.Count());
            return newValues;
        }

        public static string ToJoinedString<T>(this IEnumerable<T?>? values, char joiner)
        {
            if (values is null) return StringHelpers.NULL_STRING;
            return string.Join(joiner, values.Select(v => v?.ToString() ?? StringHelpers.NULL_STRING));
        }

        public static string ToJoinedString<T>(this IEnumerable<T?>? values, string? joiner = null)
        {
            joiner ??= string.Empty;
            if (values is null) return StringHelpers.NULL_STRING;
            return string.Join(joiner, values.Select(v => v?.ToString() ?? StringHelpers.NULL_STRING));
        }

        public static string To2DJoinedString<T>(this T[][] values, string joiner = "")
        {
            if (values is null) return "Null";
            string result = string.Empty;
            int maxI = values.Length;
            int maxJ = values[0].Length;
            for (int i = 0; i < maxI; i++)
            {
                result += ",{";
                for (int j = 0; j < maxJ; j++)
                {
                    result += $"{values[i][j]},";
                }

                result += "}";
            }
            return result.Replace(",}", "}")[1..];
        }
    }
}
