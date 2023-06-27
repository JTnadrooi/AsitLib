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
    /// Represents one input parameter of a catagory.
    /// </summary>
    public readonly struct CatagoryParameterInput
    {
        /// <summary>
        /// <see cref="System.Type"/> of the input paramter.
        /// </summary>
        public readonly Type Type { get; }
        /// <summary>
        /// Index of the input parameter relative to the others.
        /// </summary>
        public readonly int Index { get; }
        /// <summary>
        /// Create a new <see cref="CatagoryParameterInput"/> from a string with set <see cref="Index"/>.
        /// </summary>
        /// <param name="str">The string to contruct a new <see cref="CatagoryParameterInput"/> struct from.</param>
        /// <exception cref="ArgumentException"></exception>
        public CatagoryParameterInput(string str)
        {
            Type? _type = Type.GetType("System." + str.Split('~')[0]);
            Index = Used.SafeIntParse(str.Split('~')[1]) - 1;
            if (Index < 0) throw new InvalidCastException("Index is invalid: " + str.Split('~')[1]);
            if (_type == null) throw new ArgumentException("Unable to load type of name System." + str.Split('~')[0]);
            else Type = _type;
        }
        /// <summary>
        /// Create a new <see cref="CatagoryParameterInput"/> with set values.
        /// </summary>
        /// <param name="name">Name of the input parameter.</param>
        /// <param name="type"><see cref="System.Type"/> of the input paramter.</param>
        /// <param name="index">Index of the input parameter relative to the others.</param>
        public CatagoryParameterInput(string name, Type type, int index)
        {
            Type = type;
            Index = index;
        }
        public override string ToString()
            => Type.ToString()[7..] + "~" + (Index + 1).ToString();
    }
}
