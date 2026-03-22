using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsitLib.CommandLine
{
    public class CommandNotFoundException : CommandException
    {
        public string CommandId { get; }

        public CommandNotFoundException(string message, string commandId) : base(message)
        {
            CommandId = commandId;
        }
    }
}
