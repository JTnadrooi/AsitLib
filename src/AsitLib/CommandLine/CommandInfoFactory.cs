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
