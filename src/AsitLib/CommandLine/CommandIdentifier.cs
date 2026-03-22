using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsitLib.CommandLine
{
    public sealed class CommandIdentifier
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
                    ThrowHelpers.ThrowIfInvalidCommandId(value); // works for groups as well.
                    _group = value;
                }
            }
        }

        public ImmutableArray<string> Groups
        {
            get
            {
                if (Group is null)
                    return ImmutableArray<string>.Empty;

                var groups = new List<string>();
                var parts = Group.Split(' ');

                for (int i = 1; i <= parts.Length; i++)
                {
                    groups.Add(string.Join(" ", parts.Take(i)));
                }

                return groups.ToImmutableArray();
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

        public CommandIdentifier(string source)
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

        public override string ToString()
        {
            return $"{Group} {Name}";
        }
    }
}
