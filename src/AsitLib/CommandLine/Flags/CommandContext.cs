using AsitLib.CommandLine;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AsitLib
{
    public sealed class CommandContext
    {
        public ArgumentsInfo ArgumentsInfo { get; internal set; }
        public CommandEngine Source { get; internal set; }
        public ExecutingContext? ExecutingContext { get; internal set; }

        internal CommandContext(CommandEngine source, ArgumentsInfo argumentsInfo, ExecutingContext? executingContext = null)
        {
            ArgumentsInfo = argumentsInfo;
            Source = source;
            ExecutingContext = executingContext;
        }

        public T? GetFlagHandlerArgument<T>(GlobalOptionHandler flagHandler)
        {
            if (TryGetFlagHandlerArgument<T>(flagHandler, out T? value)) return value;
            else throw new InvalidOperationException($"No flag argument provided for flag '{flagHandler.LongFormId}'.");
        }

        public bool TryGetFlagHandlerArgument<T>(GlobalOptionHandler flagHandler, out T? value)
        {
            foreach (Argument argument in ArgumentsInfo.Arguments)
                if (argument.Target.TargetsFlag(flagHandler))
                {
                    value = (T?)ParseHelpers.GetValue(argument.Tokens, typeof(T), flagHandler.GetType().GetCustomAttributes(true).Cast<Attribute>(), flagHandler.ImplicitValue);
                    return true;
                }
            value = default;
            return false;
        }
    }
}
