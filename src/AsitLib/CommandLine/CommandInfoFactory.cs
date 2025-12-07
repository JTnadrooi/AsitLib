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
        public TCommandInfo? Convert(TAttribute attribute, CommandProvider provider, MethodInfo methodInfo);
    }

    public static class CommandInfoFactory
    {
        public class DefaultInfoFactory : ICommandInfoFactory<CommandAttribute, ProviderCommandInfo>
        {
            public DefaultInfoFactory() { }

            public ProviderCommandInfo? Convert(CommandAttribute attribute, CommandProvider provider, MethodInfo methodInfo)
            {
                string cmdId;
                if (provider.NameOfMainCommandMethod == methodInfo.Name) cmdId = provider.Namespace;
                else cmdId = (attribute.InheritNamespace ? (provider.Namespace + "-") : string.Empty) + (attribute.Id ?? ParseHelpers.GetSignature(methodInfo));

                return new ProviderCommandInfo(cmdId.ToSingleArray().Concat(attribute.Aliases).ToArray(), attribute, methodInfo, provider);
            }
        }

        public static ICommandInfoFactory<CommandAttribute, CommandInfo> Default { get; } = new DefaultInfoFactory();
    }
}
