using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
#nullable enable

namespace AsitLib.SpellScript
{
    /// <summary>
    /// Default manager of all the base spell script actions. This <see cref="ISpellInterpeter"/> is always included when a 
    /// spellscript is ran via any of the <see cref="SpellReader.Run(Action{SpellBlockReturnArgs}?, string?, string?, object[])"/> and <see cref="SpellReader.RunAsync(Action{SpellBlockReturnArgs}?, string?, string?, object[])"/> overloads.
    /// This class cannot be inherited.
    /// </summary>
    public sealed class SpellMemoryManager : ISpellInterpeter
    {
        public string? Namespace => "&";
        public SpellReturnArgs Run([DisallowNull] SpellRunArgs args)
        {
            ref object[] memory = ref args.Executor!.GetMemory(args.Command.Name.Split('_')[0] switch
            {
                "stack" => SpellMemoryAddress.StackMemoryAdr,
                "line" => SpellMemoryAddress.LineMemoryAdr,
                "func" => SpellMemoryAddress.FunctionMemoryAdr,
                _ => throw new SpellScriptException()
            });
            switch (args.Command.Name.Split('_')[1])
            {
                case "set":
                    if (!SSpell.Debug.ValidateArgs<int, object>(args, true))
                        throw new SpellScriptException("Invalid command argument count.");
                    memory[(int)args.Command.Arguments![0]] = args.Command.Arguments[1];
                    return SpellReturnArgs.NullSucces;
                case "get":
                    if (!SSpell.Debug.ValidateArgs<int>(args, true))
                        throw new SpellScriptException("Invalid command argument count.");
                    return SpellReturnArgs.GetFromObject(memory[(int)args.Command.Arguments![0]]);
                case "getsize":
                    if (!SSpell.Debug.ValidateArgs(args, true))
                        throw new SpellScriptException("Invalid command argument count.");
                    return SpellReturnArgs.GetFromObject(memory.Length);
                //case "setsize":
                //    if (!SSpell.Debug.ValidateArgs<int>(args, true))
                //        throw new SpellScriptException("Invalid command argument count.");
                //    memory = new object[(int)args.Command.Arguments![0]];
                //    return SpellReturnArgs.NullSucces;
                case "clear":
                    if (!SSpell.Debug.ValidateArgs(args, true))
                        throw new SpellScriptException("Invalid command argument count.");
                    memory = new object[memory.Length];
                    return SpellReturnArgs.NullSucces;
                default: throw new SpellScriptException(args.Command);
            }
            
        }
    }
}
