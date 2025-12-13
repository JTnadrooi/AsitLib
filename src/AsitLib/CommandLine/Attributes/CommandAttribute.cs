using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsitLib.CommandLine
{
    /// <summary>
    /// Marks a method as a command and provides related metadata.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class CommandAttribute : Attribute
    {
        /// <summary>
        /// Gets the command identifier. 
        /// If not specified, <see cref="ParseHelpers.GetSignature(System.Reflection.MemberInfo)"/> is used to derive one from the method name.
        /// </summary>
        public string? Id { get; init; }

        /// <summary>
        /// Gets the description.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Gets alternative identifiers for invoking the command.
        /// </summary>
        public string[] Aliases { get; init; }

        public bool IsGenericFlag { get; init; }

        public OptionPassingPolicies PassingPolicies { get; init; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandAttribute"/> with the specified <see cref="Description"/>.
        /// </summary>
        /// <param name="desc">The <see cref="Description"/>.</param>
        public CommandAttribute(string desc)
        {
            Description = desc;
            Aliases = Array.Empty<string>();
        }
    }
}
