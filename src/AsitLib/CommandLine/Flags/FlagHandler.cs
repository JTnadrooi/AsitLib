using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsitLib.CommandLine
{
    /// <summary>
    /// Represents a command flag listener and handler.
    /// </summary>
    public abstract class FlagHandler
    {
        public string? ShorthandId { get; }
        public string LongFormId { get; }
        public string Description { get; }
        public bool HasShorthandId => ShorthandId != null;

        public FlagHandler(string longFormId, string description, string? shorthandId = null)
        {
            ShorthandId = shorthandId;
            LongFormId = longFormId;
            Description = description;
        }

        /// <summary>
        /// Gets called before the command executes. Only calls if the <see cref="ShouldListen(ArgumentsInfo)"/> returns <see langword="true"/>.
        /// </summary>
        /// <param name="arguments">The command arguments. Do not check if this flag is present, that is done by the <see cref="ShouldListen(ArgumentsInfo)"/> method.</param>
        public virtual void PreCommand(FlagContext context) { }

        /// <summary>
        /// Gets called after the command executes. Only calls if the <see cref="ShouldListen(ArgumentsInfo)"/> returns <see langword="true"/>.
        /// </summary>
        /// <param name="arguments">The command arguments. Do not check if this flag is present, that is done by the <see cref="ShouldListen(ArgumentsInfo)"/> method.</param>
        public virtual void PostCommand(FlagContext context) { }

        public virtual object? OnReturned(FlagContext context, object? returned) => returned;
    }
}
