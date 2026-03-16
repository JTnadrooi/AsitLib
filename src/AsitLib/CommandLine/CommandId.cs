using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsitLib.CommandLine
{
    public sealed class CommandId
    {
        private string? _group;
        public string? Group
        {
            get => _group;
            set
            {
                if (value is null)
                {
                    _group = null;
                }
                else
                {
                    ThrowHelpers.ThrowIfInvalidCommandId(value);
                    _group = value;
                }
            }
        }

        private string _name;
        public string Name
        {
            get => _name;
            set
            {
                if (value.Contains(' ')) throw new InvalidOperationException("IsolatedId cannot contain spaces.");
                ThrowHelpers.ThrowIfInvalidCommandId(value);

                _name = value;
            }
        }

        //public CommandId() : this(string.Empty) { }

        public CommandId(string source)
        {
            ThrowHelpers.ThrowIfInvalidCommandId(source);

            string[] split = source.Split(' ');

            if (split.Length == 1)
            {
                _name = split[0];
                _group = null;
            }
            else
            {
                _name = split.Last();
                _group = split[..^1].ToJoinedString(' ');
            }
        }

        //public CommandId SetGroup(string? group)
        //{
        //    Group = group;
        //    return this;
        //}

        //public CommandId SetIsolatedId(string isolatedId)
        //{
        //    Name = isolatedId;
        //    return this;
        //}

        public override string ToString()
        {
            return $"{Group} {Name}";
        }
    }
}
