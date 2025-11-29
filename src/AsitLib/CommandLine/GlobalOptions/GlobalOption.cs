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
    public abstract class GlobalOption
    {
        public string? ShorthandId { get; }
        public string LongFormId { get; }
        public string Description { get; }
        public bool HasShorthandId => ShorthandId is not null;

        public object? ImplicitValue { get; }

        protected GlobalOption(string longFormId, string description, string? shorthandId = null, object? implicitValue = null)
        {
            ShorthandId = shorthandId;
            LongFormId = longFormId;
            Description = description;

            ImplicitValue = implicitValue;
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

    }

    [Flags]
    public enum ExecutingContextFlags
    {
        None = 0,
        PreventCommand = 1,
        PreventFlags = 2,
        FullStop = PreventCommand | PreventFlags,
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
