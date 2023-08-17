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
    /// Two memory locations a <see cref="MemoryPointer"/> can target. Seealso <see cref="MemoryPointer.TargetMemory"/>.
    /// </summary>
    public enum MemoryTarget
    {
        /// <summary>
        /// The <see cref="SpellReader.FuncMemory"/> <see cref="Array"/>.
        /// </summary>
        FunctionMemory,
        /// <summary>
        /// The <see cref="SpellReader.LineMemory"/> <see cref="Array"/>.
        /// </summary>
        LineMemory
    }
}
