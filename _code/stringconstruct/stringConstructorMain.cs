using AsitLib;
using AsitLib.IO;
using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AsitLib.StringContructor;
using AsitLib.StringContructor.Defaults;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;

namespace AsitLib.StringContructor
{
    /// <summary>
    /// Different ways of searching through a <see cref="Array"/> of <see cref="GString"/> objects. <br/>
    /// <see cref="Enum"/> used for the <see cref="StringConstructorUtils.GetFromGSArray(GString[], int, Random, StringContructSearchMode)"/> method.
    /// </summary>
    public enum StringContructSearchMode
    {
        /// <summary>
        /// Get only <see cref="string"/>s that have the <see cref="GString.FromStart"/> property set to true.
        /// </summary>
        FromStart,
        /// <summary>
        /// Get only <see cref="string"/>s that have the <see cref="GString.FromEnd"/> property set to true.
        /// </summary>
        FromEnd,
        /// <summary>
        /// Get only any <see cref="string"/>s.
        /// </summary>
        All,
        /// <summary>
        /// Get only <see cref="string"/>s that have both the <see cref="GString.FromEnd"/> and <see cref="GString.FromEnd"/> property set to false.
        /// </summary>
        Normal
    }
    /// <summary>
    /// Utils for the <see langword="classes"/> and <see langword="structs"/> in the <see cref="StringContructor"/> namespace.
    /// </summary>
    public static class StringConstructorUtils
    {
        /// <summary>
        /// Construct a <see cref="string"/> using a given <see cref="AsitContructorConfig"/>.
        /// </summary>
        /// <param name="config"><see cref="AsitContructorConfig"/> to construct a <see cref="string"/> from.</param>
        /// <returns>
        /// A <see cref="string"/> made from the <see cref="AsitContructorConfig.Occurrences"/> in the given <paramref name="config"/>
        /// after being edited by the given <see cref="AsitContructorConfig.Manipulator"/> and verified by the contained <see cref="AsitContructorConfig.Validator"/>.
        /// </returns>
        /// <exception cref="InvalidRequestException"></exception>
        /// <exception cref="Exception"></exception>
        public static string Generate(AsitContructorConfig config)
        {
            Random random;
            if (config.Seed <= 0) random = new Random();
            else random = new Random(config.Seed);
            if (config.Manipulator == null) config.Manipulator = (str) => str;
            else config.Manipulator = config.Manipulator;
            if (config.Validator == null) config.Validator = (str) => true;
            else config.Validator = config.Validator;
            if (config.Occurrences == null || config.Occurrences.Length == 0) throw new ArgumentException("Invalid GString array.");
            if (!config.Occurrences.Any(g => g.Value.Length == 1)) throw new ArgumentException("Invalid GString array; must contain GString with 1 lenght.");

            int attemts = 0;
            Restart:
            string toret = String.Empty;
            int lenghtFree = config.Lenght;
            while (lenghtFree != 0) //Contruct string.
            {
                string gotten = String.Empty;
                GString[] Searchfrom = new GString[0];
                if (toret.Length == 0) //start
                    Searchfrom = config.Occurrences.Where(s => s.FromStart && s.Value.Length <= lenghtFree).ToArray();
                else if (config.Occurrences.Any(o => o.FromEnd && o.Value.Length == lenghtFree)) //end
                    Searchfrom = config.Occurrences.Where(o => o.FromEnd && o.Value.Length == lenghtFree).ToArray();
                if (Searchfrom.Length != 0 && !config.DiscardWeight) gotten = StringConstructorUtils.GetFromGSArray(Searchfrom, random: random, searchMode: StringContructSearchMode.All);
                if (Searchfrom.Length != 0 && config.DiscardWeight) gotten = Searchfrom[random.Next(Searchfrom.Length)].Value;
                if (Searchfrom.Length == 0) gotten = StringConstructorUtils.GetFromGSArray(config.Occurrences.Where(s => !s.FromStart && !s.FromEnd && s.Value.Length <= lenghtFree).ToArray(), random: random, searchMode: StringContructSearchMode.All);
                toret += gotten;
                lenghtFree -= gotten.Length;
                if (gotten == "") lenghtFree -= 1;
            }
            if (!config.Validator.Invoke(toret))
                if (attemts == config.Attempts) throw new InvalidRequestException("Values given to construct the string are invalid; Attempt Overflow.");
                else
                {
                    attemts++;
                    goto Restart;
                }
            toret = Used.Capatalize(toret);
            try
            {
                return config.Manipulator.Invoke(toret);
            }
            catch (Exception e)
            {
                throw new Exception("Encapsuled function failed; " + e.Message);
            }
        }
        /// <summary>
        /// Counts the maximum number of <see cref="char"/> objects occuring in a sequence in the given <see cref="string"/>.
        /// </summary>
        /// <param name="str">The <see cref="string"/> to count the largest sequence from.</param>
        /// <param name="exact">If <see langword="true"/>, sequences may only exist of exact duplicates.</param>
        /// <param name="chars">Sole characters a sequence must consist of.</param>
        /// <returns>The lenght of the largest sequence in the given <see cref="string"/>.</returns>
        public static int CountSequence(string str, bool exact, char[] chars)
        {
            int in_sequence = 0;
            int max = 0;
            char last = '\0';
            if (exact)
                foreach (char c in str)
                {
                    if (chars.Contains(c) && last == c)
                        in_sequence++;
                    if (in_sequence > max) max = in_sequence;
                    last = c;
                }
            else
                foreach (char c in str)
                    if (chars.Contains(c)) in_sequence++;
                    else
                    {
                        if (in_sequence > max) max = in_sequence;
                        in_sequence = 0;
                    }

            if (in_sequence > max) max = in_sequence;
            if (last != '\0') max++;
            return max;
        }
        /// <summary>
        /// Counts the maximum number of <see cref="char"/> objects occuring in a sequence in the given <see cref="string"/>.
        /// </summary>
        /// <param name="str">The <see cref="string"/> to count the largest duplicate sequence from.</param>
        /// <returns>The lenght of the largest sequence in the given <see cref="string"/>.</returns>
        public static int CountSequence(string str) => CountSequence(str, true, str.ToCharArray());
        /// <summary>
        /// Not implemented.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="exact"></param>
        /// <param name="occurences"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public static int CountSequence(string str, bool exact, string[] occurences)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Incorporates the <see cref="GString.Weight"/> property to get a <see cref="string"/> from the <see cref="Array"/> of <see cref="GString"/> objects.
        /// </summary>
        /// <returns>A random <see cref="string"/> from the <see cref="GString"/> <see cref="Array"/> influenced by the <see cref="GString.Weight"/> property.</returns>
        public static string GetFromGSArray(GString[] strings, int accuracy = 1, Random random = null, StringContructSearchMode searchMode = StringContructSearchMode.Normal)
        {
            if (strings == null || strings.Length == 0) throw new ArgumentException("Invalid GString array.");
            if (random == null) random = new Random();
            switch (searchMode)
            {
                case StringContructSearchMode.FromStart: strings = strings.Where(s => s.FromStart).ToArray(); break;
                case StringContructSearchMode.FromEnd: strings = strings.Where(s => s.FromEnd).ToArray(); break;
                case StringContructSearchMode.Normal: strings = strings.Where(s => !s.FromEnd && !s.FromStart).ToArray(); break;
                case StringContructSearchMode.All: break;
                default: throw new ArgumentException("Invalid SearchMode.");
            }
            static int ZeroS(int times)
            {
                string toret = "1";
                for (int i = 0; i < times; i++) toret += "0";
                return Int32.Parse(toret);
            }
            int lowest = strings.Select(s => s.Weight).Min();
            //Console.WriteLine("l = " +lowest);
            List<string> endarr = new List<string>();
            foreach (GString s in strings.Select(g => g.Value))
            {
                float fplaces = (float)System.Math.Round(s.Weight / (float)lowest, accuracy);
                int places = (int)(fplaces * ZeroS(accuracy));
                //Console.WriteLine(s + ":" + s.Weight + " -> " + fplaces + ", " + places);
                for (int i = 0; i < places; i++) endarr.Add(s);
            }
            //Console.WriteLine(endarr.ToJoinedString(", "));
            return endarr[random.Next(endarr.Count)];
        }
        /// <summary>
        /// Stack the <see cref="GString.Weight"/> property of the captured Arrays.
        /// </summary>
        /// <param name="strs1">1st <see cref="GString"/> <see cref="Array"/>.</param>
        /// <param name="strs2">2nd <see cref="GString"/> <see cref="Array"/>.</param>
        /// <param name="strs3">3rd <see cref="GString"/> <see cref="Array"/>.</param>
        /// <param name="strs4">4th <see cref="GString"/> <see cref="Array"/>.</param>
        /// <returns>A GString array with the <see cref="GString.Weight"/> property of all <see cref="GString"/> objects added to equals.</returns>
        public static GString[] Stack(GString[] strs1, GString[] strs2, GString[] strs3, GString[] strs4) => Stack(Stack(strs1, strs2, strs3), strs4);
        /// <summary>
        /// Stack the <see cref="GString.Weight"/> property of the captured Arrays.
        /// </summary>
        /// <param name="strs1">1st <see cref="GString"/> <see cref="Array"/>.</param>
        /// <param name="strs2">2nd <see cref="GString"/> <see cref="Array"/>.</param>
        /// <param name="strs3">3rd <see cref="GString"/> <see cref="Array"/>.</param>
        /// <returns>A GString array with the <see cref="GString.Weight"/> property of all <see cref="GString"/> objects added to equals.</returns>
        public static GString[] Stack(GString[] strs1, GString[] strs2, GString[] strs3) => Stack(Stack(strs1, strs2), strs3);
        /// <summary>
        /// Stack the <see cref="GString.Weight"/> property of the captured Arrays.
        /// </summary>
        /// <param name="strs1">1st <see cref="GString"/> <see cref="Array"/>.</param>
        /// <param name="strs2">2nd <see cref="GString"/> <see cref="Array"/>.</param>
        /// <returns>A GString array with the <see cref="GString.Weight"/> property of all <see cref="GString"/> objects added to equals.</returns>
        public static GString[] Stack(this GString[] strs1, GString[] strs2) => strs1.Concat(strs2).ToArray().Clean();
        /// <summary>
        /// Cast every <see cref="char"/> in this <see cref="Array"/> to a <see cref="GString"/> with <see cref="GString.Weight"/> set according to the <paramref name="seletor"/>.
        /// </summary>
        /// <param name="chars"><see cref="Array"/> of <see cref="char"/> objects to convert.</param>
        /// <param name="seletor"></param>
        /// <returns>A <see cref="Array"/> of <see cref="GString"/> objects casted from this array.</returns>
        public static GString[] ToGSArray(this char[] chars, Func<int, int> seletor) => chars.Select(s => (GString)s).ToArray().ChangeValues(seletor);
        /// <summary>
        /// Cast every <see cref="char"/> in this <see cref="Array"/> to a <see cref="GString"/> with <see cref="GString.Weight"/> set to <see langword="0"/>.
        /// </summary>
        /// <param name="chars"></param>
        /// <returns>A <see cref="Array"/> of <see cref="GString"/> objects casted from this array.</returns>
        public static GString[] ToGSArray(this char[] chars) => chars.Select(s => (GString)s).ToArray();
        /// <summary>
        /// Cast every <see cref="string"/> in this <see cref="Array"/> to a <see cref="GString"/> with <see cref="GString.Weight"/> set to <see langword="0"/>.
        /// </summary>
        /// <param name="strings"><see cref="Array"/> of <see cref="string"/> objects to convert.</param>
        /// <returns>A <see cref="Array"/> of <see cref="GString"/> objects casted from this array.</returns>
        public static GString[] ToGSArray(this string[] strings) => strings.Select(s => (GString)s).ToArray();
        /// <summary>
        /// Cast every <see cref="string"/> in this <see cref="Array"/> to a <see cref="GString"/> with <see cref="GString.Weight"/> set according to the <paramref name="seletor"/>.
        /// </summary>
        /// <param name="strings"><see cref="Array"/> of <see cref="string"/> objects to convert.</param>
        /// <param name="seletor"></param>
        /// <returns>A <see cref="Array"/> of <see cref="GString"/> objects casted from this array.</returns>
        public static GString[] ToGSArray(this string[] strings, Func<int, int> seletor) => strings.Select(s => (GString)s).ToArray().ChangeValues(seletor);
        /// <summary>
        /// Cast every <see cref="GString"/> in this <see cref="Array"/> to a <see cref="string"/>. <br/>
        /// <i>Casted from the <see cref="GString.Value"/> Property.</i>
        /// </summary>
        /// <param name="gStrings"><see cref="Array"/> of <see cref="GString"/> objects.</param>
        /// <returns>A <see cref="string"/> <see cref="Array"/> made from casted <see cref="GString"/> objects.</returns>
        public static string[] ToStringArray(this GString[] gStrings) => gStrings.Select(s => s.Value).ToArray();
        /// <summary>
        /// Convert this <see cref="string"/> to a <see cref="GString"/> with set weight.
        /// </summary>
        /// <param name="str">A <see cref="string"/> to construct a <see cref="GString"/> from.</param>
        /// <param name="weight">Set the <see cref="GString.Weight"/> Property of the constructed <see cref="GString"/>.</param>
        /// <returns></returns>
        public static GString ToGString(this string str, int weight) => new GString(str, weight);
        /// <summary>
        /// Convert this <see cref="char"/> to a <see cref="GString"/> with set weight.
        /// </summary>
        /// <param name="c">A <see cref="char"/> to construct a <see cref="GString"/> from.</param>
        /// <param name="weight">Set the <see cref="GString.Weight"/> Property of the constructed <see cref="GString"/>.</param>
        /// <returns></returns>
        public static GString ToGString(this char c, int weight) => new GString(c, weight);
        /// <summary>
        /// Change the <see cref="GString.Weight"/> property of all <see cref="GString"/> objects provided.
        /// </summary>
        /// <param name="strings">A <see cref="Array"/> of <see cref="GString"/> objects.</param>
        /// <param name="seletor">A <see cref="Func{T, TResult}"/> that changes the <see cref="GString.Weight"/> property of all provided <see cref="GString"/> objects.</param>
        /// <returns>A <see cref="GString"/> <see cref="Array"/> where all <see cref="GString.Weight"/> property values have been changed according to the given <paramref name="seletor"/>.</returns>
        public static GString[] ChangeValues(this GString[] strings, Func<int, int> seletor) => strings.Select(s =>
        {
            s.Weight = seletor.Invoke(s.Weight);
            return s;
        }).ToArray();
        /// <summary>
        /// Create a new <see cref="GString"/> <see cref="Array"/> where all the <see cref="GString.FromStart"/> and <see cref="GString.FromEnd"/>
        /// properties are both set to their default value. (<see langword="false"/>)
        /// </summary>
        /// <param name="strings"><see cref="Array"/> of <see cref="GString"/>s</param>
        /// <returns>
        /// A new <see cref="GString"/> <see cref="Array"/> where all the <see cref="GString.FromStart"/> and <see cref="GString.FromEnd"/>
        /// properties are both set to their default value. (<see langword="false"/>)
        /// </returns>
        public static GString[] DiscardIndicators(this GString[] strings) => strings.Select(s => new GString(s.Value)).ToArray();
        /// <summary>
        /// Removes duplicates and stacks the holden <see cref="GString.Weight"/> properties.
        /// </summary>
        /// <param name="strings"><see cref="Array"/> of <see cref="GString"/></param>
        /// <returns>The <paramref name="strings"/> <see cref="Array"/> with all duplicate <see cref="GString.Weight"/> properties cleaned.</returns>
        public static GString[] Clean(this GString[] strings) => strings.GroupBy(
                s => s.ToString(),
                s => s.Weight,
                (mainValue, weights) => new
                {
                    Key = mainValue,
                    Value = weights.Sum(),
                }).Select(s => new GString(s.Key, s.Value)).ToArray();
        //public static GString[] Learn(string[] examples)
        //{
        //    List<GString> parts = new List<GString>();
        //    //examples = examples.Select(e => "[" + e + "]").ToArray()[..1];
        //    examples = examples.Select(e =>  e).ToArray();
        //    foreach (string example in examples)
        //    {
        //        int icurrent = 0;
        //        string examout = example.ToLower().Trim();
        //        Console.WriteLine(examout);
        //        Console.WriteLine(Used.GetSyllables(examout));
        //        //parts.Concat(Used.GetSyllables(examout).Where(p => p != string.Empty).Select(s => new GString(s, 1)).ToList());
        //        foreach (var item in Used.GetSyllables(examout).Where(p => p != string.Empty).Select(s => new GString(s, 1)))
        //        {
        //            parts.Add(item);
        //        }
        //        //Console.WriteLine(Used.SyllableCount(examout));
        //        ////examout = Regex.Replace
        //        ////(example, "([aeiou])", @"\\1-");
        //        ////examout = Regex.Replace(examout, "-$", "");
        //        ////examout = Regex.Replace(examout, "-([bcdfghjklmnpqrstvxz]$)", @"\\1");
        //        ////examout = Regex.Replace(examout, "-(n|r|st)(t|n|d|f)", @"\\1-\\2");
        //        ////examout = Regex.Replace(examout, "([aeiou])-s([tpnml])", @"\\1s-\\2");
        //        ////examout = [0];
        //        //foreach (string str in Regex.Matches(examout, "[aeiouy]+").Cast<Match>().Select(m => m.Value)) parts.Add(str);

        //        //parts.Add(new GString(examout, 1));
        //    }
        //    return parts.ToArray().Clean().Where(p => p != string.Empty).ToArray();
        //}
        public static bool ValidateConfig(AsitContructorConfig config) => config.Lenght > 1 
            && config.Accuracy > 0 
            && config.Attempts > 0 
            && config.Occurrences.Length > 0 
            && config.Occurrences.Any(o => o.Value.Length == 1);
        public static GString[] Learn(string[] examples) => Learn(examples, new Func<string, string[]>((str) => str.ToCharArray().Select(c => c.ToString()).ToArray()));
        public static GString[] Learn(string[] examples, Func<string, string[]> splitter) => Learn(string.Join("", examples), splitter);
        public static GString[] Learn(string example) => Learn(new string[] { example });
        public static GString[] Learn(string example, Func<string, string[]> splitter)
        {
            if (splitter == null) throw new ArgumentException("\"splitter\" was null.");
            return splitter.Invoke(example).Select(s =>
            {
                GString gs = new GString(s, 1);
                return gs;
            }).ToArray().Clean();
        }
    }
}
