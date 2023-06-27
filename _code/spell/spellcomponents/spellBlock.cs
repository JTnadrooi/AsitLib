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
    /// <summary>
    /// "Lines" used to read spellscripts.
    /// </summary>
    public readonly struct SpellBlock
    {
        public readonly int CommandLenght => Value.Count(c => c == '|') + 1;
        /// <summary>
        /// The full value this <see cref="SpellBlock"/> holds.
        /// </summary>
        public string Value { get; }
        /// <summary>
        /// Construct a new <see cref="SpellBlock"/> with set values.
        /// </summary>
        /// <param name="value"></param>
        public SpellBlock(string value)
        {
            if (value == null) throw new ArgumentNullException("value cant be null");
            if (!value.EndsWith(';')) throw new ArgumentException("value isnt a block; doesnt end with \";\".");
            Value = value;
        }
        /// <summary>
        /// Check if this <see cref="SpellBlock"/> is the header of a <see cref="SpellCatagory"/>.
        /// </summary>
        /// <returns>A value indicating if the given <see cref="SpellBlock"/> is a <see cref="SpellCatagory"/> header.</returns>
        public readonly bool IsCatagoryHeader()
            => Value.Contains(":") && Value.Split(':')[1].StartsWith(" ");
        public readonly SpellCommand[] GetCommands(object[]? linememory, object[]? funcmemory)
            => ("|" + Value.Trim()[..^1]).Split("|", StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim().AsSpellCommand(linememory, funcmemory)).ToArray();
        public readonly SpellCommand[] GetCommands() => GetCommands(null, null);
            

        /// <summary>
        /// A <see cref="SpellBlock"/> with all values set to their defaults.
        /// </summary>
        public static SpellBlock Empty => default;
        public static explicit operator SpellBlock(string s) => new SpellBlock(s);
        public static implicit operator string(SpellBlock sb) => sb.Value;
        public override readonly string ToString() => Value;
        public bool Validate(bool layered = false, object[]? linemem = null, object[]? funcmem = null)
        {
            if (layered)
            {
                foreach (SpellCommand cmd in GetCommands(linemem, funcmem))
                    if (!cmd.Validate()) return false;
                return true;
            }
            else
            {
                if (Value.Contains("||") || Value.Contains(":::")) return false;
                return true;
            }
        }

    }
}
