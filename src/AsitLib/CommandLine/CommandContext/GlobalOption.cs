namespace AsitLib.CommandLine
{
    /// <summary>
    /// Represents a global option listener.
    /// </summary>
    public abstract class GlobalOption : ActionHook
    {
        public string Description { get; }

        public OptionInfo Option { get; }

        protected GlobalOption(OptionInfo option, string description, string? id = null) : base(id ?? option.Id)
        {
            ArgumentNullException.ThrowIfNull(option);
            ArgumentNullException.ThrowIfNullOrEmpty(description);

            if (option.Id == OptionInfo.IdForUnnamedOptions) throw new ArgumentException("Missing Id.", nameof(option));

            Description = description;
            Option = option;
        }

        public sealed override void PreCommand(CommandContext context)
            => PreCommand(context, context.GetGlobalOptionValue(this));
        /// <inheritdoc cref="ActionHook.PreCommand(CommandContext)"/>
        /// <param name="optionValue">
        /// The value parsed from the argument matching the <see cref="GlobalOption.Option"/>.
        /// </param>
        protected virtual void PreCommand(CommandContext context, object? optionValue) { }

        public sealed override void PostCommand(CommandContext context)
            => PostCommand(context, context.GetGlobalOptionValue(this));
        /// <inheritdoc cref="ActionHook.PostCommand(CommandContext)"/>
        /// <param name="optionValue">
        /// The value parsed from the argument matching the <see cref="GlobalOption.Option"/>.
        /// </param>
        protected virtual void PostCommand(CommandContext context, object? optionValue) { }

        public sealed override object? OnReturned(CommandContext context, object? returned)
            => OnReturned(context, context.GetGlobalOptionValue(this), returned);
        /// <inheritdoc cref="ActionHook.OnReturned(CommandContext, object?)"/>
        /// <param name="optionValue">
        /// The value parsed from the argument matching the <see cref="GlobalOption.Option"/>.
        /// </param>
        protected virtual object? OnReturned(CommandContext context, object? optionValue, object? returned)
            => returned;
    }

    /// <summary>
    /// Controls what executes when a command gets called.
    /// </summary>
    [Flags]
    public enum ExecutingContextFlags
    {
        /// <summary>
        /// No restrictions.
        /// </summary>
        None = 0,

        /// <summary>
        /// Prevents the command itself from executing.
        /// </summary>
        PreventCommand = 1 << 0,

        /// <summary>
        /// Prevents flags (such as global options/hooks) from executing.
        /// </summary>
        PreventFlags = 1 << 1,

        /// <summary>
        /// Stops both command and flags execution.
        /// </summary>
        FullStop = PreventCommand | PreventFlags,
    }
}
