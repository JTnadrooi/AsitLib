using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsitLib.Diagnostics
{
    public static class LoggerExtensions
    {
        public static void SuccessIf(this IRichLogger logger, bool succes, string? msg = null)
        {
            if (succes) logger.Success();
            else logger.Fail();
        }

        public static bool IsConsole(this IRichLogger logger) => logger.Out == Console.Out;
    }
}
