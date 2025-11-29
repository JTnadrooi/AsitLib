using AsitLib.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsitLib.CommandLine
{
    public static class LoggerExtensions
    {
        public static VerboseGlobalOption GetVerboseFlagHandler(this Logger logger) => new VerboseGlobalOption(logger);
    }
}
