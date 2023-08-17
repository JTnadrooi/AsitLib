using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using static AsitLib.SpellScript.SpellUtils;
#nullable enable

namespace AsitLib.SpellScript
{
    /// <summary>
    /// A 
    /// </summary>
    public class SpellScript : ISpellScript
    {
        public SpellCatagory[] Catagories { get; }
        private readonly string full = string.Empty;
        /// <summary>
        /// The full <see cref="string"/> used to get all catagories.
        /// </summary>
        public string Full => full;
        public string GetFull() => full;
        public SpellScript(FileInfo sourceFile) : this(File.ReadAllText(sourceFile.FullName)) { }
        public SpellScript(string source)
        {
            full = source;
            Catagories = SSpellComponent.GetBlocks(full)
                .GetIndexes(s => ((SpellBlock)s).IsCatagoryHeader())
                .Concat(new int[] { SSpellComponent.GetBlocks(full).Count() })
                .ToBetweenRanges()
                .Select(r => new SpellCatagory(SSpellComponent.GetBlocks(full)[r].ToJoinedString("\n"))).ToArray();
        }
    }
}
