using AsitLib.Diagnostics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsitLib.CommandLine
{
    public sealed class VersionGlobalOption : GlobalOption
    {
        private string _versionString;

        public VersionGlobalOption(Version version) : this(version.ToString()) { }
        public VersionGlobalOption(string versionString) : base("version", "Print version.")
        {
            _versionString = versionString;
        }

        public override void PreCommand(CommandContext context)
        {
            context
                .AddFlag(ExecutingContextFlags.FullStop)
                .AddAction(() => _versionString);
        }
    }
}
