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

        protected ICommandInfoFactory<CommandAttribute, CommandInfo>? InfoFactory { get; }

        public CommandProvider(string name, ICommandInfoFactory<CommandAttribute, CommandInfo>? infoFactory)
        {
            ParseHelpers.ThrowIfInvalidName(name, false, "CommandProvider Name");

            Name = name;
            InfoFactory = infoFactory;
        }

        public virtual CommandInfo[] GetCommands()
        {
            if (InfoFactory is null) throw new InvalidOperationException("Cannot get commands if InfoFactory is null.");

            MethodInfo[] commandMethods = GetType().GetMethods();
            List<CommandInfo> commands = new List<CommandInfo>();

            foreach (MethodInfo methodInfo in commandMethods)
                if (methodInfo.GetCustomAttribute<CommandAttribute>() is CommandAttribute attribute)
                {
                    CommandInfo? info = InfoFactory.Convert(attribute, this, methodInfo);
                    if (info is not null) commands.Add(info);
                }

            return commands.ToArray();
        }

        //public override string ToString()
        //    => $"{{Namespace: {Namespace}{(NameOfMainCommandMethod is null ? string.Empty : $", NameOfMainCommandMethod: {NameOfMainCommandMethod}")}}}";
    }

    public abstract class CommandGroup : CommandProvider
    {
        public string? NameOfMainMethod { get; }

        public CommandGroup(string name, ICommandInfoFactory<CommandAttribute, CommandInfo>? infoFactory = null, string? nameOfMainMethod = null) : base(name, infoFactory)
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
