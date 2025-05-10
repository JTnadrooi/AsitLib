using AsitLib;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
#nullable enable

namespace AsitLib
{
    public  static class StringExtensions
    {
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
            foreach(char c in str)
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

    }
    public enum BetweenMethod
    {
        FirstFirst,
        FirstLast,
        LastLast,
    }
}
