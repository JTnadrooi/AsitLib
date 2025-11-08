using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsitLib.CommandLine
{
    /// <summary>
    /// Represents a command provider.
    /// </summary>
    public abstract class CommandProvider
    {
        public string Namespace { get; }

        public CommandProvider(string @namespace)
        {
            Namespace = @namespace;
        }

        public virtual void AddCommands(CommandBuilder builder) { }

        public override string ToString() => $"{{Namespace: {Namespace}}}";
    }
}
