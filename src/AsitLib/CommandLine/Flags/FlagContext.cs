using AsitLib.CommandLine;
using AsitLib.CommandLine.Attributes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
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

        public T? GetFlagHandlerArgument<T>(FlagHandler flagHandler)
        {
            if (TryGetFlagHandlerArgument<T>(flagHandler, out T? value)) return value;
            else throw new InvalidOperationException($"No flag argument provided for flag '{flagHandler.LongFormId}'.");
        }

        public bool TryGetFlagHandlerArgument<T>(FlagHandler flagHandler, out T? value)
        {
            foreach (Argument argument in ArgumentInfo.Arguments)
                if (argument.Target.TargetsFlag(flagHandler))
                {
                    value = (T?)ParseHelpers.Convert(argument.Tokens, typeof(T), flagHandler.GetType().GetCustomAttributes(true));
                    return true;
                }
            value = default;
            return false;
        }
    }
}
