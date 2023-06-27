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
    public static class SSpellMemory
    {
        public static bool IsPointer(object? pointer) => pointer != null && pointer is MemoryPointer;
        public static void ToMemory(object value, MemoryPointer pointer, object[]? memory)
            => ToMemory(value, pointer.Adress, memory);
        public static void ToMemory(object value, int adress, object[]? memory)
        {
            if (memory == null) throw new ArgumentNullException("memory was null");
            try
            {
                memory[adress] = value;
            }
            catch
            {
                throw new SpellScriptMemoryException();
            }
        }
        public static object FromMemory(MemoryPointer pointer, object[]? memory)
            => FromMemory(pointer.Adress, memory);
        public static object FromMemory(int adress, object[]? memory)
        {
            if (memory == null) throw new ArgumentNullException("memory was null");
            try
            {
                return memory[adress];
            }
            catch (Exception e)
            {
                throw new SpellScriptMemoryException(e.Message + "adress: " + adress + "memory: " + memory.ToJoinedString());
            }
        }
        public static object[] ProccesPointers(IEnumerable<object> objectsWithPointers, object[]? lineMemory, object[]? funcMemory)
            => objectsWithPointers.Select(o => ProccesPointer(o, lineMemory, funcMemory)).ToArray();
        public static object ProccesPointer(object o, object[]? lineMemory, object[]? funcMemory)
        {
            if (IsPointer(o))
            {
                MemoryPointer p = new MemoryPointer(o);
                if (p.TargetMemory == MemoryTarget.FunctionMemory) return FromMemory(p, funcMemory);
                else return FromMemory(p, lineMemory);
            }
            else return o;
        }
    }
}
