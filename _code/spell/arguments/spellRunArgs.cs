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
    /// A collection of arguments for the <see cref="ISpellInterpeter.Run(SpellRunArgs)"/> method.
    /// </summary>
    public readonly struct SpellRunArgs
    {
        /// <summary>
        /// The command that will be executed.
        /// </summary>
        public SpellCommand Command { get; }
        /// <summary>
        /// The parent reader. (Often the one creating this <see cref="SpellRunArgs"/>.)
        /// </summary>
        public ISpellExecutor? Executor { get; }
        /// <summary>
        /// Create a new <see cref="SpellRunArgs"/> with set command. (<see cref="Executor"/> will be set to <see langword="null"/>.)
        /// </summary>
        /// <param name="cmd">The <see cref="Command"/> property.</param>
        public SpellRunArgs(SpellCommand cmd) : this(cmd, null) { }
        /// <summary>
        /// Create a new <see cref="SpellRunArgs"/> with set values.
        /// </summary>
        /// <param name="cmd">The <see cref="Command"/> property.</param>
        /// <param name="parentExecutor">The <see cref="Executor"/> property. This <strong>must</strong> be a reference, not a copy.</param>
        public SpellRunArgs(SpellCommand cmd, ISpellExecutor? parentExecutor)
        {
            Command = cmd;
            Executor = parentExecutor;
        }
    }
}
