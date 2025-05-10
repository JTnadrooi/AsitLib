using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.IO.Compression;
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
    /// <summary>
    /// Thanks stack! (Not for all)
    /// </summary>
    public static class Used
    {
        public static int[] GetIndexes<T>(this IEnumerable<T> values, Func<T, bool> validator)
            => Enumerable.Range(0, values.Count()).Where(i => validator.Invoke(values.ToArray()[i])).ToArray();
            //values.Select(v => (v, Array.IndexOf(values.ToArray(), v))).Where(tup => validator.Invoke(tup.v)).Select(tup => tup.Item2).ToArray();
        public static Range[] ToBetweenRanges(this IEnumerable<int> ints)
        {
            //Console.WriteLine(ints.ToJoinedString("/") + "<<");
            if (ints.Any(i => i < 0) || ints.Count() != ints.Distinct().Count()) throw new ArgumentException("Invalid int array.");
            List<Range> toret = new List<Range>();
            int last = 0;
            foreach(int i in ints)
            {
                toret.Add(new Range(last, i));
                last = i;
            }
            //Console.WriteLine(toret.ToArray()[1..].ToJoinedString("//"));
            return toret.ToArray()[1..];
        }
        /// <summary>
        /// Suprisingly usefull when in those <see langword="void"/> returning one-liner functions/methods!
        /// </summary>
        public static void DoNothing() { }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="exception"></param>
        public static void ThrowException<T>(T exception) where T : Exception { }
        public static IDictionary<TKey, TValue> CopyPartailly<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, params TKey[] keys) where TValue : ICloneable
        {
            List<KeyValuePair<TKey, TValue>> toret = new List<KeyValuePair<TKey, TValue>>();
            for (int i = 0; i < keys.Length; i++)
                toret.Add(new KeyValuePair<TKey, TValue>(keys[i], (TValue)dictionary[keys[i]].Clone()));
            return new Dictionary<TKey, TValue>(toret.ToArray());
        }
        //public static Stream ToStream(this string s)
        //{
        //    var stream = new MemoryStream();
        //    var writer = new StreamWriter(stream);
        //    writer.Write(s);
        //    writer.Flush();
        //    stream.Position = 0;
        //    return stream;
        //}
        public static TArray[] Copy<TArray>(this TArray[] array)
        {
            TArray[] toret = new TArray[array.Length];
            array.CopyTo(toret, 0);
            return toret;
        }
        public static void CheckNull(object? maybeNull, [CallerArgumentExpression("maybeNull")] string thrownVarableName = "input for CheckNull") => CheckNull(maybeNull, new ArgumentNullException(thrownVarableName));
        public static void CheckNull(object? maybeNull,  Exception exception)
        {
            if (maybeNull == null) throw exception;
        }

        //public static byte[] ToByteArray(this string str) => str.ToCharArray().Select(c => (byte)c).ToArray();
        public static byte[] ToByteArray(this string str) => Encoding.UTF8.GetBytes(str);
        public static string FromByteToString(this byte[] by) => string.Join("", by.Select(b => (char)b).ToArray());
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "Mirroring default behavior.")]
        public static void CreateRegistryKeys(string fileExtension, string master, string desc, string imageIcoPath)
        {
            if (new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator))
            {
                string location = master + "." + fileExtension[1..].ToUpper();
                Registry.ClassesRoot.CreateSubKey(fileExtension).SetValue(null, location);
                Registry.ClassesRoot.CreateSubKey(location).SetValue(null, desc);
                Registry.ClassesRoot.CreateSubKey(location + "\\DefaultIcon").SetValue(null, imageIcoPath);
                return;
            }
        }
        public static void WriteAllLinesLE(string path, IEnumerable<string> strings, string lineEnding = "\r\n") => File.WriteAllText(path, string.Join(lineEnding, strings) + lineEnding);
        public static byte[]? ReadEncodingLine(this FileStream fs)
        {
            List<byte> toReturn = new List<byte>();
            for (int i = 0; i != '\u000a' && fs.Length != fs.Position; i = fs.ReadByte())
            {
                if (i == -1) return null;
                toReturn.Add((byte)i);
            }
            return toReturn.ToArray();
        }
        public static bool OnlyLettersPlusUnderscore(this string str) => Regex.IsMatch(str, @"^[a-zA-Z_]+$");
        //public static string ToJoinedString<T>(this IEnumerable<T>? ts, string joiner = "")
        //{
        //    if (ts == null) return "Null";
        //    else return string.Join(joiner, ts);
        //}
        public static void AppendAllBytes(string path, byte[] bytes)
        {
            if (!File.Exists(path)) throw new Exception();
            using var stream = new FileStream(path, FileMode.Append); 
            stream.Write(bytes, 0, bytes.Length);
        }
        public static int PuncToInt(this string str)
        {
            int Iout = 0;
            foreach (char c in str.Where(c => c == '.' || c == ':'))
                if (c == '.') Iout++;
            else if (c == ':') Iout += 2;
            return Iout;
        }
        public static void AppendToFile(string path, string content, FileStream fs, Encoding e, bool dispose = false)
        {
            long mempos = fs.Position;
            foreach (byte b in Encoding.UTF8.GetBytes(content))
            {
                fs.WriteByte(b);
            }
            fs.Position = mempos;
            if(dispose) fs.Dispose();
        }
        //public static void AppendReadify(string path, string lineEnder, FileStream fs, Encoding e)
        //{
        //    if(!File.ReadLines(path).Last().EndsWith(lineEnder)) AppendToFile(lineEnder)
        //}
        public static IEnumerable<T> ConcatToStart<T>(this IEnumerable<T> values, T value)
        {
            T[] newValues = new T[values.Count() + 1];
            newValues[0] = value;
            Array.Copy(values.ToArray(), 0, newValues, 1, values.Count());
            return newValues;
        }
        /// <summary>
        /// Returns -1 if Parsing failed.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public static int SafeIntParse(this string i)
        {
            try { return int.Parse(i); }
            catch (Exception) { return -1; }
        }
        /// <summary>
        /// Returns null if Parsing failed.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public static int? SafeNullIntParse(this string i)
        {
            try { return int.Parse(i); }
            catch (Exception) { return null; }
        }
        /// <summary>
        /// Returns null if Parsing failed.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public static bool? SafeNullBoolParse(this string i)
        {
            if(i == "true" || i == "True") return true;
            else if(i == "false" || i == "False") return false;
            else return null;
        }
        /// <summary>
        /// Gets the count of strinf sequence in the specified string.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        public static int GetCharCount(this string str, string c) => str.Split(c).Length - 1;
        public static string Capatalize(this string str) => Char.ToUpper(str[0]) + str[1..];
        public static (int DataLenght, int GottenInt) FirstIntData(string str)
        {
            using StringReader reader = new StringReader(str);
            if (SafeIntParse(str[0].ToString()) == -1) throw new ArgumentException("Invalid string.");
            reader.Read();
            string _int = "";
            for (int i = 0; i < SafeIntParse(str[0].ToString()); i++)
                _int += (char)reader.Read();
            if (SafeIntParse(_int) == -1) throw new ArgumentException("Invalid string.");
            return (SafeIntParse(str[0].ToString()), SafeIntParse(_int));
        }
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
        public static string KeyCodeToUnicode(Keys key)
        {
            byte[] keyboardState = new byte[255];
            bool keyboardStateStatus = GetKeyboardState(keyboardState);

            if (!keyboardStateStatus)
            {
                return "";
            }

            uint virtualKeyCode = (uint)key;
            uint scanCode = MapVirtualKey(virtualKeyCode, 0);
            IntPtr inputLocaleIdentifier = GetKeyboardLayout(0);

            StringBuilder result = new StringBuilder();
            ToUnicodeEx(virtualKeyCode, scanCode, keyboardState, result, (int)5, (uint)0, inputLocaleIdentifier);

            return result.ToString();
        }

        [DllImport("user32.dll")]
        static extern bool GetKeyboardState(byte[] lpKeyState);

        [DllImport("user32.dll")]
        static extern uint MapVirtualKey(uint uCode, uint uMapType);

        [DllImport("user32.dll")]
        static extern IntPtr GetKeyboardLayout(uint idThread);

        [DllImport("user32.dll")]
        static extern int ToUnicodeEx(uint wVirtKey, uint wScanCode, byte[] lpKeyState, [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pwszBuff, int cchBuff, uint wFlags, IntPtr dwhkl);
        public static ConsoleColor ColorToConsoleColor(Color a)
        {
            byte r = a.R;
            byte g = a.G;
            byte b = a.B;
            ConsoleColor ret = 0;
            double rr = r, gg = g, bb = b, delta = double.MaxValue;
            foreach (ConsoleColor? cc in Enum.GetValues(typeof(ConsoleColor)))
            {
                string n = Enum.GetName(typeof(ConsoleColor), cc ?? throw new ArgumentNullException("Consolecolornull"))!;
                Color c = System.Drawing.Color.FromName(n == "DarkYellow" ? "Orange" : n); // bug fix
                double t = System.Math.Pow(c.R - rr, 2.0) + System.Math.Pow(c.G - gg, 2.0) + System.Math.Pow(c.B - bb, 2.0);
                if (t == 0.0) return cc.Value;
                if (t < delta)
                {
                    delta = t;
                    ret = cc.Value;
                }
            }
            return ret;
        }
        public static System.Drawing.Color FromColor(System.ConsoleColor c)
        {
            int[] cColors = {   0x000000, //Black = 0
                        0x000080, //DarkBlue = 1
                        0x008000, //DarkGreen = 2
                        0x008080, //DarkCyan = 3
                        0x800000, //DarkRed = 4
                        0x800080, //DarkMagenta = 5
                        0x808000, //DarkYellow = 6
                        0xC0C0C0, //Gray = 7
                        0x808080, //DarkGray = 8
                        0x0000FF, //Blue = 9
                        0x00FF00, //Green = 10
                        0x00FFFF, //Cyan = 11
                        0xFF0000, //Red = 12
                        0xFF00FF, //Magenta = 13
                        0xFFFF00, //Yellow = 14
                        0xFFFFFF  //White = 15
                    };
            return Color.FromArgb(cColors[(int)c]);
        }
        public static Color DrawingColor(ConsoleColor color) => color switch
        {
            ConsoleColor.Black => Color.Black,
            ConsoleColor.Blue => Color.Blue,
            ConsoleColor.Cyan => Color.Cyan,
            ConsoleColor.DarkBlue => Color.DarkBlue,
            ConsoleColor.DarkGray => Color.DarkGray,
            ConsoleColor.DarkGreen => Color.DarkGreen,
            ConsoleColor.DarkMagenta => Color.DarkMagenta,
            ConsoleColor.DarkRed => Color.DarkRed,
            ConsoleColor.DarkYellow => Color.FromArgb(255, 128, 128, 0),
            ConsoleColor.Gray => Color.Gray,
            ConsoleColor.Green => Color.Green,
            ConsoleColor.Magenta => Color.Magenta,
            ConsoleColor.Red => Color.Red,
            ConsoleColor.White => Color.White,
            ConsoleColor.DarkCyan => Color.DarkCyan,
            ConsoleColor.Yellow => Color.Yellow,
            _ => Color.Red,
        };
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
        public static string UnescapeCodes(this string src)
        {
            var rx = new Regex("\\\\([0-9A-Fa-f]+)");
            var res = new StringBuilder();
            var pos = 0;
            foreach (Match? m in rx.Matches(src))
            {
                res.Append(src.Substring(pos, m?.Index ?? default(int) - pos));
                pos = m?.Index ?? default(int) + (m?.Length ?? default);
                res.Append((char)Convert.ToInt32((m ?? throw new Exception("Invalid match.")).Groups[1].ToString(), 16));
            }
            res.Append(src[pos..]);
            return res.ToString();
        }
        public static Form? GetParentForm(Control parent)
        {
            Form? form = parent as Form;
            if (form != null)
            {
                return form;
            }
            if (parent != null)
            {
                // Walk up the control hierarchy
                return GetParentForm(parent.Parent);
            }
            return null; // Control is not on a Form
        }
        public static int SyllableCount(string word)
        {
            word = word.ToLower().Trim();
            int count = System.Text.RegularExpressions.Regex.Matches(word, "[aeiouy]+").Count;
            //if ((word.EndsWith("e") || (word.EndsWith("es") || word.EndsWith("ed"))) && !word.EndsWith("le"))
            //    count--;
            return count;
        }
        public static string[] GetSyllables(string word)
        {
            word = word.ToLower().Trim();
            List<string> syllableList = new List<string>();
            bool lastWasVowel = false;
            string vowels = "aeiouy";

            StringBuilder currSyllable = new StringBuilder();

            foreach (char c in word)
            {
                if (vowels.Contains(c))
                {
                    if (!lastWasVowel)
                    {
                        lastWasVowel = true;

                        // Finish this syllable and add to the list
                        syllableList.Add(currSyllable.ToString());
                        currSyllable.Clear();
                    }
                }
                else
                {
                    lastWasVowel = false;
                }

                // Add this character to the current syllable
                currSyllable.Append(c);
            }


            return syllableList.ToArray();
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
