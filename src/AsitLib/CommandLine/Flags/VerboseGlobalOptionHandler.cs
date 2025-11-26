using AsitLib.Diagnostics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsitLib.CommandLine
{
    public class VerboseGlobalOptionHandler : GlobalOptionHandler
    {
        private bool _initialVerboseState;

        private Logger _logger;

        public VerboseGlobalOptionHandler(Logger logger) : base("verbose", "Enable verbose logging.", "v")
        {
            _logger = logger;
        }

        public override void PreCommand(CommandContext context)
        {
            _initialVerboseState = _logger.Silent;
            _logger.Silent = false;
        }

        public override void PostCommand(CommandContext context)
        {
            _logger.Silent = _initialVerboseState;
        }
    }
}
