using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using AsitLib;
#nullable enable

namespace AsitLib
{
    public static class AsitLibStatic
    {
        /// <summary>
        /// basicaly same as <see cref="Enumerable.Zip{TFirst, TSecond}(IEnumerable{TFirst}, IEnumerable{TSecond})"/>.. needs some work.
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
                if (exceptions.Contains(ex))
                    action2.Invoke();
                else throw ex;
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

    }
}
