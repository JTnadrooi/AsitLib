using AsitLib.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsitLib.CommandLine
{
    public class VerboseFlagHandler : FlagHandler
    {
        private bool _initialVerboseState;

        private Logger _logger;

        public VerboseFlagHandler(Logger logger) : base("verbose", "Enable verbose logging.", "v")
        {
            _logger = logger;
        }

        public override void PreCommand(FlagContext context)
        {
            _initialVerboseState = _logger.Silent;
            _logger.Silent = false;
        }

        public override void PostCommand(FlagContext context)
        {
            _logger.Silent = _initialVerboseState;
        }
    }
}
