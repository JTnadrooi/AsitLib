using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsitLib.CommandLine
{
    public class ParseException : CommandException
    {
        public ParseException(string message) : base(message) { }
    }
}
