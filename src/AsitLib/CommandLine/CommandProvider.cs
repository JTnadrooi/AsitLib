using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AsitLib.CommandLine
{
    /// <summary>
    /// Represents a command provider.
    /// </summary>
    public abstract class CommandProvider
    {
        public string Namespace { get; }

        public string? NameOfMainCommandMethod { get; }

        public CommandProvider(string @namespace, string? nameOfMainCommandMethod = null)
        {
            Namespace = @namespace;
            NameOfMainCommandMethod = nameOfMainCommandMethod;

            if (nameOfMainCommandMethod is not null && GetType().GetMethod(nameOfMainCommandMethod) is null)
                throw new ArgumentException($"Method with name '{nameOfMainCommandMethod}' not found.", nameof(nameOfMainCommandMethod));
        }

        public virtual void AddCommands(CommandBuilder builder) { }

        public override string ToString()
            => $"{{Namespace: {Namespace}{(NameOfMainCommandMethod is null ? string.Empty : $", NameOfMainCommandMethod: {NameOfMainCommandMethod}")}}}";
    }
}
