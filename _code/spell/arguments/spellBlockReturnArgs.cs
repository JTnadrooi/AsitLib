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
    /// Arguments passed to the custom function executed after every <see cref="SpellBlock"/>. 
    /// This <see langword="readonly"/> <see langword="struct"/> mostly contains info about that struct and a couple refrences.
    /// </summary>
    public readonly struct SpellBlockReturnArgs
    {
        /// <summary>
        /// Time taken to execute the most recent <see cref="SpellBlock"/>.
        /// </summary>
        public TimeSpan TimeTaken { get; }
        /// <summary>
        /// The parent <see cref="SpellReader"/>.
        /// </summary>
        public SpellReader Parent { get; }
        /// <summary>
        /// The name of the current <see cref="SpellCatagory"/> the <see cref="Parent"/> <see cref="SpellReader"/> is executing.
        /// </summary>
        public string CurrentCatagory { get; }
        /// <summary>
        /// The index of the current <see cref="SpellCatagory"/> the <see cref="Parent"/> <see cref="SpellReader"/> is executing.
        /// </summary>
        public int CurrentCatagoryIndex { get; }
        /// <summary>
        /// Create a new <see cref="SpellBlockReturnArgs"/> with set values.
        /// </summary>
        /// <param name="timeTaken">Time taken to execute the most recent spellblock.</param>
        /// <param name="parent">The <see cref="SpellReader"/> the most recent spellblock originates from.</param>
        /// <param name="catagoryName">The name of the currently executing <see cref="SpellCatagory"/>.</param>
        /// <param name="catagoryIndex">The index of the currently executing <see cref="SpellCatagory"/>.</param>
        public SpellBlockReturnArgs(TimeSpan timeTaken, SpellReader parent, string catagoryName, int catagoryIndex)
        {
            TimeTaken = timeTaken;
            Parent = parent;
            CurrentCatagory = catagoryName;
            CurrentCatagoryIndex = catagoryIndex;
        }
    }
}
