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
    /// A struct reprecenting the current index of a <see cref="SpellReader"/>.
    /// </summary>
    public struct SpellReaderIndex
    {
        /// <summary>
        /// The index of the <see cref="SpellBlock"/> the <see cref="SpellReader"/> has last read.
        /// </summary>
        public int BlockIndex { get; set; }
        /// <summary>
        /// The index of the <see cref="SpellCommand"/> the <see cref="SpellReader"/> has last read.
        /// </summary>
        public int CmdIndex { get; set; }
        /// <summary>
        /// Get if this <see cref="SpellReaderIndex"/> is finished.
        /// </summary>
        public bool IsFinished { get; private set; }
        /// <summary>
        /// Create a new <see cref="SpellReaderIndex"/> with set values.
        /// </summary>
        /// <param name="blockIndex">The index of the <see cref="SpellBlock"/> the <see cref="SpellReader"/> has last read.</param>
        /// <param name="cmdIndex">The index of the <see cref="SpellCommand"/> the <see cref="SpellReader"/> has last read.</param>
        public SpellReaderIndex(int blockIndex, int cmdIndex)
        {
            BlockIndex = blockIndex;
            CmdIndex = cmdIndex;
            IsFinished = false;
        }
        /// <summary>
        /// Increase the global index by one. This accounts for all variables.
        /// </summary>
        /// <param name="catagory">The <see cref="SpellCatagory"/> objects this <see cref="SpellReaderIndex"/> uses to increment the position.</param>
        /// <returns>A value indicating if this <see cref="SpellReaderIndex"/> is finished. (<see cref="IsFinished"/>)</returns>
        /// <exception cref="InvalidOperationException"></exception>
        internal bool Next(SpellCatagory catagory)
        {
            CmdIndex++;
            //Console.WriteLine(CmdIndex + ":cmd:" + catagories[CatagoryIndex].CodeBlocks[BlockIndex].CommandLenght);
            if (IsFinished) throw new InvalidOperationException("Can't count past finished state.");
            if (catagory.CodeBlocks[BlockIndex].CommandLenght == CmdIndex)
            {
                CmdIndex = 0;
                //Console.WriteLine(BlockIndex + ":bli:" + catagories[CatagoryIndex].CodeBlocks.Length);
                BlockIndex++;
                //Console.WriteLine(BlockIndex + ":bli:" + catagories[CatagoryIndex].CodeBlocks.Length);
                if (catagory.CodeBlocks.Length == BlockIndex) //WHAT
                {
                    BlockIndex = 0;
                    CmdIndex = 0;
                    IsFinished = true;
                }
            }
            //Console.WriteLine(this);
            //Console.WriteLine("cur(AFTER): " + catagories[CatagoryIndex].CodeBlocks[BlockIndex]);
            return IsFinished;
        }
        public void Reset()
        {
            BlockIndex = 0;
            CmdIndex = 0;
        }
        public override readonly string ToString()
            => "blockIndex: {" + BlockIndex + "}, cmdIndex: {" + CmdIndex + "}";

    }
    /// <summary>
    /// A class that reads spellscrips and .spl files. This class cannot be inherited.
    /// </summary>
    public sealed class SpellReader : ISpellExecutor
    {
        public SpellCatagory[] Catagories => SpellScript.Catagories;

        private object[]? stackmem;
        private object[]? funcmem;
        private ISpellInterpeter[] interpeters;
        private object[]? linemem;
        private SpellReaderIndex index;

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
        /// <summary>
        /// Current reader location.
        /// </summary>
        public SpellReaderIndex Index { get => index; private set => index = value; }
        public SpellScript SpellScript { get; }
        public SpellReturnArgs Run(Action<SpellBlockReturnArgs>? onBlockFinish = null, string entryPoint = "main", string? entryPointNamespace = null, object[]? arguments = null)
        {
            //cleaning input
            arguments = arguments?.Length == null || arguments?.Length == 0 ? null : arguments;
            
            //Getting start / entry catagory.
            if(!SpellScript.Catagories.Any(catagory => catagory.Name == entryPoint)) throw new EntryPointNotFoundException("There is no catagory called " + entryPoint );            
            SpellCatagory start = this.SpellScript.Catagories.Where(c => c.Name == entryPoint && c.TargetNamespace == entryPointNamespace).First();

            //Checking and casting function memory.
            if(arguments != null)
            {
                
                if (funcmem == null) throw new SpellScriptMemoryException("Funcmem is null");
                KeyValuePair<CatagoryParameterInput, object>[] keyValuePairs = AsitLibStatic.Merge(start.Arguments!, arguments!);
                int i = 0;
                foreach(KeyValuePair<CatagoryParameterInput, object> pair in keyValuePairs)
                {
                    funcmem[i] = Convert.ChangeType(pair.Value, pair.Key.Type);
                    i++;
                }
            }

            //Looping through the commands.
            while (true) 
            {
                //check if finished.
                if (Index.IsFinished) return SpellReturnArgs.NullSucces;

                //if new block, clear linemem.
                if (Index.CmdIndex == 0) Clear(ref linemem);

                //Getting current command.
                //Console.WriteLine(linemem == null);
                //Console.WriteLine(funcmem == null);
                SpellCommand current = start.CodeBlocks[Index.BlockIndex].GetCommands(linemem, funcmem)[Index.CmdIndex];
                Console.WriteLine("Proccesing command: " + current);

                //The return function.
                if(current.Namespace == "&" && current.Name == "return")
                {
                    Console.WriteLine("Returning something:: " + current.Arguments.First());
                    
                    //It returns a object.
                    if(SSpell.Debug.ValidateArgs<object>(new SpellRunArgs(current, this), true))
                    {
                        Console.WriteLine("Returning something. A");
                        if (start.ReturnType == null)
                            throw new SpellScriptException("Return command error. Invalid return type. : " + current);
                        if (current.Arguments![0].GetType().Name != start.ReturnType.Name)
                            throw new SpellScriptException("Return command error. Invalid return type. : " + current);
                        return SpellReturnArgs.GetFromObject(current.Arguments[0]);
                    }
                    //It returns void.
                    else if(SSpell.Debug.ValidateArgs(new SpellRunArgs(current, this), true))
                    {
                        Console.WriteLine("Returning something. B");
                        if (start.ReturnType != null)
                            throw new SpellScriptException("Return command error. Invalid return type. : " + current);
                        return SpellReturnArgs.NullSucces;
                    }
                    else throw new SpellScriptException("Return command error. : " + current);
                }
                //Private functions, user USER defined.
                else if (current.Namespace == "@")
                {
                    if (SpellScript.Catagories.Where(c => c.Name == current.Name).Count() == 1)
                    {
                        //backup values that will be changed.
                        object[]? funccopy = funcmem?.Copy();
                        //same as object[]? funccopy = funcmem == null ? null : funcmem.Copy();

                        SpellReaderIndex indexcopy = index;
                        index.Reset();

                        Console.WriteLine(start.Name + " reset: " + index);

                        SpellReturnArgs returnArgs = Run(onBlockFinish, current.Name, null, current.Arguments);
                        if (returnArgs.HasReturned && current.ShouldReturnValue) SSpellMemory.ToMemory(returnArgs.ReturnValue!, current.OutPointer, linemem);

                        Console.WriteLine("casted out (" + returnArgs.ReturnValue!.ToString() + ") to" + current.OutPointer);

                        if (funccopy == null) funcmem = null;
                        else funcmem = funccopy.Copy();
                        index = indexcopy;
                    }
                    else throw new SpellScriptException(current.Name + " isn't a known function or there are more than one functions with the same name..");
                }
                else // Looping through all interpeters.
                    foreach (ISpellInterpeter interpeter in interpeters)
                    {
                        if (interpeter.Namespace == current.Namespace)
                        {
                            SpellReturnArgs returnArgs = interpeter.Run(new SpellRunArgs(current, this));
                            if (returnArgs.HasReturned && current.ShouldReturnValue) SSpellMemory.ToMemory(returnArgs.ReturnValue!, current.OutPointer, linemem);
                        }
                        //else if (current.Namespace == null) interpeter.Run(current, linemem);
                    }

                //Advance index position by one.
                index.Next(start);
                onBlockFinish?.Invoke(new SpellBlockReturnArgs());
            }


        }
        
        public async Task<SpellReturnArgs> RunAsync(Action<SpellBlockReturnArgs>? onBlockFinish = null, string entryPoint = "main", string? entryPointNamespace = null, params object[] arguments)
            => await Task.Run(() =>
            {
                return Run(onBlockFinish, entryPoint, entryPointNamespace, arguments);
            });
        /// <summary>
        /// Create a new <see cref="SpellReader"/> from a specified <paramref name="source"/> <see cref="Stream"/> and with set values.
        /// </summary>
        /// <param name="source">The <see cref="Stream"/> used to read the spellscript in the specified <paramref name="encoding"/>.</param>
        /// <param name="encoding">The encoding used to read the given <paramref name="source"/> <see cref="Stream"/>.</param>
        /// <param name="stacksize">The size of the <see cref="StackMemory"/> <see cref="Array"/>.</param>
        /// <param name="funcmemsize">The size of the <see cref="FuncMemory"/> <see cref="Array"/>.</param>
        /// <param name="linememsize">The size of the <see cref="LineMemory"/> <see cref="Array"/>.</param>
        /// <param name="interpeters">
        /// An <see cref="Array"/> of <see cref="ISpellInterpeter"/> implementing objects used for execution.<br/>
        /// <i>If there are no given, a <see cref="InvalidOperationException"/> is thrown.</i>
        /// </param>
        public SpellReader(Stream source, Encoding encoding, int stacksize, int funcmemsize, int linememsize, params ISpellInterpeter[] interpeters)
        {
            if (interpeters.Length <= 0) throw new InvalidOperationException("Interperter Array lenght cannot be 0.");
            Index = new SpellReaderIndex();
            source.Position = 0;
            using StreamReader reader = new StreamReader(source, encoding);
            SpellScript = new SpellScript(reader.ReadToEnd());


            stackmem = stacksize <= 0 ? null : new object[stacksize];
            funcmem = funcmemsize <= 0 ? null : new object[funcmemsize];
            linemem = linememsize <= 0 ? null : new object[linememsize];

            //Console.WriteLine(linemem == null);
            //Console.WriteLine(funcmem == null);

            this.interpeters = interpeters.Concat(new ISpellInterpeter[] { new SpellMemoryManager(), }).ToArray();
        }
        /// <summary>
        /// Create a new <see cref="SpellReader"/> from a specified <paramref name="source"/> <see cref="Stream"/> and with set values.
        /// </summary>
        /// <param name="source">The <see cref="Stream"/> used to read the spellscript in <see cref="Encoding.UTF8"/> encoding.</param>
        /// <param name="stacksize">The size of the <see cref="StackMemory"/> <see cref="Array"/>.</param>
        /// <param name="funcmemsize">The size of the <see cref="FuncMemory"/> <see cref="Array"/>.</param>
        /// <param name="linememsize">The size of the <see cref="LineMemory"/> <see cref="Array"/>.</param>
        /// <param name="interpeters">
        /// An <see cref="Array"/> of <see cref="ISpellInterpeter"/> implementing objects used for execution.<br/>
        /// <i>If there are no given, a <see cref="InvalidOperationException"/> is thrown.</i>
        /// </param>
        public SpellReader(Stream source, int stacksize, int funcmemsize, int linememsize, params ISpellInterpeter[] interpeters)
            : this(source, Encoding.UTF8, stacksize, funcmemsize, linememsize, interpeters) { }
        /// <summary>
        /// Create a new <see cref="SpellReader"/> from a specified <paramref name="sourceString"/> <see cref="string"/> and with set values.
        /// </summary>
        /// <param name="sourceString">The full <see cref="string"/> object this <see cref="SpellReader"/> uses to operate.</param>
        /// <param name="stacksize">The size of the <see cref="StackMemory"/> <see cref="Array"/>.</param>
        /// <param name="funcmemsize">The size of the <see cref="FuncMemory"/> <see cref="Array"/>.</param>
        /// <param name="linememsize">The size of the <see cref="LineMemory"/> <see cref="Array"/>.</param>
        /// <param name="interpeters">
        /// An <see cref="Array"/> of <see cref="ISpellInterpeter"/> implementing objects used for execution.<br/>
        /// <i>If there are no given, a <see cref="InvalidOperationException"/> is thrown.</i>
        /// </param>
        public SpellReader(string sourceString, int stacksize, int funcmemsize, int linememsize, params ISpellInterpeter[] interpeters) : this(sourceString.ToStream(), stacksize, funcmemsize, linememsize, interpeters) { }
        /// <summary>
        /// Create a new <see cref="SpellReader"/> from the specified <paramref name="file"/> and with set values.
        /// </summary>
        /// <param name="file">The <see cref="FileInfo"/> this <see cref="SpellReader"/> reads to operate.</param>
        /// <param name="stacksize">The size of the <see cref="StackMemory"/> <see cref="Array"/>.</param>
        /// <param name="funcmemsize">The size of the <see cref="FuncMemory"/> <see cref="Array"/>.</param>
        /// <param name="linememsize">The size of the <see cref="LineMemory"/> <see cref="Array"/>.</param>
        /// <param name="interpeters">
        /// An <see cref="Array"/> of <see cref="ISpellInterpeter"/> implementing objects used for execution.<br/>
        /// <i>If there are no given, a <see cref="InvalidOperationException"/> is thrown.</i>
        /// </param>
        public SpellReader(FileInfo file, int stacksize, int funcmemsize, int linememsize, params ISpellInterpeter[] interpeters) : this(File.ReadAllText(file.FullName), stacksize, funcmemsize, linememsize, interpeters) { }
        /// <summary>
        /// Create a new <see cref="SpellReader"/> from a specified <paramref name="reader"/> and with set values.
        /// </summary>
        /// <param name="reader">The <see cref="TextReader"/> this <see cref="SpellReader"/> reads to operate.</param>
        /// <param name="stacksize">The size of the <see cref="StackMemory"/> <see cref="Array"/>.</param>
        /// <param name="funcmemsize">The size of the <see cref="FuncMemory"/> <see cref="Array"/>.</param>
        /// <param name="linememsize">The size of the <see cref="LineMemory"/> <see cref="Array"/>.</param>
        /// <param name="interpeters">
        /// An <see cref="Array"/> of <see cref="ISpellInterpeter"/> implementing objects used for execution.<br/>
        /// <i>If there are no given, a <see cref="InvalidOperationException"/> is thrown.</i>
        /// </param>
        public SpellReader(TextReader reader, int stacksize, int funcmemsize, int linememsize, params ISpellInterpeter[] interpeters) : this(((StreamReader)reader).BaseStream, stacksize, funcmemsize, linememsize, interpeters) { }

        /// <summary>
        /// Set the a value of a index in memory externaly.
        /// </summary>
        /// <param name="address">The adress indicating in what memory space the value will be set.</param>
        /// <param name="index">The index of the value; Follows the same rules as a c# array where 0 will be the first.)</param>
        /// <param name="value">The new value.</param>
        public void SetAtMemory(SpellMemoryAddress address, int index, object value)
        {
            switch (address)
            {
                case SpellMemoryAddress.StackMemoryAdr:
                    if (stackmem != null) stackmem[index] = value;
                    else throw new SpellScriptMemoryException("stackMemory is not initialized.");
                    break;
                case SpellMemoryAddress.LineMemoryAdr:
                    if (linemem != null) linemem[index] = value;
                    else throw new SpellScriptMemoryException("lineMemory is not initialized.");
                    break;
                case SpellMemoryAddress.FunctionMemoryAdr:
                    if (funcmem != null) funcmem[index] = value;
                    else throw new SpellScriptMemoryException("funcMemory is not initialized.");
                    break;
                default: throw new ArgumentException("Invalid address input. Its probably null.");
            }
        }
        /// <summary>
        /// Doesnt return a refrence but a copy of the target memory in its current state.
        /// </summary>
        /// <param name="address">The adress of the memory to get.</param>
        /// <returns>A copy of the target memory in its current state. (Unlike the the other getters.)</returns>
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
        ///// <summary>
        ///// Get a value from memory.
        ///// </summary>
        ///// <param name="address">The memory space this value lives in.</param>
        ///// <param name="index">The index of the value.</param>
        ///// <returns>The value at the specified index.</returns>
        //public object GetFromMemory(SpellMemoryAddress address, Index index)
        //    => GetMemory(address)[index];
        
        private void Clear(ref object[]? memory)
        {
            if (memory == null) return;
            Array.Clear(memory, 0, memory.Length);
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public IReadOnlyCollection<object> GetReadOnlyMemory(SpellMemoryAddress address)
            => Array.AsReadOnly(GetMemory(address));

        public ref object GetAtMemory(SpellMemoryAddress address, int index)
            => ref GetMemory(address)[index];

        public IReadOnlyCollection<ISpellInterpeter> GetReadOnlyInterpeters()
            => Array.AsReadOnly(GetInterpeters());
        public ref ISpellInterpeter[] GetInterpeters()
            => ref interpeters;
    }
}
