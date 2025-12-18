using System.Reflection;

namespace AsitLib.CommandLine
{
    /// <summary>
    /// Represents a command provider.
    /// </summary>
    public abstract class CommandProvider
    {
        public string Name { get; }

        protected ICommandInfoFactory InfoFactory { get; }

        public CommandProvider(string name, ICommandInfoFactory? infoFactory = null)
        {
            ParseHelpers.ThrowIfInvalidName(name, false);

            Name = name;
            InfoFactory = infoFactory ?? CommandInfoFactory.Default;
        }

        public virtual CommandInfo[] GetCommands()
        {
            MethodInfo[] commandMethods = GetType().GetMethods();
            List<CommandInfo> commands = new List<CommandInfo>();

            foreach (MethodInfo methodInfo in commandMethods)
                if (methodInfo.GetCustomAttribute<CommandAttribute>() is CommandAttribute attribute)
                {
                    CommandInfo? info = InfoFactory.Convert(this, methodInfo, attribute);
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

        public CommandGroup(string name, ICommandInfoFactory? infoFactory = null, string? nameOfMainMethod = null) : base(name, infoFactory)
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
