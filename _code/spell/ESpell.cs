using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
#nullable enable

namespace AsitLib.SpellScript
{
    public static class SpellExtensions
    {
        public static bool Validate(this ISpellInterpeter interpeter)
            => interpeter.Namespace.All(c => c > 64 && c < 123);
        /// <summary>
        /// Get a value from memory and cast it to the specified <see cref="Type"/>. (<typeparamref name="T"/>)<br/>
        /// If this fails, a <see cref="ArgumentException"/> is thrown.
        /// </summary>
        /// <param name="address">The memory space this value lives in.</param>
        /// <param name="index">The index of the value.</param>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <returns>The value at the specified index.</returns>
        public static T GetFromMemory<T>(this ISpellExecutor executor, SpellMemoryAddress address, Index index)
        {
            object gotten = executor.GetMemory(address)[index];
            if (gotten is T) return (T)gotten;
            else throw new ArgumentException("The object at the specified index is not convertable to the specified type: " + gotten.GetType().FullName + " to " + typeof(T).FullName);
        }
        public static bool TryGetMemory(this ISpellExecutor executor, SpellMemoryAddress address, ref object[]? memory)
        {
            try
            {
                memory = ref executor.GetMemory(address)!;
                return true;
            }
            catch (Exception)
            {
                memory = null;
                return false;
            }
        }
        public static bool TryGetReadOnlyMemory(this ISpellExecutor executor, SpellMemoryAddress address, out IReadOnlyCollection<object>? memory)
        {
            try
            {
                memory = executor.GetReadOnlyMemory(address)!;
                return true;
            }
            catch (Exception)
            {
                memory = null;
                return false;
            }
        }
        public static bool TryGetInterpeters(this ISpellExecutor executor, ref ISpellInterpeter[]? interpeters)
        {
            try
            {
                interpeters = ref executor.GetInterpeters()!;
                return true;
            }
            catch (Exception)
            {
                interpeters = null;
                return false;
            }
        }
        public static bool TryGetReadOnlyInterpeters(this ISpellExecutor executor, out IReadOnlyCollection<ISpellInterpeter>? interpeters)
        {
            try
            {
                interpeters = executor.GetReadOnlyInterpeters()!;
                return true;
            }
            catch (Exception)
            {
                interpeters = null;
                return false;
            }
        }
    }
}
