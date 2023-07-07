using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
#nullable enable
namespace AsitLib.SpellScript
{
    /// <summary>
    /// a
    /// </summary>
    public class SpellStream : ISpellExecutor
    {
        private object[]? stackmem;
        private object[]? funcmem;
        private object[]? linemem;
        public ISpellInterpeter[] interpeters;
        /// <summary>
        /// Get the stack memory <see cref="Array"/>.
        /// </summary>
        public object[] StackMemory => stackmem ?? throw new SpellScriptMemoryException("stackMemory is not initialized.");
        /// <summary>
        /// Get the function memory <see cref="Array"/>.
        /// </summary>
        public object[] FuncMemory => funcmem ?? throw new SpellScriptMemoryException("funcMemory is not initialized.");
        /// <summary>
        /// Get the line memory <see cref="Array"/>.
        /// </summary>
        public object[] LineMemory => linemem ?? throw new SpellScriptMemoryException("lineMemory is not initialized.");
        public SpellStream(int stacksize, int funcmemsize, int linememsize, params ISpellInterpeter[] interpeters)
        {
            stackmem = stacksize <= 0 ? null : new object[stacksize];
            funcmem = funcmemsize <= 0 ? null : new object[funcmemsize];
            linemem = linememsize <= 0 ? null : new object[linememsize];
            this.interpeters = interpeters.Concat(new SpellMemoryManager().ToSingleArray()).ToArray();
        }
        public IReadOnlyCollection<ISpellInterpeter> GetReadOnlyInterpeters() 
            => Array.AsReadOnly(GetInterpeters());
        public ref ISpellInterpeter[] GetInterpeters()
            => ref interpeters;
        public void Dispose() { }
        public ref object[] GetMemory(SpellMemoryAddress address)
        {
            switch (address)
            {
                case SpellMemoryAddress.StackMemoryAdr:
                    if (stackmem != null) return ref stackmem!;
                    else throw new SpellScriptMemoryException("stackMemory is not initialized.");
                case SpellMemoryAddress.LineMemoryAdr:
                    if (linemem != null) return ref linemem!;
                    else throw new SpellScriptMemoryException("lineMemory is not initialized.");
                case SpellMemoryAddress.FunctionMemoryAdr:
                    if (funcmem != null) return ref funcmem!;
                    else throw new SpellScriptMemoryException("funcMemory is not initialized.");
                default: throw new ArgumentException("Invalid address input.");
            }
        }
        public void SetAtMemory(SpellMemoryAddress address, int index, object value)
            => GetMemory(address)[index] = value;
        public void Next(string command) => Next(new SpellCommand(command, linemem, funcmem));
        public void Next(string command, IUniManipulator<string, string> manipulator, string? manipulatorArgs) 
            => Next(new SpellCommand(command, manipulator, manipulatorArgs, linemem, funcmem));
        public void Next(SpellCommand command) 
        {
            foreach (ISpellInterpeter interpeter in interpeters)
            {
                if (interpeter.Namespace == command.Namespace)
                {
                    SpellReturnArgs returnArgs = interpeter.Run(new SpellRunArgs(command, this));
                    if (returnArgs.HasReturned && command.ShouldReturnValue) SSpellMemory.ToMemory(returnArgs.ReturnValue!, command.OutPointer, linemem);
                }
                //else if (current.Namespace == null) interpeter.Run(current, linemem);
            }
        }
        public IReadOnlyCollection<object> GetReadOnlyMemory(SpellMemoryAddress address)
            => Array.AsReadOnly(GetMemory(address));
        public ref object GetAtMemory(SpellMemoryAddress address, int index)
            => ref GetMemory(address)[index];
    }
}
