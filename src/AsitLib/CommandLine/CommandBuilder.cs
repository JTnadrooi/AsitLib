using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AsitLib.CommandLine
{
    public sealed class CommandBuilder
    {
        public CommandBuilder()
        {

        }

        public CommandBuilder Register(MethodInfo method)
        {
            return this;
        }

        public CommandBuilder Register(string sourceId, string targetId)
        {
            return this;
        }
    }
}
