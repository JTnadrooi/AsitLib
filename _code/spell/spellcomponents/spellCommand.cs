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
    /// A command that can be read by any class implementing <see cref="ISpellInterpeter"/> using <see cref="ISpellInterpeter.Run(SpellRunArgs)"/>.
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
        public SpellCommand(string value, IUniManipulator<string, string>? manipulator = null, string? manipulatorArgs = null, object[]? lineMemory = null, object[]? funcMemory = null)
        {
            // if method returns null all hell breaks loose.
            Value = manipulator?.Maniputate(value, manipulatorArgs) ?? value;

            //One line magic! (Scrapped because waaay to unreadable for the quick reader)
            if (Value.Contains("()")) Arguments = null; //none
            else if (Value.Between("(", ")").Contains(',')) //poly
                Arguments = SSpellMemory.ProccesPointers(Value.Between("(", ")")
                .Split(",", StringSplitOptions.RemoveEmptyEntries)
                .Select(s => AsitGlobal.Cast(s.Trim(), CastMethod.SpellScript)), lineMemory, funcMemory);
            else Arguments = new object[] { SSpellMemory.ProccesPointer(AsitGlobal.Cast(Value.Between("(", ")"), CastMethod.SpellScript), lineMemory, funcMemory) }; //mono
        }
        public SpellCommand(string? nameSpace, string name, string[]? arguments, object[]? lineMemory = null, object[]? funcMemory = null)
            : this(GetFromComposition(nameSpace, name, arguments), (IUniManipulator<string, string>?)null, null, lineMemory, funcMemory) { }
        /// <summary>
        /// Returns the <see cref="Value"/> of this <see cref="SpellCommand"/>.
        /// </summary>
        /// <returns>The <see cref="Value"/> of this <see cref="SpellCommand"/>.</returns>
        public override readonly string ToString() => Value;
        public readonly bool Validate()
        {
            return false;
        }
        /// <summary>
        /// A <see cref="SpellCommand"/> with all values set to their defaults.
        /// </summary>
        public static SpellCommand Empty => default;
        public static explicit operator SpellCommand(string s) => new SpellCommand(s);
        public static string GetFromComposition(string? nameSpace, string name, string[]? arguments)
        {
            return nameSpace == null ? string.Empty : (nameSpace + "::") + name + "(" + (arguments ?? Array.Empty<string>()).ToJoinedString(", ") + ")";
        }
    }
}
