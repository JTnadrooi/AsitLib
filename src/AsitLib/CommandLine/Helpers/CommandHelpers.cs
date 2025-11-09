using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AsitLib.CommandLine
{
    public static class CommandHelpers
    {
        public static string CreateCommandId(CommandAttribute attribute, CommandProvider provider, MethodInfo methodInfo)
        {
            if (methodInfo.Name == "_M") return provider.Namespace;
            else return (attribute.InheritNamespace ? (provider.Namespace + "-") : string.Empty) + (attribute.Id ?? ParseHelpers.ParseSignature(methodInfo));
        }
    }
}
