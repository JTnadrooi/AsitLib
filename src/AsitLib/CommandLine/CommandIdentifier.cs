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

        private string _name;
        public string Name
        {
            get => _name;
            set
            {
                if (value.Contains(' ')) throw new ArgumentException("Name cannot contain spaces.", nameof(value));
                ThrowHelpers.ThrowIfInvalidCommandId(value);

                _name = value;
            }
        }

        private bool _isGenericFlag;
        public bool IsGenericFlag => _isGenericFlag;

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

            _isGenericFlag = _name.StartsWith('-');
        }

        public string[] GetGroups()
        {
            if (Group is null)
                return Array.Empty<string>();

            List<string> groups = new List<string>();
            string[] parts = Group.Split(' ');

            for (int i = 1; i <= parts.Length; i++)
            {
                groups.Add(string.Join(" ", parts.Take(i)));
            }

            return groups.ToArray();
        }

        public override bool Equals(object? obj)
            => obj switch
            {
                CommandIdentifier other => ToString() == other.ToString(),
                string str => ToString() == str,
                _ => false
            };

        public override int GetHashCode()
            => ToString().GetHashCode();

        public static bool operator ==(CommandIdentifier a, CommandIdentifier b)
            => a?.ToString() == b?.ToString();

        public static bool operator !=(CommandIdentifier a, CommandIdentifier b)
            => !(a == b);

        public override string ToString()
        {
            return Group is null ? Name : $"{Group} {Name}";
        }

        public static implicit operator CommandIdentifier(string source)
            => new CommandIdentifier(source);

        public static implicit operator string(CommandIdentifier id)
            => id.ToString();
    }
}
