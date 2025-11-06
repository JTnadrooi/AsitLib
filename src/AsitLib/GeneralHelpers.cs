using AsitLib.Numerics;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
#nullable enable

namespace AsitLib
{
    public static class StringHelpers
    {
        public enum BetweenMethod
        {
            FirstFirst,
            FirstLast,
            LastLast,
        }
        public static Dictionary<string, string> DefaultEscapes => new Dictionary<string, string>
        {
            {"\\s", " "},
            {"\\n", "\n"},
            {"\\t", "\t"},
            {"\\dq", "\""},
            {"\\q", "\'"},
        };
        public static ReadOnlyCollection<char> Alphabet { get; } = new char[] {'a', 'b',
            'c', 'd',
            'e', 'f',
            'g', 'h',
            'i', 'j',
            'k', 'l',
            'm', 'n',
            'o', 'p',
            'q', 'r',
            's', 't',
            'u', 'v',
            'w', 'x',
            'y', 'z' }.AsReadOnly();

        public static string Reverse(this string str) => string.Join(string.Empty, str.Reverse());

        public static Stream ToStream(this string str)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(str);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        public static string? FirstLine(this string str)
        {
            using StringReader reader = new StringReader(str);
            return reader.ReadLine();
        }

        public static string ToJoinedString<T>(this IEnumerable<T>? values)
        {
            if (values == null) return "Null";
            return string.Join("", values);
        }

        public static string ToJoinedString<T>(this IEnumerable<T>? values, char joiner)
        {
            if (values == null) return "Null";
            return string.Join(joiner, values);
        }

        public static string ToJoinedString<T>(this IEnumerable<T>? values, string joiner)
        {
            if (values == null) return "Null";
            return string.Join(joiner, values);
        }

        public static string ToJoinedString<T>(this IEnumerable<T>? values, char joiner, int lenght, int maxSafe = 50)
        {
            if (values == null) return "Null";
            if (lenght > maxSafe) return "MaxSafeOverflow {max_safe: " + maxSafe + " > count" + lenght + "}";
            return string.Join(joiner, values);
        }

        public static string ToJoinedString<T>(this IEnumerable<T>? values, string joiner, int lenght, int maxSafe = 50)
        {
            if (values == null) return "Null";
            if (lenght > maxSafe) return "MaxSafeOverflow {max_safe: " + maxSafe + " > count" + lenght + "}";
            return string.Join(joiner, values);
        }

        public static string To2DJoinedString<T>(this T[][] values, string joiner = "")
        {
            if (values == null) return "Null";
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

        public static bool Contains(this string input, string str, int maxcount) => Regex.Matches(input, str).Count <= maxcount;
        public static bool Contains(this string input, char c, int maxcount) => input.Count(cc => c == cc) <= maxcount;

        public static string Between(this string str, string FirstString, string LastString, BetweenMethod method = BetweenMethod.FirstFirst)
        {
            return method switch
            {
                BetweenMethod.FirstFirst => str[(str.IndexOf(FirstString) + FirstString.Length)..str.IndexOf(LastString)],
                BetweenMethod.FirstLast => str[(str.IndexOf(FirstString) + FirstString.Length)..str.LastIndexOf(LastString)],
                BetweenMethod.LastLast => str[(str.LastIndexOf(FirstString) + FirstString.Length)..str.LastIndexOf(LastString)],
                _ => throw new Exception(str),
            };
        }

        public static string[] Betweens(this string str, string FirstString, string LastString)
        {
            List<string> toret = new List<string>();
            while (true)
            {
                if (!str.Contains(FirstString) || !str.Contains(LastString) || str.IndexOf(FirstString) >= str.IndexOf(LastString)) break;

                string betw = str.Between(FirstString, LastString);
                toret.Add(betw); //add
                str = str.ReplaceFirst(FirstString + betw + LastString, string.Empty); //remove
            }
            return toret.ToArray();
        }

        public static string ReplaceFirst(this string str, string oldStr, string newString)
        {
            int pos = str.IndexOf(oldStr);
            if (pos < 0) return str;
            return str[..pos] + newString + str[(pos + oldStr.Length)..];
        }

        public static int SafeIntParse(this string i) => int.TryParse(i, out int v) ? v : -1;
        public static int? SafeNullIntParse(this string i) => int.TryParse(i, out int v) ? v : (int?)null;
        public static bool? SafeNullBoolParse(this string i)
        {
            return i.Equals("true", StringComparison.OrdinalIgnoreCase) ? true :
                   i.Equals("false", StringComparison.OrdinalIgnoreCase) ? false :
                   (bool?)null;
        }
    }

    public static class ArrayHelpers
    {
        public static bool EndsWith<T>(this T[] source, T[] value)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (value == null) throw new ArgumentNullException(nameof(value));
            if (value.Length > source.Length) throw new ArgumentException(nameof(value) + " lenght exeeds base array lenght.");

            return source[(source.Length - value.Length)..].SequenceEqual(value);
        }

        public static T[] Copy<T>(this T[] source)
        {
            T[] toret = new T[source.Length];
            Array.Copy(source, toret, source.Length);
            return source;
        }

        public static void SwitchIndexes<T>(ref T[] source, int index1, int index2)
        {
            T item1 = source[index1];
            T item2 = source[index2];

            source[index1] = item2;
            source[index2] = item1;
        }

        public static void SqueezeIndexes<T>(ref T?[] source)
        {
            List<T?> squeezedList = new List<T?>();

            for (int i = 0; i < source.Length; i++)
                if (source[i] != null) squeezedList.Add(source[i]);
            while (squeezedList.Count != source.Length) squeezedList.Add(default(T));

            source = squeezedList.ToArray();
        }

        public static void ExtrapolateValues<T>(ref T[] source)
        {
            int nullIndex = source.GetFirstIndexWhere(t => t == null);
            Console.WriteLine(nullIndex);
            while (source.Any(t => t == null))
            {
                AddValuesTo(ref source, source);
            }
        }

        public static void SetSize<T>(ref T?[] source, int newSize)
        {
            if (source.Length == newSize) return;
            else if (source.Length > newSize)
            {
                source = source[..newSize];
                return;
            }
            else
            {
                List<T?> values = new List<T?>(source);
                int diff = (int)MathFI.Diff(source.Length, newSize);
                for (int i = 0; i < diff; i++) values.Add(default(T));
                source = values.ToArray();
            }
        }

        public static void AddValuesTo<T>(ref T[] source, params T[] values)
        {
            int startIndex = source.GetFirstIndexWhere(t => t == null);
            for (int i = 0; i < values.Length; i++)
                if (source.Length > i + startIndex && source[startIndex + i] == null) source[startIndex + i] = values[i];
        }

        public static void Shift<T>(ref T[] array, int amount)
        {
            T[] shiftedArray = new T[array.Length];
            int newIndex;

            for (int i = 0; i < array.Length; i++)
            {
                newIndex = i + amount;
                if (newIndex >= 0 && newIndex < array.Length) shiftedArray[newIndex] = array[i];
            }

            array = shiftedArray;
        }

        public static void ShiftIndexes<T>(ref T[] array, int oldIndex, int newIndex)
        {
            if (oldIndex == newIndex) return;
            T atIndex = array[oldIndex];
            if (newIndex < oldIndex) Array.Copy(array, newIndex, array, newIndex + 1, oldIndex - newIndex);
            else Array.Copy(array, oldIndex + 1, array, oldIndex, newIndex - oldIndex);
            array[newIndex] = atIndex;
        }

        public static void PushIndex<T>(ref T[] source, int index)
        {
            ShiftIndexes(ref source, index, 0);
        }

        public static T[] ToSingleArray<T>(this T value) => [value]; // still needed in rare cases where [] doesn't work.
        public static T[,] CreateRectangularArray<T>(this Collections.WideEnumerable<T> source)
            => source.ToArray().CreateRectangularArray();
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
            throw new Exception();
        }
        public static int? GetFirstIndexWhereOrNull<T>(this IEnumerable<T> source, Func<T, bool> predicate)
        {
            try
            {
                return GetFirstIndexWhere(source, predicate);
            }
            catch (Exception)
            {
                return null;
            }
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
    }

    public static class IOHelpers
    {
        public static IEnumerable<string> Chunk(this Stream stream, string delimiter, StringSplitOptions options)
        {
            char[] buffer = new char[50];
            StringBuilder output = new StringBuilder();
            int read;
            using (StreamReader reader = new StreamReader(stream))
            {
                do
                {
                    read = reader.ReadBlock(buffer, 0, buffer.Length);
                    output.Append(buffer, 0, read);

                    string text = output.ToString();
                    int id = 0, total = 0;
                    while ((id = text.IndexOf(delimiter, id)) >= 0)
                    {
                        string line = text[total..id];
                        id += delimiter.Length;
                        if (options != StringSplitOptions.RemoveEmptyEntries || line != string.Empty)
                            yield return line;
                        total = id;
                    }
                    output.Remove(0, total);
                }
                while (read == buffer.Length);
            }

            if (options != StringSplitOptions.RemoveEmptyEntries || output.Length > 0)
                yield return output.ToString();
        }
    }
}
