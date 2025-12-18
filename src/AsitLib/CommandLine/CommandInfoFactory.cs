using System.Reflection;

namespace AsitLib.CommandLine
{
    public interface ICommandInfoFactory
    {
        public CommandInfo? Convert(CommandProvider provider, MethodInfo methodInfo, CommandAttribute attribute);
    }

    public static class CommandInfoFactory
    {
        public class DefaultInfoFactory : ICommandInfoFactory
        {
            public DefaultInfoFactory() { }

            public CommandInfo? Convert(CommandProvider provider, MethodInfo methodInfo, CommandAttribute attribute)
            {
                return MethodCommandInfo.FromMethod(methodInfo, provider);
            }
        }

        public static ICommandInfoFactory Default { get; } = new DefaultInfoFactory();
    }
}
