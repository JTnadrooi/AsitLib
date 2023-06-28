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
    public sealed class SpellInterpeterManager : ISpellInterpeter
    {
        public string? Namespace => "&";
        [return: MaybeNull]
        public SpellReturnArgs Run([DisallowNull] SpellRunArgs args)
        {
            switch (args.Command.Name) 
            {
                case "intr_add":
                    if (!SSpell.Debug.ValidateArgs<ISpellInterpeter>(args, true))
                        throw new SpellScriptException("Invalid command composition.");
                    return SpellReturnArgs.GetFromObject(AsitLibStatic.ArrayAdd(args.Executor!.GetInterpeters(), (ISpellInterpeter)args.Command.Arguments![0]));
                case "intr_get":
                    if (SSpell.Debug.ValidateArgs<int>(args, true))
                        return SpellReturnArgs.GetFromObject(args.Executor!.GetInterpeters()[(int)args.Command.Arguments![0]]);
                    else if (SSpell.Debug.ValidateArgs<string>(args, true))
                    {
                        throw new NotImplementedException();
                        //file
                    }
                    else throw new SpellScriptException("Invalid command composition    .");
                    return SpellReturnArgs.GetFromObject(args.Executor!.GetInterpeters()[((int)args.Command.Arguments![0])]);
                case "intr_set":
                    if (!SSpell.Debug.ValidateArgs<int, ISpellInterpeter>(args, true))
                        throw new SpellScriptException("Invalid command composition.");
                    if (args.Executor!.GetInterpeters()[(int)args.Command.Arguments![0]] != null) args.Executor!.GetInterpeters()[(int)args.Command.Arguments![0]] = (ISpellInterpeter)args.Command.Arguments![1];
                    return SpellReturnArgs.NullSucces;
                case "intr_clear":
                    if (!SSpell.Debug.ValidateArgs(args, true))
                        throw new SpellScriptException("Invalid command composition.");
                    args.Executor!.GetInterpeters() = new ISpellInterpeter[args.Executor!.GetInterpeters().Length];
                    return SpellReturnArgs.NullSucces;

            }
            return SpellReturnArgs.Empty;
        }
    }
}
