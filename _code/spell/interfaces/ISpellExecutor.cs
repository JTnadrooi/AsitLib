using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using static AsitLib.SpellScript.SSpell;
#nullable enable

namespace AsitLib.SpellScript
{
    public interface ISpellExecutor : IDisposable
    {
        public ref object[] GetMemory(SpellMemoryAddress address);
        public ref object GetAtMemory(SpellMemoryAddress address, int index);
        public IReadOnlyCollection<object> GetReadOnlyMemory(SpellMemoryAddress address);
        public IReadOnlyCollection<ISpellInterpeter> GetReadOnlyInterpeters();
        public ref ISpellInterpeter[] GetInterpeters();
        /// <summary>
        /// Set the a value of a index in memory externaly.
        /// </summary>
        /// <param name="address">The adress indicating in what memory space the value will be set.</param>
        /// <param name="index">The index of the value; Follows the same rules as a c# array where 0 will be the first.)</param>
        /// <param name="value">The new value.</param>
        public void SetAtMemory(SpellMemoryAddress address, int index, object value);
    }
}
