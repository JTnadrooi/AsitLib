using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AsitLib.CommandLine
{
    public interface ICommandInfoFactory<in TAttribute, out TCommandInfo> where TAttribute : CommandAttribute where TCommandInfo : CommandInfo
    {
        public TCommandInfo? Convert(TAttribute attribute, ICommandProvider provider, MethodInfo methodInfo);
    }

    public static class CommandInfoFactory
    {
        public class DefaultInfoFactory : ICommandInfoFactory<CommandAttribute, CommandInfo>
        {
            public DefaultInfoFactory() { }

            public CommandInfo? Convert(CommandAttribute attribute, ICommandProvider provider, MethodInfo methodInfo)
            {
                string cmdId;
                switch (provider)
                {
                    case CommandGroup g:
                        if (g.NameOfMainMethod == methodInfo.Name) cmdId = g.Name;
                        else cmdId = provider.Name + "-" + (attribute.Id ?? ParseHelpers.GetSignature(methodInfo));
                        return new CommandGroupCommandInfo(cmdId.ToSingleArray().Concat(attribute.Aliases).ToArray(), attribute, methodInfo, g);
                    case CommandProvider p:
                        cmdId = attribute.Id ?? ParseHelpers.GetSignature(methodInfo);
                        return new ProviderCommandInfo(cmdId.ToSingleArray().Concat(attribute.Aliases).ToArray(), attribute, methodInfo, p);
                    default:
                        throw new Exception();
                }

            }
        }

        public static ICommandInfoFactory<CommandAttribute, CommandInfo> Default { get; } = new DefaultInfoFactory();
    }
}
