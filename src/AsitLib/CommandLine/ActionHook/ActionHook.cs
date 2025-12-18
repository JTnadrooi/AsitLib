namespace AsitLib.CommandLine
{
    public abstract class ActionHook
    {
        public string Id { get; }

        protected ActionHook(string id)
        {
            Id = id;
        }

        /// <summary>
        /// Gets called before the command executes.
        /// </summary>
        /// <param name="context">The command conext, contains information about the currently executing command and <see cref="CommandEngine"/>.</param>
        public virtual void PreCommand(CommandContext context) { }

        /// <summary>
        /// Gets called after the command executes.
        /// </summary>
        /// <param name="context">The command conext, contains information about the currently executing command and <see cref="CommandEngine"/>.</param>
        public virtual void PostCommand(CommandContext context) { }

        /// <summary>
        /// Gets called on the value returned by the executed command. This calls after <see cref="PostCommand(CommandContext)"/>.
        /// </summary>
        /// <param name="context">The command conext, contains information about the currently executing command and <see cref="CommandEngine"/>.</param>
        public virtual object? OnReturned(CommandContext context, object? returned) => returned;
    }
}
