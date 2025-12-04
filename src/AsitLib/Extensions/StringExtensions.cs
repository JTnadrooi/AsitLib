using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static AsitLib.StringHelpers;

namespace AsitLib
{
    public static class StringExtensions
    {
        public static string FirstLine(this string str)
        {
            using StringReader reader = new StringReader(str);
            return reader.ReadLine() ?? throw new InvalidOperationException("End of reader reached.");
        }

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
}
