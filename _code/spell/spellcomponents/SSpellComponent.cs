using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using static AsitLib.SpellScript.SSpell;
#nullable enable

namespace AsitLib.SpellScript
{
    public static class SSpellComponent
    {
        /// <summary>
        /// Base of most <see cref="SSpell"/> methods. Made public because it may be usefull.
        /// </summary>
        /// <param name="input">Input <see cref="string"/>.</param>
        /// <returns>A <see cref="Array"/> of <see cref="string"/> objects representing blocks read by a spellscipt reader.</returns>
        public static SpellBlock[] GetBlocks(string input)
            => input.NormalizeLE().IgnoreComments().Replace("\n", "").Split(";").Select(s => new SpellBlock(OrganizeTabsAndSpaces(s.Replace("\n", "")) + ";")).ToArray()[..^1];
        public static SpellBlock AsSpellBlock(this string s) => new SpellBlock(s);
        public static SpellCommand AsSpellCommand(this string s) => new SpellCommand(s);
        public static SpellCommand AsSpellCommand(this string s, object[]? lineMemory, object[]? funcMemory) => new SpellCommand(s, lineMemory, funcMemory);
    }
}
