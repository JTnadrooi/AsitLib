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
        public ExecutingContextFlags Flags { get; set; }
        internal bool PreCommand { get; set; }

        private List<Func<object?>> _actions;

        internal CommandContext(CommandEngine source, ArgumentsInfo argumentsInfo, bool preCommand)
        {
            ArgumentsInfo = argumentsInfo;
            Source = source;
            Flags = ExecutingContextFlags.None;

            _actions = new List<Func<object?>>();
            PreCommand = preCommand;
        }

        internal object? RunAllActions()
        {
            object? toret = _actions.Aggregate((object?)null, (result, action) => action.Invoke() ?? result);

            return toret;
        }

        internal void ThrowIfNotPreCommand()
        {
            if (!PreCommand) throw new InvalidOperationException("This operaton is invalid after the command has already executed.");
        }

        public bool HasFlag(ExecutingContextFlags flag) => Flags.HasFlag(flag);

        public CommandContext AddFlag(ExecutingContextFlags flag)
        {
            ThrowIfNotPreCommand();
            Flags |= flag;
            return this;
        }

        public CommandContext AddAction(Action action)
        {
            ThrowIfNotPreCommand();
            AddAction(() =>
            {
                action.Invoke();
                return null;
            });
            return this;
        }

        public CommandContext AddAction(Func<object?> action)
        {
            ThrowIfNotPreCommand();
            _actions.Add(action);
            return this;
        }

        public T? GetFlagHandlerArgument<T>(GlobalOption flagHandler)
        {
            if (TryGetFlagHandlerArgument<T>(flagHandler, out T? value)) return value;
            else throw new InvalidOperationException($"No flag argument provided for flag '{flagHandler.LongFormId}'.");
        }

        public bool TryGetFlagHandlerArgument<T>(GlobalOption flagHandler, out T? value)
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
