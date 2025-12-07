using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AsitLib.CommandLine
{
    public interface ICommandProvider
    {
        public string Name { get; }
    }

    /// <summary>
    /// Represents a command provider.
    /// </summary>
    public abstract class CommandProvider : ICommandProvider
    {
        public string Name { get; }

        public CommandProvider(string name)
        {
            Name = name;
        }


        //public override string ToString()
        //    => $"{{Namespace: {Namespace}{(NameOfMainCommandMethod is null ? string.Empty : $", NameOfMainCommandMethod: {NameOfMainCommandMethod}")}}}";
    }

    public abstract class CommandGroup : ICommandProvider
    {
        public string Name { get; }

        public string? NameOfMainMethod { get; }

        public CommandGroup(string name, string? nameOfMainMethod = null)
        {
            Name = name;
            NameOfMainMethod = nameOfMainMethod;

            if (nameOfMainMethod is not null && GetType().GetMethod(nameOfMainMethod) is null)
                throw new ArgumentException($"Method with name '{nameOfMainMethod}' not found.", nameof(nameOfMainMethod));
        }

        public override string ToString()
            => $"{{Namespace: {Name}{(NameOfMainMethod is null ? string.Empty : $", NameOfMainCommandMethod: {NameOfMainMethod}")}}}";
    }
}
