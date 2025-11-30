using AsitLib.Diagnostics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsitLib.CommandLine
{
    public class HelpGlobalOption : GlobalOption
    {
        public HelpGlobalOption() : base("help", "Display command help.", "h")
        {

        }

        public override void PreCommand(CommandContext context)
        {
            //context
            //    .AddFlag(ExecutingContextFlags.FullStop)
            //    .AddAction(() => context.ArgumentsInfo.CommandId);

            context
                .AddFlag(ExecutingContextFlags.FullStop)
                .AddAction(() => context.Engine.Execute("help " + context.ArgumentsInfo.CommandId));
        }
    }
}
