using AsitLib.CommandLine;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsitLib
{
    public class FlagContext
    {
        public ArgumentsInfo ArgumentInfo { get; }
        public CommandEngine Source { get; }

        public FlagContext(CommandEngine source, ArgumentsInfo arguments)
        {
            Source = source;
            ArgumentInfo = arguments;
        }

        public IReadOnlyList<string> GetFlagHandlerArguments(FlagHandler flagHandler)
        {
            foreach (Argument argument in ArgumentInfo.Arguments)
            {
                if ((argument.Target.IsLongForm && argument.Target.SanitizedParameterToken == flagHandler.LongFormId) ||
                    (flagHandler.HasShorthandId && argument.Target.IsShorthand && argument.Target.SanitizedParameterToken == flagHandler.ShorthandId))
                    return argument.Tokens;
            }
            throw new InvalidOperationException();
        }
    }
}
