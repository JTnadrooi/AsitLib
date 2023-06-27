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
    /// A interface for custom spellscipts recognizable by the <see cref="AsitLib.SpellScript"/> <see langword="namespace"/>.
    /// </summary>
    public interface ISpellScript
    {
        /// <summary>
        /// All the <see cref="SpellCatagory"/> objects detected.
        /// </summary>
        public SpellCatagory[] Catagories { get; }
        /// <summary>
        /// Get the full contents of the <see cref="ISpellScript"/> inheriting <see langword="class"/>.
        /// </summary>
        /// <returns>The full contents of the <see cref="ISpellScript"/> inheriting <see langword="class"/>.</returns>
        public string GetFull();
    }
}
