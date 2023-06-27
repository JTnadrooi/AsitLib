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
    /// A struct containig information about a location in memory. 
    /// (Seealso: <see cref="SpellReader.StackMemory"/>, <see cref="SpellReader.LineMemory"/> and <see cref="SpellReader.FuncMemory"/>)
    /// </summary>
    public readonly struct MemoryPointer
    {
        /// <summary>
        /// The memory space this <see cref="MemoryPointer"/> targets.<br/>
        /// <i>Seealso <see cref="SpellReader.LineMemory"/> and <see cref="SpellReader.FuncMemory"/>.</i>
        /// </summary>
        public MemoryTarget TargetMemory { get; }
        private readonly int? adress;
        /// <summary>
        /// The location in memory this pointer points to.
        /// </summary>
        public int Adress => IsEmpty ? throw new Exception("This is a empty pointer.") : adress!.Value;
        /// <summary>
        /// A value indicating if this <see cref="MemoryPointer"/> is empty.
        /// </summary>
        public readonly bool IsEmpty => adress == null || adress < 0;
        /// <summary>
        /// Create a new <see cref="MemoryPointer"/> with a set adress. If a <paramref name="_adress"/> of zero or below is given, a <see cref="Empty"/> pointer is created.
        /// </summary>
        /// <param name="_adress">The <see cref="Adress"/> property of the new <see cref="MemoryPointer"/>.</param>
        /// <param name="targetMemory">
        /// The memory space this <see cref="MemoryPointer"/> targets.<br/>
        /// <i>Seealso <see cref="SpellReader.LineMemory"/> and <see cref="SpellReader.FuncMemory"/>.</i>
        /// </param>
        public MemoryPointer(int _adress, MemoryTarget targetMemory)
        {
            adress = _adress;
            TargetMemory = targetMemory;
        }
        /// <summary>
        /// Create a new <see cref="MemoryPointer"/> from a boxed <see cref="object"/>. The object must be of <see cref="MemoryPointer"/> type.<br/>
        /// <i>Seealso: <see cref="SSpell.SpellCast(string)"/></i>
        /// </summary>
        /// <param name="boxedpointer">A boxed object of <see cref="MemoryPointer"/> type to extract the <see cref="Adress"/> from.</param>
        /// <exception cref="InvalidCastException"></exception>
        public MemoryPointer(object boxedpointer)
        {
            if (!SSpellMemory.IsPointer(boxedpointer)) throw new InvalidCastException("pointer isnt of memorypointer type.");
            adress = ((MemoryPointer)boxedpointer).Adress;
            TargetMemory = ((MemoryPointer)boxedpointer).TargetMemory;
        }
        /// <summary>
        /// Create a new <see cref="MemoryPointer"/> from a string. Accepted syntax: <strong>"***"</strong> or alternate; <strong>"*3"</strong>.
        /// </summary>
        /// <param name="pointer">The string to extract a <see cref="MemoryPointer"/> from.</param>
        /// <exception cref="InvalidOperationException"></exception>
        public MemoryPointer(string pointer)
        {
            if (pointer.Any(c => char.IsLetter(c))) throw new InvalidOperationException("Invalid pointer: " + pointer);
            try
            {
                if (pointer.Contains('*'))
                {
                    if (pointer.Any(c => char.IsDigit(c)))
                        adress = pointer.Count(c => c == '*') * int.Parse(pointer.Replace("*", "")) - 1;
                    else adress = (pointer.Length - pointer.TrimEnd('*').Length) - 1;
                    TargetMemory = MemoryTarget.LineMemory;
                }
                else if (pointer.Contains('@'))
                {
                    if (pointer.Any(c => char.IsDigit(c)))
                        adress = pointer.Count(c => c == '@') * int.Parse(pointer.Replace("@", "")) - 1;
                    else adress = (pointer.Length - pointer.TrimEnd('@').Length) - 1;
                    TargetMemory = MemoryTarget.FunctionMemory;
                }
                else throw new InvalidOperationException("Invalid pointer: " + pointer);
            }
            catch (FormatException)
            {
                throw new FormatException("Input pointer (\"" + pointer + "\") is invalid.");
            }
        }
        /// <summary>
        /// Extract the <see cref="MemoryPointer"/> from a <see cref="SpellCommand"/>. <br/>
        /// <i>You could also use <see cref="SpellCommand.OutPointer"/> to get the same return value.</i>
        /// </summary>
        /// <param name="command">The <see cref="SpellCommand"/> to extract a <see cref="MemoryPointer"/> from.</param>
        /// <exception cref="InvalidOperationException"></exception>
        public MemoryPointer(SpellCommand command) : this(command.Value.Split(")").Last()) { }
        /// <summary>
        /// A empty <see cref="MemoryPointer"/> with the adress set to -1. The returned pointer objects <see cref="IsEmpty"/> propert will always return <see langword="false"/>.
        /// </summary>
        public static MemoryPointer Empty => new MemoryPointer(-1, MemoryTarget.LineMemory);
        /// <summary>
        /// Returns the <see cref="Adress"/> property as a <see cref="string"/> or "Null" if the adress is <see langword="null"/>.
        /// </summary>
        /// <returns>The <see cref="Adress"/> property as a <see cref="string"/> or "Null" if the adress is <see langword="null"/>.</returns>
        public override readonly string ToString() => adress == null ? "Null" : ("adress: {" + adress.Value.ToString() + "} target: " + TargetMemory.ToString());
        public static explicit operator int(MemoryPointer pointer) => pointer.Adress;
        public static explicit operator Index(MemoryPointer pointer) => new Index(pointer.Adress);
    }
}
