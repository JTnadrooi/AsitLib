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
    /// All different locations in the memory used to run a spellscript.
    /// </summary>
    public enum SpellMemoryAddress
    {
        /// <summary>
        /// The memory where global variables are stored.
        /// </summary>
        StackMemoryAdr,
        /// <summary>
        /// The memory where function paramters are stored.
        /// </summary>
        FunctionMemoryAdr,
        /// <summary>
        /// The memory where variables are stored that will be forgotten when the next line starts.
        /// </summary>
        LineMemoryAdr,
    }
}
