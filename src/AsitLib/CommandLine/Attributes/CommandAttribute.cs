using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsitLib.CommandLine
{
    [AttributeUsage(AttributeTargets.Method)]
    public class CommandAttribute : Attribute
    {
        /// <summary>
        /// Gets the <see cref="CommandInfo.Id"/>. If <see langword="null"/>, the <see cref="CommandInfo.Id"/> will be the name of the method converted to lowercase using <see cref="string.ToLower()"/>.
        /// </summary>
        public string? Id { get; }
        /// <summary>
        /// Gets the command description. Will be displayed in the <see cref="CLICommandProvider.Help(string?)"/> command.
        /// </summary>
        public string Description { get; }
        /// <summary>
        /// Gets if the <see cref="CommandProvider.FullNamespace"/> should be prefixed to the <see cref="CommandInfo.Id"/>, separated by a dash.
        /// </summary>
        public bool InheritNamespace { get; }
        /// <summary>
        /// Initializes a new instance of the <see cref="CommandAttribute"/> class by using a specified command parameters.
        /// </summary>
        /// <param name="description"><inheritdoc cref="Description" path="/summary"/></param>
        /// <param name="id"><inheritdoc cref="Id" path="/summary"/></param>
        /// <param name="inheritNamespace"><inheritdoc cref="InheritNamespace" path="/summary"/></param>
        public CommandAttribute(
            string description,
            string? id = null,
            bool inheritNamespace = true)
        {
            Id = id;
            Description = description;
            InheritNamespace = inheritNamespace;
        }
    }
}
