using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Diagnostics.CodeAnalysis;
#nullable enable

namespace AsitLib.SpellScript
{
    public static class SSpell
    {
        public static Dictionary<string, string> DefaultSpellEscapes
            => new Dictionary<string, string>(new KeyValuePair<string, string>[]
                {
                    new KeyValuePair<string, string>(@"\s", " "),
                    new KeyValuePair<string, string>(@"\n", "\n"),
                    new KeyValuePair<string, string>(@"\t", "\t"),
                    new KeyValuePair<string, string>(@"\dq", "\""),
                    new KeyValuePair<string, string>(@"\q", "\'"),
                });
        public static string OrganizeTabsAndSpaces(string str)
        {
            string toret = str;
            toret = RemoveDups(toret.Replace('\t', ' '), ' ');
            return toret.Trim();
        }
        public static string RemoveDups(string input, params char[] whitelist)
        {
            StringBuilder response = new StringBuilder(input.Length);
            char mem = '\0';
            for (int i = 0; i < input.Length; i++)
            {
                char current = input[i];
                if (mem == current && whitelist.Any(c => c == current))
                {

                }
                else
                {
                    mem = current;
                    response.Append(mem);
                }
            }
            return response.ToString();
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
        /// <summary>
        /// Cast a string to a spellscipt designated <see cref="object"/>.
        /// </summary>
        /// <param name="input">Input <see cref="string"/>.</param>
        /// <returns>A spellscipt designated <see cref="object"/>.</returns>
        /// <exception cref="InvalidCastException"></exception>
        public static object SpellCast(string input)
        {
            if (input.StartsWith("\"") && input.EndsWith("\"")) return ProccesEscapes(input[1..^1], DefaultSpellEscapes);
            else if (Used.SafeNullIntParse(input) != null) return int.Parse(input);
            else if (Used.SafeNullBoolParse(input) != null) return input == "false" || input == "False";
            else if (input.All(c => c == '@') || input.All(c => c == '*') || input.TrimStart('*').All(c => char.IsDigit(c)) || input.TrimStart('@').All(c => char.IsDigit(c))) return new MemoryPointer(input);
            else throw new InvalidCastException("invalid cast: <" + input + ">");
        }
        //public static object? Run(this ISpellInterpeter interpeter, SpellCommand cmd, object[]? linememory, SpellReader? parentReader)
        //{
        //    object? returned = interpeter.Run(new SpellRunArgs(cmd, parentReader)).ReturnValue;
        //    if (returned == null) return null;
        //    else SSpellMemory.ToMemory(returned, cmd.OutPointer, linememory);
        //    return returned;
        //}
        //public static object? Run(this ISpellInterpeter interpeter, SpellCommand cmd, object[]? linememory) => Run(interpeter, cmd, linememory, null);
        //public static object? Run(this ISpellInterpeter interpeter, SpellCommand cmd) => Run(interpeter, cmd, new object[0], null);
        //public static object? Run(this ISpellInterpeter interpeter, SpellCommand cmd, SpellReader parentReader) => Run(interpeter, cmd, parentReader.LineMemory, parentReader);
        public static class Debug
        {
            public static bool ValidateArgs<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10,
                P11, P12, P13, P14, P15, P16, P17>(SpellRunArgs args, bool mustHaveReader)
            {
                if (mustHaveReader && args.Executor == null) return false;
                if (args.Command.Arguments == null) return false;
                if (args.Command.Arguments.Length != 17) return false;
                if (args.Command.Arguments[0] is not P1) return false;
                if (args.Command.Arguments[1] is not P2) return false;
                if (args.Command.Arguments[2] is not P3) return false;
                if (args.Command.Arguments[3] is not P4) return false;
                if (args.Command.Arguments[4] is not P5) return false;
                if (args.Command.Arguments[5] is not P6) return false;
                if (args.Command.Arguments[6] is not P7) return false;
                if (args.Command.Arguments[7] is not P8) return false;
                if (args.Command.Arguments[8] is not P9) return false;
                if (args.Command.Arguments[9] is not P10) return false;
                if (args.Command.Arguments[10] is not P11) return false;
                if (args.Command.Arguments[11] is not P12) return false;
                if (args.Command.Arguments[12] is not P13) return false;
                if (args.Command.Arguments[13] is not P14) return false;
                if (args.Command.Arguments[14] is not P15) return false;
                if (args.Command.Arguments[15] is not P16) return false;
                if (args.Command.Arguments[16] is not P17) return false;
                return true;
            }
            public static bool ValidateArgs<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10,
                P11, P12, P13, P14, P15, P16>(SpellRunArgs args, bool mustHaveReader)
            {
                if (mustHaveReader && args.Executor == null) return false;
                if (args.Command.Arguments == null) return false;
                if (args.Command.Arguments.Length != 16) return false;
                if (args.Command.Arguments[0] is not P1) return false;
                if (args.Command.Arguments[1] is not P2) return false;
                if (args.Command.Arguments[2] is not P3) return false;
                if (args.Command.Arguments[3] is not P4) return false;
                if (args.Command.Arguments[4] is not P5) return false;
                if (args.Command.Arguments[5] is not P6) return false;
                if (args.Command.Arguments[6] is not P7) return false;
                if (args.Command.Arguments[7] is not P8) return false;
                if (args.Command.Arguments[8] is not P9) return false;
                if (args.Command.Arguments[9] is not P10) return false;
                if (args.Command.Arguments[10] is not P11) return false;
                if (args.Command.Arguments[11] is not P12) return false;
                if (args.Command.Arguments[12] is not P13) return false;
                if (args.Command.Arguments[13] is not P14) return false;
                if (args.Command.Arguments[14] is not P15) return false;
                if (args.Command.Arguments[15] is not P16) return false;
                return true;
            }
            public static bool ValidateArgs<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10,
                P11, P12, P13, P14, P15>(SpellRunArgs args, bool mustHaveReader)
            {
                if (mustHaveReader && args.Executor == null) return false;
                if (args.Command.Arguments == null) return false;
                if (args.Command.Arguments.Length != 15) return false;
                if (args.Command.Arguments[0] is not P1) return false;
                if (args.Command.Arguments[1] is not P2) return false;
                if (args.Command.Arguments[2] is not P3) return false;
                if (args.Command.Arguments[3] is not P4) return false;
                if (args.Command.Arguments[4] is not P5) return false;
                if (args.Command.Arguments[5] is not P6) return false;
                if (args.Command.Arguments[6] is not P7) return false;
                if (args.Command.Arguments[7] is not P8) return false;
                if (args.Command.Arguments[8] is not P9) return false;
                if (args.Command.Arguments[9] is not P10) return false;
                if (args.Command.Arguments[10] is not P11) return false;
                if (args.Command.Arguments[11] is not P12) return false;
                if (args.Command.Arguments[12] is not P13) return false;
                if (args.Command.Arguments[13] is not P14) return false;
                if (args.Command.Arguments[14] is not P15) return false;
                return true;
            }
            public static bool ValidateArgs<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10,
                P11, P12, P13, P14>(SpellRunArgs args, bool mustHaveReader)
            {
                if (mustHaveReader && args.Executor == null) return false;
                if (args.Command.Arguments == null) return false;
                if (args.Command.Arguments.Length != 14) return false;
                if (args.Command.Arguments[0] is not P1) return false;
                if (args.Command.Arguments[1] is not P2) return false;
                if (args.Command.Arguments[2] is not P3) return false;
                if (args.Command.Arguments[3] is not P4) return false;
                if (args.Command.Arguments[4] is not P5) return false;
                if (args.Command.Arguments[5] is not P6) return false;
                if (args.Command.Arguments[6] is not P7) return false;
                if (args.Command.Arguments[7] is not P8) return false;
                if (args.Command.Arguments[8] is not P9) return false;
                if (args.Command.Arguments[9] is not P10) return false;
                if (args.Command.Arguments[10] is not P11) return false;
                if (args.Command.Arguments[11] is not P12) return false;
                if (args.Command.Arguments[12] is not P13) return false;
                if (args.Command.Arguments[13] is not P14) return false;
                return true;
            }
            public static bool ValidateArgs<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10,
                P11, P12, P13>(SpellRunArgs args, bool mustHaveReader)
            {
                if (mustHaveReader && args.Executor == null) return false;
                if (args.Command.Arguments == null) return false;
                if (args.Command.Arguments.Length != 13) return false;
                if (args.Command.Arguments[0] is not P1) return false;
                if (args.Command.Arguments[1] is not P2) return false;
                if (args.Command.Arguments[2] is not P3) return false;
                if (args.Command.Arguments[3] is not P4) return false;
                if (args.Command.Arguments[4] is not P5) return false;
                if (args.Command.Arguments[5] is not P6) return false;
                if (args.Command.Arguments[6] is not P7) return false;
                if (args.Command.Arguments[7] is not P8) return false;
                if (args.Command.Arguments[8] is not P9) return false;
                if (args.Command.Arguments[9] is not P10) return false;
                if (args.Command.Arguments[10] is not P11) return false;
                if (args.Command.Arguments[11] is not P12) return false;
                if (args.Command.Arguments[12] is not P13) return false;
                return true;
            }
            public static bool ValidateArgs<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10,
                P11, P12>(SpellRunArgs args, bool mustHaveReader)
            {
                if (mustHaveReader && args.Executor == null) return false;
                if (args.Command.Arguments == null) return false;
                if (args.Command.Arguments.Length != 12) return false;
                if (args.Command.Arguments[0] is not P1) return false;
                if (args.Command.Arguments[1] is not P2) return false;
                if (args.Command.Arguments[2] is not P3) return false;
                if (args.Command.Arguments[3] is not P4) return false;
                if (args.Command.Arguments[4] is not P5) return false;
                if (args.Command.Arguments[5] is not P6) return false;
                if (args.Command.Arguments[6] is not P7) return false;
                if (args.Command.Arguments[7] is not P8) return false;
                if (args.Command.Arguments[8] is not P9) return false;
                if (args.Command.Arguments[9] is not P10) return false;
                if (args.Command.Arguments[10] is not P11) return false;
                if (args.Command.Arguments[11] is not P12) return false;
                return true;
            }
            public static bool ValidateArgs<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10,
                P11>(SpellRunArgs args, bool mustHaveReader)
            {
                if (mustHaveReader && args.Executor == null) return false;
                if (args.Command.Arguments == null) return false;
                if (args.Command.Arguments.Length != 11) return false;
                if (args.Command.Arguments[0] is not P1) return false;
                if (args.Command.Arguments[1] is not P2) return false;
                if (args.Command.Arguments[2] is not P3) return false;
                if (args.Command.Arguments[3] is not P4) return false;
                if (args.Command.Arguments[4] is not P5) return false;
                if (args.Command.Arguments[5] is not P6) return false;
                if (args.Command.Arguments[6] is not P7) return false;
                if (args.Command.Arguments[7] is not P8) return false;
                if (args.Command.Arguments[8] is not P9) return false;
                if (args.Command.Arguments[9] is not P10) return false;
                if (args.Command.Arguments[10] is not P11) return false;
                return true;
            }
            public static bool ValidateArgs<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10>(SpellRunArgs args, bool mustHaveReader)
            {
                if (mustHaveReader && args.Executor == null) return false;
                if (args.Command.Arguments == null) return false;
                if (args.Command.Arguments.Length != 10) return false;
                if (args.Command.Arguments[0] is not P1) return false;
                if (args.Command.Arguments[1] is not P2) return false;
                if (args.Command.Arguments[2] is not P3) return false;
                if (args.Command.Arguments[3] is not P4) return false;
                if (args.Command.Arguments[4] is not P5) return false;
                if (args.Command.Arguments[5] is not P6) return false;
                if (args.Command.Arguments[6] is not P7) return false;
                if (args.Command.Arguments[7] is not P8) return false;
                if (args.Command.Arguments[8] is not P9) return false;
                if (args.Command.Arguments[9] is not P10) return false;
                return true;
            }
            public static bool ValidateArgs<P1, P2, P3, P4, P5, P6, P7, P8, P9>(SpellRunArgs args, bool mustHaveReader)
            {
                if (mustHaveReader && args.Executor == null) return false;
                if (args.Command.Arguments == null) return false;
                if (args.Command.Arguments.Length != 9) return false;
                if (args.Command.Arguments[0] is not P1) return false;
                if (args.Command.Arguments[1] is not P2) return false;
                if (args.Command.Arguments[2] is not P3) return false;
                if (args.Command.Arguments[3] is not P4) return false;
                if (args.Command.Arguments[4] is not P5) return false;
                if (args.Command.Arguments[5] is not P6) return false;
                if (args.Command.Arguments[6] is not P7) return false;
                if (args.Command.Arguments[7] is not P8) return false;
                if (args.Command.Arguments[8] is not P9) return false;
                return true;
            }
            public static bool ValidateArgs<P1, P2, P3, P4, P5, P6, P7, P8>(SpellRunArgs args, bool mustHaveReader)
            {
                if (mustHaveReader && args.Executor == null) return false;
                if (args.Command.Arguments == null) return false;
                if (args.Command.Arguments.Length != 8) return false;
                if (args.Command.Arguments[0] is not P1) return false;
                if (args.Command.Arguments[1] is not P2) return false;
                if (args.Command.Arguments[2] is not P3) return false;
                if (args.Command.Arguments[3] is not P4) return false;
                if (args.Command.Arguments[4] is not P5) return false;
                if (args.Command.Arguments[5] is not P6) return false;
                if (args.Command.Arguments[6] is not P7) return false;
                if (args.Command.Arguments[7] is not P8) return false;
                return true;
            }
            public static bool ValidateArgs<P1, P2, P3, P4, P5, P6, P7>(SpellRunArgs args, bool mustHaveReader)
            {
                if (mustHaveReader && args.Executor == null) return false;
                if (args.Command.Arguments == null) return false;
                if (args.Command.Arguments.Length != 7) return false;
                if (args.Command.Arguments[0] is not P1) return false;
                if (args.Command.Arguments[1] is not P2) return false;
                if (args.Command.Arguments[2] is not P3) return false;
                if (args.Command.Arguments[3] is not P4) return false;
                if (args.Command.Arguments[4] is not P5) return false;
                if (args.Command.Arguments[5] is not P6) return false;
                if (args.Command.Arguments[6] is not P7) return false;
                return true;
            }
            public static bool ValidateArgs<P1, P2, P3, P4, P5, P6>(SpellRunArgs args, bool mustHaveReader)
            {
                if (mustHaveReader && args.Executor == null) return false;
                if (args.Command.Arguments == null) return false;
                if (args.Command.Arguments.Length != 6) return false;
                if (args.Command.Arguments[0] is not P1) return false;
                if (args.Command.Arguments[1] is not P2) return false;
                if (args.Command.Arguments[2] is not P3) return false;
                if (args.Command.Arguments[3] is not P4) return false;
                if (args.Command.Arguments[4] is not P5) return false;
                if (args.Command.Arguments[5] is not P6) return false;
                return true;
            }
            public static bool ValidateArgs<P1, P2, P3, P4, P5>(SpellRunArgs args, bool mustHaveReader)
            {
                if (mustHaveReader && args.Executor == null) return false;
                if (args.Command.Arguments == null) return false;
                if (args.Command.Arguments.Length != 5) return false;
                if (args.Command.Arguments[0] is not P1) return false;
                if (args.Command.Arguments[1] is not P2) return false;
                if (args.Command.Arguments[2] is not P3) return false;
                if (args.Command.Arguments[3] is not P4) return false;
                if (args.Command.Arguments[4] is not P5) return false;
                return true;
            }
            public static bool ValidateArgs<P1, P2, P3, P4>(SpellRunArgs args, bool mustHaveReader)
            {
                if (mustHaveReader && args.Executor == null) return false;
                if (args.Command.Arguments == null) return false;
                if (args.Command.Arguments.Length != 4) return false;
                if (args.Command.Arguments[0] is not P1) return false;
                if (args.Command.Arguments[1] is not P2) return false;
                if (args.Command.Arguments[2] is not P3) return false;
                if (args.Command.Arguments[3] is not P4) return false;
                return true;
            }
            public static bool ValidateArgs<P1, P2, P3>(SpellRunArgs args, bool mustHaveReader)
            {
                if (mustHaveReader && args.Executor == null) return false;
                if (args.Command.Arguments == null) return false;
                if (args.Command.Arguments.Length != 3) return false;
                if (args.Command.Arguments[0] is not P1) return false;
                if (args.Command.Arguments[1] is not P2) return false;
                if (args.Command.Arguments[2] is not P3) return false;
                return true;
            }
            public static bool ValidateArgs<P1, P2>(SpellRunArgs args, bool mustHaveReader)
            {
                if (mustHaveReader && args.Executor == null) return false;
                if (args.Command.Arguments == null) return false;
                if (args.Command.Arguments.Length != 2) return false;
                if (args.Command.Arguments[0] is not P1) return false;
                if (args.Command.Arguments[1] is not P2) return false;
                return true;
            }
            public static bool ValidateArgs<P1>(SpellRunArgs args, bool mustHaveReader)
            {
                if (mustHaveReader && args.Executor == null) return false;
                if (args.Command.Arguments == null) return false;
                if (args.Command.Arguments.Length != 1) return false;
                if (args.Command.Arguments[0] is not P1) return false;
                return true;
            }
            public static bool ValidateArgs(SpellRunArgs args, bool mustHaveReader)
            {
                if (mustHaveReader && args.Executor == null) return false;
                if (args.Command.Arguments == null) return true;
                if (args.Command.Arguments.Length != 0) return false;
                return true;
            }
        }
    }
}
