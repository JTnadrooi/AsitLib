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
        public ReturnArgs Run([DisallowNull] SpellRunArgs args)
        {
            switch (args.Command.Name) 
            {
                case "intr_add":
                    if (!SpellUtils.Debug.ValidateArgs<ISpellInterpeter>(args, true))
                        throw new SpellScriptException("Invalid command composition.");
                    return ReturnArgs.GetFromObject(AsitGlobal.ArrayAdd(args.Executor!.GetInterpeters(), (ISpellInterpeter)args.Command.Arguments![0]));
                case "intr_get":
                    if (SpellUtils.Debug.ValidateArgs<int>(args, true))
                        return ReturnArgs.GetFromObject(args.Executor!.GetInterpeters()[(int)args.Command.Arguments![0]]);
                    else if (SpellUtils.Debug.ValidateArgs<string>(args, true))
                    {
                        throw new NotImplementedException();
                        //file
                    }
                    else throw new SpellScriptException("Invalid command composition    .");
                    return ReturnArgs.GetFromObject(args.Executor!.GetInterpeters()[((int)args.Command.Arguments![0])]);
                case "intr_set":
                    if (!SpellUtils.Debug.ValidateArgs<int, ISpellInterpeter>(args, true))
                        throw new SpellScriptException("Invalid command composition.");
                    if (args.Executor!.GetInterpeters()[(int)args.Command.Arguments![0]] != null) args.Executor!.GetInterpeters()[(int)args.Command.Arguments![0]] = (ISpellInterpeter)args.Command.Arguments![1];
                    return ReturnArgs.NullSucces;
                case "intr_clear":
                    if (!SpellUtils.Debug.ValidateArgs(args, true))
                        throw new SpellScriptException("Invalid command composition.");
                    args.Executor!.GetInterpeters() = new ISpellInterpeter[args.Executor!.GetInterpeters().Length];
                    return ReturnArgs.NullSucces;

            }
            return ReturnArgs.Empty;
        }
    }
}
