using AsitLib.CommandLine;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AsitLib.CommandLine
{
    public sealed class CommandContext
    {
        public ArgumentsInfo ArgumentsInfo { get; }
        public CommandEngine Engine { get; }
        public CommandInfo Command { get; }
        public ExecutingContextFlags Flags { get; set; }
        internal bool PreCommand { get; set; }

        private List<Func<object?>> _funcs;

        internal CommandContext(CommandEngine engine, ArgumentsInfo argumentsInfo, bool preCommand, CommandInfo command)
        {
            ArgumentsInfo = argumentsInfo;
            Engine = engine;
            Flags = ExecutingContextFlags.None;
            Command = command;

            _funcs = new List<Func<object?>>();
            PreCommand = preCommand;
        }

        internal object?[] RunAll()
        {
            List<object?> results = new List<object?>();

            foreach (Func<object?> action in _funcs)
            {
                object? actionResult = action.Invoke();

                if (actionResult is DBNull) continue;

                results.Add(actionResult);
            }

            return results.ToArray();
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

        public CommandContext AddAction(Action action) => AddFunction(() =>
            {
                action.Invoke();
                return DBNull.Value;
            });

        public CommandContext AddFunction(Func<object?> func)
        {
            ThrowIfNotPreCommand();
            _funcs.Add(func);
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
