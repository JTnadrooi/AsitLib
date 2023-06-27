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
    public sealed class SpellInterpeterManager : ISpellInterpeter, ICustomFormatter
    {
        public string? Namespace => "&";
        public string Format(string? format, object? arg, IFormatProvider? formatProvider)
        {
            throw new NotImplementedException();
        }
        [return: MaybeNull]
        public SpellReturnArgs Run([DisallowNull] SpellRunArgs args)
        {
            switch (args.Command.Name) 
            {
                case "":

                    break;
            }
        }
    }
}
