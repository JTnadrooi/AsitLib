namespace AsitLib.CommandLine
{
    /// <summary>
    /// Provides contextual information and behavior for a command during execution.
    /// </summary>
    public sealed class CommandContext
    {
        /// <summary>
        /// Gets information about the current command call.
        /// </summary>
        public CallInfo Call { get; }

        /// <summary>
        /// Gets the <see cref="CommandEngine"/> executing the command.
        /// </summary>
        public CommandEngine Engine { get; }

        private ExecutingContextFlags _flags;
        /// <summary>
        /// Gets or sets execution flags that influence command behavior.
        /// </summary>
        public ExecutingContextFlags Flags
        {
            get => _flags;
            set
            {
                ThrowIfNotPreCommand();
                _flags = value;
            }
        }

        internal bool PreCommand;

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

        /// <summary>
        /// Determines whether the specified <see cref="ExecutingContextFlags"/> flag is set.
        /// </summary>
        /// <param name="flag">The flag to check.</param>
        /// <returns><see langword="true"/> if the flag is set; otherwise, <see langword="false"/>.</returns>
        public bool HasFlag(ExecutingContextFlags flag) => Flags.HasFlag(flag);

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

        public object? GetGlobalOptionValue(GlobalOption globalOption)
        {
            foreach (Argument argument in Call.Arguments)
                if (argument.Target.IsMatchFor(globalOption))
                    return globalOption.Option.Conform(argument.Tokens.AsSpan());
            throw new KeyNotFoundException($"No option provided for GlobalOption '{globalOption.Id}'.");
        }
    }
}
