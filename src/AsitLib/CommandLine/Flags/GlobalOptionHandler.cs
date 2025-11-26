using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsitLib.CommandLine
{
    /// <summary>
    /// Represents a global option listener.
    /// </summary>
    public abstract class GlobalOptionHandler
    {
        public string? ShorthandId { get; }
        public string LongFormId { get; }
        public string Description { get; }
        public bool HasShorthandId => ShorthandId is not null;

        protected GlobalOptionHandler(string longFormId, string description, string? shorthandId = null)
        {
            ShorthandId = shorthandId;
            LongFormId = longFormId;
            Description = description;
        }

        /// <summary>
        /// Gets called before the command executes. Only calls if the <see cref="ShouldListen(ArgumentsInfo)"/> returns <see langword="true"/>.
        /// </summary>
        /// <param name="context">The </param>
        public virtual void PreCommand(CommandContext context) { }

        /// <summary>
        /// Gets called after the command executes. Only calls if the <see cref="ShouldListen(ArgumentsInfo)"/> returns <see langword="true"/>.
        /// </summary>
        /// <param name="arguments">The command arguments. Do not check if this flag is present, that is done by the <see cref="ShouldListen(ArgumentsInfo)"/> method.</param>
        public virtual void PostCommand(CommandContext context) { }

        public virtual object? OnReturned(CommandContext context, object? returned) => returned;

        public virtual ExecutingContext GetExecutingContext(CommandContext context) => ExecutingContext.Default;
    }

    [Flags]
    public enum ExecutingContextFlags
    {
        None = 0,
        PreventCommand = 1,
        PreventFlags = 2,
    }

    public readonly struct ExecutingContext
    {
        public readonly ExecutingContextFlags Flags { get; init; }

        public ExecutingContext() { }

        public readonly ExecutingContext Layer(ExecutingContext other)
        {
            return new ExecutingContext()
            {
                Flags = other.Flags | Flags
            };
        }

        public readonly bool HasFlag(ExecutingContextFlags flag) => Flags.HasFlag(flag);

        public static ExecutingContext Default { get; } = new ExecutingContext() { Flags = ExecutingContextFlags.None };
        public static ExecutingContext PreventAll { get; } = new ExecutingContext() { Flags = ExecutingContextFlags.PreventCommand | ExecutingContextFlags.PreventFlags };
    }
    ///// <summary>
    ///// Represents a command flag listener and handler with input of type <see cref="TInput"/>.
    ///// </summary>
    ///// <typeparam name="TInput">The input type.</typeparam>
    //public abstract class FlagHandler<TInput> : FlagHandler
    //{
    //	/// <inheritdoc cref="FlagHandler.FlagHandler(string, string, string?)"/>
    //	protected FlagHandler(string longFormId, string description, string? shorthandId = null) : base(longFormId, description, shorthandId)
    //	{
    //	}

    //	public override object? OnReturned(FlagContext context, object? returned) => OnReturned(context, context.GetFlagHandlerArgument<TInput>(this), returned);
    //	public override void PreCommand(FlagContext context) => PreCommand(context, context.GetFlagHandlerArgument<TInput>(this));
    //	public override void PostCommand(FlagContext context) => PostCommand(context, context.GetFlagHandlerArgument<TInput>(this));

    //	public virtual void PreCommand(FlagContext context, TInput? input) { }
    //	public virtual void PostCommand(FlagContext context, TInput? input) { }
    //	public virtual object? OnReturned(FlagContext context, TInput? input, object? returned) => returned;
    //}
}
