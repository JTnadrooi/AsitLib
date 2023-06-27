using AsitLib;
using AsitLib.IO;
using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;

namespace AsitLib.StringContructor.Defaults
{
    /// <summary>
    /// Collection of <see cref="GString"/> arrays that are often used in the construction of <see cref="string"/> objects through the
    /// <see cref="StringConstructorUtils.Generate(AsitContructorConfig)"/> method.
    /// <seealso cref="AsitContructorConfig"/>
    /// </summary>
    public static class ContructorArrays
    {
        /// <summary>
        /// The Modern Latin Alphabet.
        /// </summary>
        public static char[] Alphabet { get; } = new char[] {'a', 'b', 'c',
            'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z', };
        /// <summary>
        /// All the consonants of the <see cref="Alphabet"/>.<br/>
        /// <i>B, C, D..</i>
        /// </summary>
        public static char[] Consonants { get; } = new char[] { 'b', 'c',
            'd', 'f', 'g', 'h', 'j', 'k', 'l', 'm', 'n', 'p', 'q', 'r', 's', 't', 'v', 'w', 'x', 'y', 'z', };
        /// <summary>
        /// All the vowels of the <see cref="Alphabet"/>.<br/>
        /// <i>A, E, I..</i>
        /// </summary>
        public static char[] Vowels { get; } = new char[] { 'a', 'e', 'i', 'o', 'u', };
        /// <summary>
        /// List of <see cref="GString"/> structs that are often found in materials.
        /// <br/>The global <see cref="GString.Weight"/> property is set to 0.
        /// </summary>
        public static GString[] Materials { get; set; } =
        {
            new GString("..ium", 0),
            new GString("..tide", 0),
            new GString("st..", 0),
            new GString("..nuim", 0),
            new GString("..ride", 0),
            new GString("..rode", 0),
            new GString("..te", 0),
            new GString("..fe", 0),
            new GString("ther..", 0),
            new GString("..gen", 0),
            new GString("..nc", 0),
            new GString("..gon", 0),
            new GString("..th", 0),
            new GString("..ic", 0),
            new GString("..stic", 0),
            new GString("ae", 0),
            new GString("..ine", 0),
            new GString("..con", 0),
        };
        /// <summary>
        /// List of <see cref="GString"/> structs that are often found in middle-age-ish names.
        /// <br/>The global <see cref="GString.Weight"/> property is set to 0.
        /// </summary>
        public static GString[] MiddleAges { get; set; } =
        {

        };
    }
    /// <summary>
    /// A default set of Validators usefull for the creaton of <see cref="AsitContructorConfig"/>s.
    /// </summary>
    public static class Validators
    {
        public static Func<string,bool> Join(params Func<string, bool>[] source)
        {
            return (str) =>
            {
                string temp = str;
                foreach (Func<string, bool> func in source)
                    if (!func.Invoke(temp)) return false;
                return true;
            };
        }
        /// <summary>
        /// Returns a <see cref="Func{T, TResult}"/> that cancels the build if the <paramref name="max"/>(-imum) of specified <see cref="char"/> objects in a sequence is exceeded.
        /// </summary>
        /// <param name="max">Mamimum of <see cref="char"/> objects allowed in a sequence.</param>
        /// <param name="exact">If <see langword="true"/>, a sequence has to consist of same characters.</param>
        /// <param name="whitelist">Whitelist of characters.</param>
        /// <returns>A <see cref="Func{T, TResult}"/> that cancels the build if the <paramref name="max"/>(-imum) of specified <see cref="char"/> objects in a sequence is exceeded.</returns>
        public static Func<string, bool> MaxInSequence(int max, bool exact, params char[] whitelist)
        {
            if(max <= 1) return (s) => !ContructorArrays.Vowels.Any(c => s.Contains(c));
            else return (s) => StringConstructorUtils.CountSequence(s, exact, whitelist) <= max;
        }
        /// <summary>
        /// Returns a <see cref="Func{T, TResult}"/> that cancels the build if the <paramref name="max"/>(-imum) of specified <see cref="char"/> objects in a sequence is exceeded.
        /// </summary>
        /// <param name="max">Mamimum of <see cref="char"/> objects allowed in a sequence.</param>
        /// <param name="exact">If <see langword="true"/>, a sequence has to consist of same characters.</param>
        /// <returns>A <see cref="Func{T, TResult}"/> that cancels the build if the <paramref name="max"/>(-imum) of specified <see cref="char"/> objects in a sequence is exceeded.</returns>
        public static Func<string, bool> MaxInSequence(int max, bool exact)
        {
            if (max <= 1) return (s) => !ContructorArrays.Vowels.Any(c => s.Contains(c));
            else return (s) => StringConstructorUtils.CountSequence(s, exact, s.ToCharArray()) <= max;
        }
        /// <summary>
        /// Blacklist a <see cref="Array"/> of <see cref="char"/> objects from occuring in the contructed string. <br/>
        /// If any of the <see cref="char"/> objects occure in the generated <see cref="string"/>, the build is canceled.
        /// </summary>
        /// <param name="blacklist"></param>
        /// <returns>A <see cref="Func{T1, TResult}"/> that blacklists certain <see cref="char"/> objects from occuring in the generated <see cref="string"/>.</returns>
        public static Func<string, bool> Blacklist(params char[] blacklist) => (str) => !blacklist.Any(c => str.Contains(c));
        /// <summary>
        /// Blacklist a <see cref="Array"/> of <see cref="string"/> objects from occuring in the generated name. <br/>
        /// If any of the <see cref="string"/> objects occure in the generated <see cref="string"/>, the build is canceled.
        /// </summary>
        /// <param name="blacklist"></param>
        /// <returns>A <see cref="Func{T1, TResult}"/> that blacklists certain <see cref="string"/> objects from occuring in the generated <see cref="string"/>.</returns>
        public static Func<string, bool> Blacklist(params string[] blacklist) => (str) => !blacklist.Any(c => str.Contains(c));
        /// <summary>
        /// Returns a <see cref="Func{T, TResult}"/> that checks if the generated <see cref="string"/> ends with any of the blacklisted <see cref="string"/>s.
        /// </summary>
        /// <param name="blacklist"></param>
        /// <returns>A <see cref="Func{T, TResult}"/> that cancels the build if the generated <see cref="string"/> endswith any from the <paramref name="blacklist"/>.</returns>
        public static Func<string, bool> NotEndWith(params string[] blacklist) => (str) => !blacklist.Any(c => str.EndsWith(c));
        /// <summary>
        /// Returns a <see cref="Func{T, TResult}"/> that checks if the input and output differ after manipulation by the <paramref name="manipulator"/>. <br/>
        /// If <see langword="true"/>, cancels the build.
        /// </summary>
        /// <param name="manipulator">Manipulator to convert.</param>
        /// <returns>
        /// A <see cref="Func{T, TResult}"/> that checks if the input and output differ after manipulation by the <paramref name="manipulator"/>. <br/>
        /// If <see langword="true"/>, cancels the build.
        /// </returns>
        public static Func<string, bool> ToValidator(Func<string, string> manipulator) => (str) =>
        {
            if (manipulator.Invoke(str) != str) return false;
            else return true;
        };
    }
    /// <summary>
    /// A set of default Manipulators usefull for the creaton of <see cref="AsitContructorConfig"/> objects.
    /// </summary>
    public static class Manipulators
    {
        public static Func<string, string> Join(params Func<string, string>[] source)
        {
            return (str) =>
            {
                string temp = str;
                foreach (Func<string, string> func in source)
                    temp = func.Invoke(temp);
                return temp;
            };
        }
        /// <summary>
        /// <see cref="DuplicateStyle"/> for the <see cref="Duplicate(int, DuplicateStyle, string)"/> method.
        /// </summary>
        public enum DuplicateStyle
        {
            /// <summary>
            /// |<see cref="string"/>_x||connector||<see cref="string"/>_x_with_major_edit|
            /// </summary>
            Edit,
            /// <summary>
            /// |<see cref="string"/>_x||connector||<see cref="string"/>_x|
            /// </summary>
            None,
        }
        /// <summary>
        /// Removes duplicates from the generated <see cref="string"/>.
        /// </summary>
        /// <param name="count"><strong>If count is <see langword="1"/> the whole string will be removed.</strong><br/>Be carefull with high values.</param>
        /// <param name="tries">Sometimes when duplicates values are removed, they create new ones.<br/>Specify here how many times this function will loop.</param>
        /// <param name="blacklist"><see cref="Array"/> of <see cref="char"/> objects to exclude from the search.</param>
        /// <returns>A <see cref="Func{T, TResult}"/> that removes duplicates from the generated <see cref="string"/>.</returns>
        public static Func<string, string> RemoveDuplicates(int count = 2, int tries = 1, char[] blacklist = null)
        {
            blacklist ??= new char[0];
            if (count <= 0) return (str) => str;
            if (count == 1) return (str) => String.Empty;
            else return (str) =>
            {
                static string Is(string s, int _count)
                {
                    string toret = string.Empty;
                    for (int i = 0; i < _count; i++) toret += s;
                    return toret;
                }
                for (int i = 0; i < tries; i++)
                    foreach (char c in str)
                        if (!blacklist.Any(cc => cc == c)) str = str.Replace(Is(c.ToString(), count), "");
                return str;
            };
        }
        /// <summary>
        /// Duplicate the <see cref="string"/> following a specified <see cref="DuplicateStyle"/>.
        /// </summary>
        /// <param name="count">How many times the <see cref="string"/> will be duplicated.</param>
        /// <param name="style">Duplication style.</param>
        /// <param name="connector">A <see cref="string"/> that connects the duplicated <see cref="string"/> objects.</param>
        /// <returns>A <see cref="Func{T, TResult}"/> that duplicates the input <see cref="string"/> following any of the default <see cref="DuplicateStyle"/>s.</returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static Func<string, string> Duplicate(int count, DuplicateStyle style = DuplicateStyle.None, string connector = "-")
        {
            if (count <= 0) return (str) => str;
            return style switch
            {
                DuplicateStyle.Edit => (str) =>
                {
                    for (int i = 0; i < count; i++)
                        if (ContructorArrays.Vowels.Any(c => str.EndsWith(c)))
                            str += connector + str[..^1] + StringConstructorUtils.GetFromGSArray(ContructorArrays.Vowels.ToGSArray());
                        else str += connector + str;
                    return str;
                },
                DuplicateStyle.None => (str) =>
                {
                    for (int i = 0; i < count; i++) str += connector + str;
                    return str;
                },
                _ => throw new InvalidOperationException("Invalid DuplicateStyle."),
            };
        }
        /// <summary>
        /// Duplicate the <see cref="string"/> following a specified manipulator.
        /// </summary>
        /// <param name="count">How many times the <see cref="string"/> will be duplicated.</param>
        /// <param name="manipulator">Manipulator that manipulates the appended <see cref="string"/> objects.</param>
        /// <param name="connector">A <see cref="string"/> that connects the duplicated <see cref="string"/> objects.</param>
        /// <returns>A <see cref="Func{T, TResult}"/> that duplicates the input <see cref="string"/> and manipulates it with the given <paramref name="manipulator"/>.</returns>
        public static Func<string, string> Duplicate(int count, Func<string, string> manipulator, string connector = "-") => (str) =>
        {
            if (count == 0) return str;
            for (int i = 0; i < count; i++)
                str += connector + manipulator.Invoke(str);
            return str;
        };
        /// <summary>
        /// Returns a <see cref="Func{T, TResult}"/> that adds random occurences in the input <see cref="string"/>.
        /// </summary>
        /// <param name="occurence">A <see cref="string"/> that gets randomly inserted in the input <see cref="string"/>. <br/>
        /// <i>This may happen multiple times.</i></param>
        /// <param name="chance">Chance the <paramref name="occurence"/> gets inserted after each character in the input <see cref="string"/>.</param>
        /// <param name="seed">Seed of the <see cref="Random"/> deciding if the <paramref name="occurence"/> gets inserted.</param>
        /// <returns>A <see cref="Func{T, TResult}"/> that adds random occurences in the input <see cref="string"/>.</returns>
        /// <exception cref="ArgumentException"></exception>
        public static Func<string, string> AddRandom(string occurence, int chance, int seed = -1)
        {
            Random rnd;
            if (seed < 0) rnd = new Random();
            else rnd = new Random(seed);
            if (chance > 100 || chance < 0) throw new ArgumentException("Chance cannot be more than 100 or less than 0.");
            return (str) =>
            {
                string toret = String.Empty;
                foreach (char c in str)
                {
                    toret += c;
                    if (rnd.Next(100) <= chance) toret += occurence;
                }
                return toret;
            };
        }
        /// <summary>
        /// Adds a extra name with a random merger from <paramref name="mergers"/> in between.<br/>
        /// The <see cref="Random"/> used to pick a merger will be created using the <see cref="AsitContructorConfig.Seed"/> property from the first <see cref="AsitContructorConfig"/> given.<br/>
        /// <i>If the <see cref="StringConstructorUtils"/> class is used for the creation of names, this Manipulator could be usefull for
        /// adding last names.</i>
        /// </summary>
        /// <param name="mergers">A <see cref="Array"/> of mergers that could be used to merge the constructed <see cref="string"/> objects.</param>
        /// <param name="configs">
        /// <see cref="AsitContructorConfig"/>(s) used when creating the next <see cref="string"/> to merge. <br/>
        /// If any of the <paramref name="configs"/> also contain the <see cref="AddExtra(string[], AsitContructorConfig[])"/> manipulator, a
        /// <see cref="StackOverflowException"/> could be thrown.
        /// </param>
        /// <returns>A manipulator(<see cref="Func{T, TResult}"/>) that merges one or multiple constructed <see cref="string"/> objects.</returns>
        public static Func<string, string> AddExtra(string[] mergers, params AsitContructorConfig[] configs)
        {
            if (mergers.Length == 0) throw new ArgumentException("Invalid merge array, lenght is 0.");
            return (str) =>
            {
                Random rnd = new Random(configs[0].Seed);
                foreach (AsitContructorConfig config in configs)
                {
                    str += mergers[rnd.Next(0, mergers.Length)] + StringConstructorUtils.Generate(config);
                }
                return str;
            };
        }

    }
    public static class Presets
    {
        public static AsitContructorConfig ThetaloreBase => new AsitContructorConfig
        {
            Occurrences = ContructorArrays.Alphabet.ToGSArray(i => i = 1),
            Validator = (str) =>
            {
                return Validators.MaxInSequence(2, true)(str) 
                && Validators.MaxInSequence(2, false, ContructorArrays.Vowels)(str)
                && Validators.MaxInSequence(2, false, ContructorArrays.Consonants)(str);
            },

        };
    }
}
