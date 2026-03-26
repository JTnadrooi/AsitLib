namespace AsitLib.CommandLine
{
    public sealed class CommandContext
    {
        public CallInfo Call { get; }
        public CommandEngine Engine { get; }
        public ExecutingContextFlags Flags { get; set; }
        internal bool PreCommand { get; set; }

        private List<Func<object?>> _funcs;

        internal CommandContext(CommandEngine engine, CallInfo argumentsInfo, bool preCommand)
        {
            Call = argumentsInfo;
            Engine = engine;
            Flags = ExecutingContextFlags.None;

            _funcs = new List<Func<object?>>();
            PreCommand = preCommand;
        }

        internal object? RunAll()
        {
            List<object?> results = new List<object?>();

            foreach (Func<object?> action in _funcs)
            {
                object? actionResult = action.Invoke();

                if (actionResult is DBNull) continue;

                results.Add(actionResult);
            }

            if (results.Count == 0) return DBNull.Value;
            else if (results.Count == 1) return results[0];
            else throw new CommandException("Multiple context-action/function returns are invalid.");
        }

        internal void ThrowIfNotPreCommand()
        {
            if (!PreCommand) throw new ArgumentException("This operaton is invalid after the command has already executed.");
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

        public T? GetGlobalOptionValue<T>(GlobalOption globalOption)
        {
            foreach (Argument argument in Call.Arguments)
                if (argument.Target.IsMatchFor(globalOption))
                    return (T?)globalOption.Option.Conform(argument.Tokens.AsSpan());
            throw new KeyNotFoundException($"No option provided for GlobalOption '{globalOption.Id}'.");
        }
    }
}
