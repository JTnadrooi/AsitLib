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
    /// A <see cref="ISpellInterpeter"/> for running cmd commands. Be aware, this grants the spellscript almost <see cref="float.PositiveInfinity"/> power!
    /// </summary>
    public sealed class SpellBatManager : ISpellInterpeter
    {
        public string? Namespace => "bat";

        public SpellReturnArgs Run([DisallowNull] SpellRunArgs args)
        {
            switch (args.Command.Name)
            {
                case "cmd":
                    if (!SSpell.Debug.ValidateArgs<string>(args, true))
                        throw new SpellScriptException("Invalid command composition: " + args);
                    else
                    {
                        ProcessStartInfo ProcessInfo = new ProcessStartInfo("cmd.exe", "/C " + args.Command.Arguments![0])
                        {
                            CreateNoWindow = true,
                            UseShellExecute = true,
                        };
                        Process process = new Process()
                        {
                            StartInfo = ProcessInfo,
                        };
                        StreamReader output = process.StandardOutput;
                        process.Start();
                        process.WaitForExit();
                        output.BaseStream.Position = 0;
                        return SpellReturnArgs.GetFromObject(output.ReadToEnd());
                    }
                //case "scriptstart":
                //    if (!SpellUtils.Debug.ValidateArgs<string>(args, true))
                //        throw new SpellScriptException("Invalid command build: " + args);
                //    else
                //    {

                //    }
                default: throw new SpellScriptException("Invalid command pointing to batch: " + args.Command);

            }
        }
    }
}
