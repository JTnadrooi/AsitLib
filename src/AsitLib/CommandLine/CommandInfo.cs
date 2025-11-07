using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AsitLib.CommandLine
{
    /// <summary>
    /// Represents info for a specific command. See <see cref="CommandAttribute"/>.
    /// </summary>
    public class CommandInfo
    {
        public string Id { get; }

        /// <summary>
        /// <inheritdoc cref="CommandAttribute.Description" path="/summary"/>
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Gets the <see cref="System.Reflection.MethodInfo"/> this <see cref="CommandInfo"/> is created from.
        /// </summary>
        public MethodInfo MethodInfo { get; }
        public CommandProvider Provider { get; }

        /// <summary>
        /// Gets a value indicating if this command is the main command. Main commands inherit their <see cref="Id"/> from the source <see cref="CommandProvider.FullNamespace"/>.
        /// </summary>
        public bool IsMain { get; }

        public CommandInfo(string id, string description, MethodInfo methodInfo, CommandProvider provider)
        {
            Id = id;
            Description = description;
            MethodInfo = methodInfo;
            Provider = provider;
            IsMain = methodInfo.Name == CommandEngine.MAIN_COMMAND_ID;
        }

        public override string ToString() => $"{{Id: {Id}, Method: {MethodInfo}, Provider: {Provider?.ToString()}}}";
    }
}
