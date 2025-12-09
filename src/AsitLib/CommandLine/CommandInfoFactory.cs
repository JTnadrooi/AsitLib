using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AsitLib.CommandLine
{
    public interface ICommandInfoFactory
    {
        public CommandInfo? Convert(CommandAttribute attribute, CommandProvider provider, MethodInfo methodInfo);
    }

    public static class CommandInfoFactory
    {
        public class DefaultInfoFactory : ICommandInfoFactory
        {
            public DefaultInfoFactory() { }

            public CommandInfo? Convert(CommandAttribute attribute, CommandProvider provider, MethodInfo methodInfo)
            {
                string cmdId;
                switch (provider)
                {
                    case CommandGroup g:
                        if (g.NameOfMainMethod == methodInfo.Name) cmdId = g.Name;
                        else cmdId = $"{provider.Name} {(attribute.Id ?? ParseHelpers.GetSignature(methodInfo))}";
                        return new CommandGroupCommandInfo(ArrayHelpers.Combine(cmdId, attribute.Aliases), attribute, methodInfo, g);
                    default:
                        cmdId = attribute.Id ?? ParseHelpers.GetSignature(methodInfo);
                        return new MethodCommandInfo(ArrayHelpers.Combine(cmdId, attribute.Aliases), attribute.Description, methodInfo, provider, attribute.IsGenericFlag);
                }

            }
        }

        public static ICommandInfoFactory Default { get; } = new DefaultInfoFactory();
    }
}
