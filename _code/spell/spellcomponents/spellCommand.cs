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
    /// A command that can be read by any class implementing <see cref="ISpellInterpeter"/> using <see cref="ISpellInterpeter.Run(SpellCommand)"/>.
    /// </summary>
    public readonly struct SpellCommand
    {
        /// <summary>
        /// The full value this <see cref="SpellCommand"/> holds.
        /// </summary>
        public string Value { get; }
        /// <summary>
        /// A <see cref="Array"/> of arguments inserted.
        /// </summary>
        public object[]? Arguments { get; }
        /// <summary>
        /// A <see cref="bool"/> value indicating if the containing command has input arguments.
        /// </summary>
        public readonly bool HasInputArguments => Arguments != null;
        /// <summary>
        /// Gets a <see cref="bool"/> value indicating if the containg command is expected to return a value.
        /// </summary>
        public bool ShouldReturnValue => !OutPointer.IsEmpty;
        /// <summary>
        /// Gets the location this <see cref="SpellCommand"/> objects return value will be written to.
        /// </summary>
        public readonly MemoryPointer OutPointer => new MemoryPointer(this);
        /// <summary>
        /// Gets the name of the containing command.
        /// </summary>
        public string Name => Namespace == null ? Value.Split("(")[0] : Value.Split("(")[0].Split("::")[1];
        /// <summary>
        /// Gets the namespace this command references; <see langword="null"/> of it targets all.
        /// </summary>
        public readonly string? Namespace => Value.Contains("::") && Value.Split("::", StringSplitOptions.RemoveEmptyEntries).Length == 2
             ? Value.Split("::", StringSplitOptions.RemoveEmptyEntries)[0] : null;
        /// <summary>
        /// Construct a new <see cref="SpellCommand"/> with set values and allow commands with arguments.
        /// </summary>
        /// <param name="value">String used to create a <see cref="SpellCommand"/>.</param>
        /// <param name="lineMemory">LineMemory used to extract pointers.</param>
        /// <param name="funcMemory">FucntionMemory used to extract pointers.</param>
        public SpellCommand(string value, object[]? lineMemory, object[]? funcMemory)
        {
            Value = value;
            //One line magic! (Scrapped because waaay to unreadable for the quick reader)
            //Arguments = Value.Contains("()") ? null : (Value.Between("(", ")").Contains(',') ? ProccesPointers(Value.Between("(", ")")
            //    .Split(",", StringSplitOptions.RemoveEmptyEntries)
            //    .Select(s => SpellCast(s.Trim())), lineMemory, funcMemory) : new object[] { ProccesPointer(SpellCast(Value.Between("(", ")")), lineMemory, funcMemory) });
            
            if (Value.Contains("()")) Arguments = null;
            else if (Value.Between("(", ")").Contains(',')) //poly arguments
                Arguments = SSpellMemory.ProccesPointers(Value.Between("(", ")")
                .Split(",", StringSplitOptions.RemoveEmptyEntries)
                .Select(s => SpellCast(s.Trim())), lineMemory, funcMemory);
            else Arguments = new object[] { SSpellMemory.ProccesPointer(SpellCast(Value.Between("(", ")")), lineMemory, funcMemory) };
        }
        /// <summary>
        /// Construct a new <see cref="SpellCommand"/> with set value.<br/>
        /// <i>If any of the <see cref="Arguments"/> detected has a pointer, a <see cref="ArgumentException"/> is thrown.</i>
        /// </summary>
        /// <param name="value">String used to create a <see cref="SpellCommand"/>.</param>
        public SpellCommand(string value) : this(value, Array.Empty<object>()) { }
        /// <summary>
        /// Construct a new <see cref="SpellCommand"/> with set values and allow commands with arguments.<br/>
        /// <i>Function pointed aguments will throw a <see cref="SpellScriptException"/>!</i>
        /// </summary>
        /// <param name="value">String used to create a <see cref="SpellCommand"/>.</param>
        /// <param name="lineMemory">LineMemory used to extract pointers.</param>
        public SpellCommand(string value, object[]? lineMemory) : this(value, lineMemory, Array.Empty<object>()) { }
        /// <summary>
        /// A <see cref="SpellCommand"/> with all values set to their defaults.
        /// </summary>
        public static SpellCommand Empty => default;
        public static explicit operator SpellCommand(string s) => new SpellCommand(s);
        /// <summary>
        /// Returns the <see cref="Value"/> of this <see cref="SpellCommand"/>.
        /// </summary>
        /// <returns>The <see cref="Value"/> of this <see cref="SpellCommand"/>.</returns>
        public override readonly string ToString() => Value;
        public readonly bool Validate()
        {
            return false;
        }
    }
}
