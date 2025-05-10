using AsitLib.Numerics;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
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
        static readonly char[] Alphabet =
        {   'a', 'b',
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
            'y', 'z',
        };
        public static string Inverse(this string str) => string.Join("", str.ToArray().Reverse());
        public static int AlphabeticalConvert(this char c) => Alphabet.ToList().FindIndex(cc => cc == c);
        public static string ReEncode(this string str, Encoding encoding) => encoding.GetString(encoding.GetBytes(str));

        public static string NummicalInverse(this string str)
        {
            string toreturn = string.Empty;
            string data = String.Empty;
            foreach (char c in str)
            {
                if (!Alphabet.Contains(c.ToString().ToLower().ToCharArray()[0])) throw new Exception("Invalid String.");
                if (char.IsUpper(c)) data += "1";
                else data += "0";
            }
            foreach (char c in str.ToLower())
            {
                if (data[0] == '1') toreturn += Char.ToUpper(Alphabet.Reverse().ToArray()[c.AlphabeticalConvert()]);
                else toreturn += Alphabet.Reverse().ToArray()[c.AlphabeticalConvert()];
                data = data[1..];
            }
            return toreturn;
        }
        public static Stream ToStream(this string str)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(str);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
        public static string? FirstLine(this string str)
        {
            using var reader = new StringReader(str);
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
            var result = string.Empty;
            var maxI = values.Length;
            var maxJ = values[0].Length;
            for (var i = 0; i < maxI; i++)
            {
                result += ",{";
                for (var j = 0; j < maxJ; j++)
                {
                    result += $"{values[i][j]},";
                }

                result += "}";
            }
            return result.Replace(",}", "}")[1..];
        }
        public static string VisualizeNewLine(this string str) => str.Replace("\n", "\n<n>").Replace("\r", "\r<r>");
        /*a*/
        public static string IgnoreComments(this string str)
        {
            var blockComments = @"/\*(.*?)\*/";
            var lineComments = @"//(.*?)\r?\n";
            var strings = @"""((\\[^\n]|[^""\n])*)""";
            var verbatimStrings = @"@(""[^""]*"")+";
            string noComments = Regex.Replace(str,
                blockComments + "|" + lineComments + "|" + strings + "|" + verbatimStrings,
                me => {
                    if (me.Value.StartsWith("/*") || me.Value.StartsWith("//"))
                        return me.Value.StartsWith("//") ? Environment.NewLine : "";
                    // Keep the literal strings
                    return me.Value;
                }, RegexOptions.Singleline);
            return noComments;
        }
        public static IEnumerable<TResult> WhereSelect<TIn, TResult>(this IEnumerable<TIn> values, Func<TIn, (TResult value, bool pass)> predicate)
        {
            List<TResult> toret = new List<TResult>();
            foreach (TIn value in values)
            {
                var t = predicate.Invoke(value);
                if (t.pass) toret.Add(t.value);
            }
            return toret;
        }
        public static string IgnoreEscapes(this string str)
        {
            StringReader reader = new StringReader(str.Replace(@"\\", ((char)22).ToString()));
            bool ignoreNext = false;
            List<byte> toReturn = new List<byte>();
            for (int i; (i = reader.Read()) != -1;)
            {
                if (!ignoreNext) toReturn.Add((byte)i);
                else toReturn.RemoveAt(toReturn.Count - 1);
                if ((char)i == '\\') ignoreNext = true;
                else ignoreNext = false;
            }
            reader.Dispose();
            return Encoding.UTF8.GetString(toReturn.ToArray()).Replace(((char)22).ToString(), @"\\");
        }
        public static string NormalizeLE(this string input, string to = "\n") => Regex.Replace(input, @"\r\n|\n\r|\n|\r", to);
        public static bool Contains(this string input, string str, int maxcount) => Regex.Matches(input, str).Count <= maxcount;
        public static bool Contains(this string input, char c, int maxcount) => input.Where(cc => c == cc).Count() <= maxcount;

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
        public static Dictionary<string, string> DefaultEscapes => new Dictionary<string, string>
        {
            {"\\s", " "},
            {"\\n", "\n"},
            {"\\t", "\t"},
            {"\\dq", "\""},
            {"\\q", "\'"},
        };
        /// <summary>
        /// Returns String.Empty if the string is null.
        /// </summary>
        /// <param name="possibleNull">The string that *could* be null.</param>
        /// <returns>String.Empty if the string is null, else the input.</returns>
        public static string NullToEmptyString(string? possibleNull)
        {
            if (possibleNull == null) return String.Empty;
            else return possibleNull;
        }
        public static int SafeIntParse(this string i) => int.TryParse(i, out var v) ? v : -1;
        public static int? SafeNullIntParse(this string i) => int.TryParse(i, out var v) ? v : (int?)null;
        public static bool? SafeNullBoolParse(this string i)
        {
            return i.Equals("true", StringComparison.OrdinalIgnoreCase) ? true :
                   i.Equals("false", StringComparison.OrdinalIgnoreCase) ? false :
                   (bool?)null;
        }
        public static int GetCharCount(this string str, string c) => str.Split(c).Length - 1;
        public static string UnescapeCodes(this string src)
        {
            var rx = new Regex("\\\\([0-9A-Fa-f]+)");
            var res = new StringBuilder();
            var pos = 0;
            foreach (Match m in rx.Matches(src))
            {
                res.Append(src.Substring(pos, m.Index - pos));
                pos = m.Index + m.Length;
                res.Append((char)Convert.ToInt32(m.Groups[1].Value, 16));
            }
            res.Append(src[pos..]);
            return res.ToString();
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
        public static void SwitchIndexes<T>(ref T[] source, int index1, int index2)
        {
            T item1 = source[index1];
            T item2 = source[index2];

            source[index1] = item2;
            source[index2] = item1;
        }
        public static void SqueezeIndexes<T>(ref T[] source)
        {
            List<T?> squeezedList = new List<T?>();

            for (int i = 0; i < source.Length; i++)
                if (source[i] != null) squeezedList.Add(source[i]);
            while (squeezedList.Count != source.Length) squeezedList.Add(default(T));

            source = squeezedList.ToArray()!;
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
        public static void SetSize<T>(ref T[] source, int newSize)
        {
            if (source.Length == newSize) return;
            else if (source.Length > newSize)
            {
                source = source[..newSize];
                return;
            }
            else
            {
                List<T> values = new List<T>(source);
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
        public static T[] ToSingleArray<T>(this T value) => new[] { value };
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
        public static TArray[] Copy<TArray>(this TArray[] array) => array.ToArray();
        public static KeyValuePair<string, T>?[] ToKeyValuePair<T>(T[] source)
        {
            KeyValuePair<string, T>?[] toret = new KeyValuePair<string, T>?[source.Length];
            for (int i = 0; i < source.Length; i++)
                toret[i] = new KeyValuePair<string, T>(i.ToString(), source[i]);
            return toret;
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

    public static class GenericHelpers
    {
        public static void DoNothing() { }
        public static void ThrowException<T>(T exception) where T : Exception { }

        public static bool TryCatch(Action action1, Action action2, params Exception[]? exceptions)
        {
            try
            {
                action1.Invoke();
                return true;
            }
            catch (Exception ex)
            {
                if (exceptions == null) throw;
                if (exceptions.Contains(ex))
                    action2.Invoke();
                else throw;
                return false;
            }
        }
    }

    public static class IOHelpers
    {
        public static void Fill(byte[] bytes, string s, int startindex, int endindex, Encoding e) //needs work
        {
            byte[] strbytes = e.GetBytes(s)[..(endindex - startindex)];
            for (int i = 0; i < endindex && i < strbytes.Length; i++)
            {
                bytes[i + startindex] = strbytes[i];
            }
        }
        public static void WriteAllLinesLE(string path, IEnumerable<string> strings, string lineEnding = "\r\n")
        {
            File.WriteAllText(path, string.Join(lineEnding, strings) + lineEnding);
        }
        public static void AppendAllBytes(string path, byte[] bytes)
        {
            if (!File.Exists(path)) throw new Exception();
            using var stream = new FileStream(path, FileMode.Append);
            stream.Write(bytes, 0, bytes.Length);
        }
        public static void AppendToFile(string path, string content, FileStream fs, Encoding e, bool dispose = false)
        {
            long mempos = fs.Position;
            foreach (byte b in Encoding.UTF8.GetBytes(content))
            {
                fs.WriteByte(b);
            }
            fs.Position = mempos;
            if (dispose) fs.Dispose();
        }
        public static IEnumerable<string> Chunk(this Stream stream, string delimiter, StringSplitOptions options)
        {
            var buffer = new char[50];
            StringBuilder output = new StringBuilder();
            int read;
            using (var reader = new StreamReader(stream))
            {
                do
                {
                    read = reader.ReadBlock(buffer, 0, buffer.Length);
                    output.Append(buffer, 0, read);

                    var text = output.ToString();
                    int id = 0, total = 0;
                    while ((id = text.IndexOf(delimiter, id)) >= 0)
                    {
                        var line = text[total..id];
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
        public static IEnumerable<byte> GetBytes(string path)
        {
            FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read);
            for (int x; (x = stream.ReadByte()) != -1;)
            {
                yield return (byte)x;
            }
            stream.Dispose();
        }
    }

    public static class ControlHelpers
    {
        /// <summary>
        /// Allows thread safe updates of UI components
        /// </summary>
        public static void InvokeEx<T>(T @this, Action<T> action) where T : ISynchronizeInvoke
        {
            if (@this.InvokeRequired)
            {
                @this.Invoke(action, new object[] { @this });
            }
            else
            {
                action(@this);
            }
        }
    }

    public static class TextHelpers
    {

    }

    internal static class ThreadHelperClass
    {
        delegate void SetTextCallback(Form f, Control ctrl, string text);
        public static void SetText(Form form, Control ctrl, string text)
        {
            // InvokeRequired required compares the thread ID of the 
            // calling thread to the thread ID of the creating thread. 
            // If these threads are different, it returns true. 
            if (ctrl.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(SetText);
                form.Invoke(d, new object[] { form, ctrl, text });
            }
            else
            {
                ctrl.Text = text;
            }
        }
        delegate void SetReadOnlyCallback(Form f, Control ctrl, bool readOnly);
        public static void SetRead(Form form, Control ctrl, bool readOnly)
        {
            // InvokeRequired required compares the thread ID of the 
            // calling thread to the thread ID of the creating thread. 
            // If these threads are different, it returns true. 
            if (ctrl.InvokeRequired)
            {
                SetReadOnlyCallback d = new SetReadOnlyCallback(SetRead);
                form.Invoke(d, new object[] { form, ctrl, readOnly });
            }
            else
            {
                ((RichTextBox)ctrl.Tag).ReadOnly = readOnly;
            }
        }

    }
}
