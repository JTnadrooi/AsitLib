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
        public string Name { get; }

        public CommandProvider(string name)
        {
            ParseHelpers.ThrowIfInvalidName(name, false, "CommandProvider Name");

            Name = name;
        }

        //public override string ToString()
        //    => $"{{Namespace: {Namespace}{(NameOfMainCommandMethod is null ? string.Empty : $", NameOfMainCommandMethod: {NameOfMainCommandMethod}")}}}";
    }

    public abstract class CommandGroup : CommandProvider
    {
        public string? NameOfMainMethod { get; }

        public CommandGroup(string name, string? nameOfMainMethod = null) : base(name)
        {
            NameOfMainMethod = nameOfMainMethod;

            if (nameOfMainMethod is not null)
            {
                MethodInfo? mainMethod = GetType().GetMethod(nameOfMainMethod);
                if (mainMethod is null)
                    throw new ArgumentException($"(main)Method with name '{nameOfMainMethod}' not found.", nameof(nameOfMainMethod));
                else if (mainMethod.GetCustomAttribute<CommandAttribute>() is null)
                    throw new ArgumentException($"(main)Method with name '{nameOfMainMethod}' does not have a {nameof(CommandAttribute)}.", nameof(nameOfMainMethod));
            }
        }

        public override string ToString()
            => $"{{Namespace: {Name}{(NameOfMainMethod is null ? string.Empty : $", NameOfMainCommandMethod: {NameOfMainMethod}")}}}";
    }
}
