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
    /// A spellscript catagory.
    /// </summary>
    public readonly struct SpellCatagory
    {
        /// <summary>
        /// Name of this <see cref="SpellCatagory"/>.
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// <see cref="Array"/> of arguments passed to this <see cref="SpellCatagory"/>.
        /// </summary>
        public CatagoryParameterInput[]? Arguments { get; }
        /// <summary>
        /// Namespace this <see cref="SpellCatagory"/> tagets.
        /// </summary>
        public string? TargetNamespace { get; }
        /// <summary>
        /// <see cref="Array"/> of blocks this <see cref="SpellCatagory"/> contains. <br/>
        /// <strong>Includes the initial <see cref="Header"/>.</strong>
        /// </summary>
        public SpellBlock[] Blocks { get; }
        /// <summary>
        /// <see cref="Array"/> of codeblocks this <see cref="SpellCatagory"/> contains.
        /// </summary>
        public readonly SpellBlock[] CodeBlocks => Blocks[1..];
        /// <summary>
        /// The expected return type of this <see cref="SpellCatagory"/> or <see langword="null"/> if "void" is expected.
        /// </summary>
        public Type? ReturnType { get; }
        /// <summary>
        /// Full content.
        /// </summary>
        public string Content { get; }
        /// <summary>
        /// The header of the <see cref="SpellCatagory"/>. (First <see cref="SpellBlock"/>)
        /// </summary>
        public string Header { get; }
        /// <summary>
        /// Create a new <see cref="SpellCatagory"/> with set content.
        /// </summary>
        /// <param name="content">Content this <see cref="SpellCatagory"/> will use to set all its values.</param>
        /// <exception cref="InvalidCastException"></exception>
        public SpellCatagory(string content)
        {
            //Content from content but wthout the whitespaces.
            Content = content.Trim();

            //Get blocks.
            Blocks = SSpellComponent.GetBlocks(Content);

            //Get the header from the previously set block array.
            Header = Blocks[0];

            //Check if header is invalid and throw error if it is the case.
            if (!Header.AsSpellBlock().IsCatagoryHeader()) throw new InvalidCastException("Invalid content, can't create Spellcatagory from it. Reason; invalid header.");
            
            //get return type of this catagory.
            if (Header.Split(": ")[0] == "void") ReturnType = null;
            else ReturnType = Type.GetType("System." + Header.Split(": ")[0]) ?? throw new TypeLoadException("Invalid type of name: " + Header.Split(": ")[0]);

            //Extract the name.
            Name = Header.Split('(')[0][(Header.Split(": ")[0].Length + 2)..];

            //Extract the targetnamespace.
            if (Name.Contains("::"))
            {
                TargetNamespace = Name.Split("::")[0];
                Name = Name.Split("::")[1];
            }
            else TargetNamespace = null;

            //Extract agruments.
            if (Header.Contains("();")) Arguments = null;
            else
            {
                string argumentstemp = Header.Between("(", ")").Replace(" ", "");
                Arguments = argumentstemp.Split(",").Select(a => new CatagoryParameterInput(a)).ToArray();
            }
            //string headertemp = Header.Between("(", ")");
            //Arguments = headertemp.GetIndexes(c => c == '\"').ToBetweenRanges()
            //        .Where(r => Array.IndexOf(headertemp.GetIndexes(c => c == '\"').ToBetweenRanges(), r) % 2 == 0)
            //        .Select(r => headertemp[r] + "\"")
            //        .Select(s => SpellUtils.SpellCast(s)).ToArray();
        }
        /// <summary>
        /// Returns the <see cref="Header"/> of this initialization.
        /// </summary>
        /// <returns>The <see cref="Header"/> of this initialization.</returns>
        public override readonly string ToString() => Header;
        public readonly bool Validate(bool layered = false)
        {
            {

            }
            if (layered)
                foreach (SpellBlock spellBlock in this.CodeBlocks)
                    if (!spellBlock.Validate()) return false;
            return true;
        }
        /// <summary>
        /// A <see cref="SpellCatagory"/> with all values set to their defaults.
        /// </summary>
        public static SpellCatagory Empty => default;
    }
}
