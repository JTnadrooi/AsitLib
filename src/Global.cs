using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using AsitLib;
using System.IO;
using System.Runtime.CompilerServices;
#nullable enable

namespace AsitLib
{
    public enum CastMethod
    {
        CS,
        SpellScript,
    }
    public static class AsitGlobal
    {
        
        /// <summary>
        /// Basicaly same as <see cref="Enumerable.Zip{TFirst, TSecond}(IEnumerable{TFirst}, IEnumerable{TSecond})"/>.. needs some work.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="arrayKeys"></param>
        /// <param name="arrayValues"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static KeyValuePair<TKey, TValue>[] Merge<TKey, TValue>(TKey[] arrayKeys, TValue[] arrayValues)
        {
            if (arrayKeys.Length != arrayValues.Length) throw new InvalidOperationException("Array lenghts aren't the same: " + arrayKeys.Length + " | " + arrayValues.Length);
            List<KeyValuePair<TKey, TValue>> keyValuePairs = new List<KeyValuePair<TKey, TValue>>();
            for (int i = 0; i < arrayKeys.Count(); i++)
                keyValuePairs.Add(new KeyValuePair<TKey, TValue>(arrayKeys[i], arrayValues[i]));
            return keyValuePairs.ToArray();
        }
        public static string ReplaceAt(this string input, Index index, char newChar)
        {
            if (input == null) throw new ArgumentNullException("input");
            char[] chars = input.ToCharArray();
            chars[index] = newChar;
            return new string(chars);
        }
        public static string ReplaceAt(this string input, NormalizedRange range, char newChar)
        {
            char[] inputChars = input.ToCharArray();
            for (int i = range.Start; i <= range.End; i++)
                inputChars[i] = newChar;
            return new string(inputChars);
        }
        public static object?[] GetItems(this ITuple tuple)
        {
            List<object?> toret2 = new List<object?>();
            for (int i = 0; i < tuple.Length; i++)
                toret2.Add(tuple[i]);
            return toret2.ToArray();
        }
        public static void Fill(byte[] bytes, string s, int startindex, int endindex, Encoding e) //needs work
        {
            byte[] strbytes = e.GetBytes(s)[..(endindex - startindex)];
            for (int i = 0; i < endindex && i < strbytes.Length; i++)
            {
                bytes[i + startindex] = strbytes[i];
            }
        }
        public static T[] ToSingleArray<T>(this T value)
            => new T[] { value };
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
        public static string ProccesEscapes(string input, params KeyValuePair<string, string>[] escapes)
            => ProccesEscapes(input, new Dictionary<string, string>(escapes));
        public static string ProccesEscapes(string input, string escape, string replaceWith)
            => ProccesEscapes(input, new KeyValuePair<string, string>(escape, replaceWith));
        public static string ProccesEscapes(string input, KeyValuePair<string, string> escape)
            => ProccesEscapes(input, new KeyValuePair<string, string>[] { escape, });
        public static string ProccesEscapes(string input, Dictionary<string, string> escapes)
        {
            escapes.ToList().ForEach(kvp => input = input.Replace(kvp.Key, kvp.Value));
            return input;
        }
        public static Dictionary<string, string> DefaultEscapes
            => new Dictionary<string, string>(new KeyValuePair<string, string>[]
                {
                    new KeyValuePair<string, string>(@"\s", " "),
                    new KeyValuePair<string, string>(@"\n", "\n"),
                    new KeyValuePair<string, string>(@"\t", "\t"),
                    new KeyValuePair<string, string>(@"\dq", "\""),
                    new KeyValuePair<string, string>(@"\q", "\'"),
                });
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
            {
                if (predicate.Invoke(item)) return index;
                index++;
            }
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
        public static bool TryCatch(Action action1, Action action2, params Exception[]? exceptions)
        {
            try
            {
                action1.Invoke();
                return true;
            }
            catch(Exception ex)
            {
                if (exceptions == null) throw;
                if (exceptions.Contains(ex))
                    action2.Invoke();
                else throw;
                return false;
            }
        }
        public static int ArrayAdd<T>(this T[] array, T value)
        {
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] == null)
                {
                    array[i] = value;
                    return i;
                }
            }
            throw new ArgumentException("Array has no empty spots.");
        } 
        public static bool EasyTryOut<TFunc, TOut>(TFunc func, out TOut output) where TFunc : Delegate
        {
            try
            {
                output = (TOut)func.DynamicInvoke()!;
                return true;
            }
            catch (Exception)
            {
                output = default!;
                return false;
            }
        }
        public static IEnumerable<T> ElementsAt<T>(this IEnumerable<T> values, Range range)
        {
            return values.ToArray()[range];
        }
        /// <summary>
        /// Determines a text file's encoding by analyzing its byte order mark (BOM).
        /// Defaults to ASCII when detection of the text file's endianness fails.
        /// </summary>
        /// <param name="filename">The text file to analyze.</param>
        /// <returns>The detected encoding.</returns>
        public static Encoding GetEncoding(string filename) //stack overflow
        {
            // Read the BOM
            var bom = new byte[4];
            using (var file = new FileStream(filename, FileMode.Open, FileAccess.Read))
                file.Read(bom, 0, 4);

            // Analyze the BOM
            #pragma warning disable SYSLIB0001 // Type or member is obsolete
            if (bom[0] == 0x2b && bom[1] == 0x2f && bom[2] == 0x76) return Encoding.UTF7;
            #pragma warning restore SYSLIB0001 // Type or member is obsolete
            if (bom[0] == 0xef && bom[1] == 0xbb && bom[2] == 0xbf) return Encoding.UTF8;
            if (bom[0] == 0xff && bom[1] == 0xfe && bom[2] == 0 && bom[3] == 0) return Encoding.UTF32; //UTF-32LE
            if (bom[0] == 0xff && bom[1] == 0xfe) return Encoding.Unicode; //UTF-16LE
            if (bom[0] == 0xfe && bom[1] == 0xff) return Encoding.BigEndianUnicode; //UTF-16BE
            if (bom[0] == 0 && bom[1] == 0 && bom[2] == 0xfe && bom[3] == 0xff) return new UTF32Encoding(true, true);  //UTF-32BE
            return Encoding.ASCII;
        }
    }
}
