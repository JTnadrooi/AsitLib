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
    /// Interface for implementing a method that can execute SpellScripts. <br/>
    /// </summary>
    public interface ISpellInterpeter
    {
        /// <summary>
        /// Run a command from a SpellScript.
        /// </summary>
        /// <param name="args">
        /// Arguments needed to run a command. A <see cref="SpellRunArgs"/> <see langword="struct"/> contains a 
        /// <see cref="SpellCommand"/> that this class will run and a reference to the parent <see cref="SpellReader"/>.
        /// </param>
        /// <returns>The return value of the command corresponding to the given <see cref="SpellCommand"/>. 
        /// Should return <see langword="null"/> when the processed command doesn't return a value.</returns>
        [return: MaybeNull]
        public SpellReturnArgs Run([DisallowNull] SpellRunArgs args);
        /// <summary>
        /// Namepace of this <see cref="ISpellInterpeter"/>. 
        /// Return <see langword="null"/> to get all commands instead of only the ones that target this <see cref="Namespace"/>.
        /// </summary>
        [MaybeNull]
        public string? Namespace { get; }
    }
}
