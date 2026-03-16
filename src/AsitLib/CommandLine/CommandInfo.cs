using AsitLib.Diagnostics;
using System.Collections.ObjectModel;
using System.Reflection;

namespace AsitLib.CommandLine
{
    public class DelegateCommandInfo : MethodCommandInfo
    {
        public Delegate Delegate { get; }

        public DelegateCommandInfo(string[] ids, string description, Delegate @delegate)
            : base(ids, description, @delegate.Method)
        {
            Target = @delegate.Target;
            Delegate = @delegate;
        }
    }

    public class MethodCommandInfo : CommandInfo
    {
        public MethodInfo MethodInfo { get; }
        public object? Target { get; init; }
        public bool IsVoid => MethodInfo.ReturnType == typeof(void);

        public MethodCommandInfo(string[] ids, string description, MethodInfo methodInfo)
            : base(ids, description)
        {
            MethodInfo = methodInfo;

            if (methodInfo.ReturnType == typeof(DBNull)) throw new ArgumentException("Source MethodInfo cannot return use type DBNull, use void instead. Use object if the method may return void, or a return value.", nameof(methodInfo));
        }

        public static MethodCommandInfo FromProvider(string[] ids, string description, MethodInfo methodInfo, CommandProvider provider)
            => new MethodCommandInfo(ids, description, methodInfo)
            {
                Target = provider,
            };

        public static MethodCommandInfo FromMethod(MethodInfo methodInfo, object? target = null, bool autoProvider = true)
            => FromMethodImpl(methodInfo, autoProvider ? target as CommandProvider : null, target);
        public static MethodCommandInfo FromMethod(MethodInfo methodInfo, CommandProvider provider)
            => FromMethodImpl(methodInfo, provider, provider);
        public static MethodCommandInfo FromMethod(MethodInfo methodInfo, CommandProvider provider, object? target)
            => FromMethodImpl(methodInfo, provider, target);

        private static MethodCommandInfo FromMethodImpl(MethodInfo methodInfo, CommandProvider? provider, object? target)
        {
            CommandAttribute? attribute = methodInfo.GetCustomAttribute<CommandAttribute>();

            if (attribute is null) throw new InvalidOperationException("Source method does not have a CommandAttribute derived attribute.");

            string cmdId;
            switch (provider)
            {
                case CommandGroup g:
                    if (g.NameOfMainMethod == methodInfo.Name) cmdId = g.Name;
                    else cmdId = $"{provider.Name} {(attribute.Id ?? ParseHelpers.GetSignature(methodInfo))}";
                    break;
                default:
                    cmdId = attribute.Id ?? ParseHelpers.GetSignature(methodInfo);
                    break;
            }

            List<string> commandIds = new List<string>([cmdId]);
            commandIds.AddRange(attribute.Aliases);

            return new MethodCommandInfo(commandIds.ToArray(), attribute.Description, methodInfo)
            {
                Provider = provider,
                PassingPolicies = attribute.PassingPolicies,
                Target = target,
            };
        }

        public override OptionInfo[] GetOptions() => MethodInfo.GetParameters().Select(p => new OptionInfo(p)).ToArray();
        public override object? Invoke(object?[] parameters)
        {
            object? result = MethodInfo.Invoke(Target, parameters); // so always runs.

            return IsVoid ? DBNull.Value : result;
        }
    }

    /// <summary>
    /// Represents info for a specific command. See <see cref="CommandAttribute"/>.
    /// </summary>
    public abstract class CommandInfo : IValidatable
    {
        /// <summary>
        /// Gets a <see cref="HashSet{T}"/> containing all strings this command belongs to. This includes generic flags, aliasses, etc.
        /// </summary>
        public IReadOnlyList<string> Ids { get; }

        public IReadOnlyList<string> Groups { get; }

        /// <summary>
        /// Gets if the command represented by this instance has aliases. Generic flag ids count as aliases.
        /// <code>Ids.Count > 1</code>
        /// </summary>
        public bool HasAliases => Ids.Count > 1;

        /// <summary>
        /// Gets the main id. (The first one.)
        /// </summary>
        public string Id => Ids[0];

        /// <summary>
        /// Gets the group this command belongs to.
        /// </summary>
        public string? Group { get; }

        public CommandProvider? Provider { get; init; }

        public OptionPassingPolicies PassingPolicies { get; init; }

        /// <summary>
        /// <inheritdoc cref="CommandAttribute.Description" path="/summary"/>
        /// </summary>
        public string Description { get; }

        public bool IsEnabled { get; set; }

        public CommandInfo(string[] ids, string description)
        {
            ArgumentNullException.ThrowIfNull(ids);
            ArgumentNullException.ThrowIfNull(description);

            if (ids.Length == 0) throw new InvalidOperationException("Command must have at least one id.");

            string mainId = ids[0];
            HashSet<string> seenIds = new HashSet<string>();
            HashSet<string> seenGroups = new HashSet<string>();
            bool validGroup = true;

            foreach (string id in ids)
            {
                ThrowHelpers.ThrowIfInvalidCommandId(id);

                if (!seenIds.Add(id)) throw new InvalidOperationException("Duplicate command id's are invalid.");

                CommandIdentifier commandId = new CommandIdentifier(id);

                if (commandId.Group is not null)
                    seenGroups.Add(commandId.Group);
                else validGroup = false;
            }

            if (validGroup && seenGroups.Count == 1)
            {
                Group = seenGroups.Single();
            }
            else
                Group = null;

            Groups = seenGroups.ToArray();

            Ids = ids.ToArray();
            Description = description;
            IsEnabled = true;
        }

        public abstract object? Invoke(object?[] parameters);
        public abstract OptionInfo[] GetOptions();

        public virtual InvalidReason[] GetInvalidReasons()
        {
            List<InvalidReason> invalidReasons = new List<InvalidReason>();

            //if (HasGenericFlagId && GetOptions().Count(o => !o.HasDefaultValue) > 0) invalidReasons.Add("Generic flags are not supported for commands with required options.");

            return invalidReasons.ToArray();
        }

        public virtual string GetHelpString()
        {
            StringBuilder sb = new StringBuilder(Id).Append(" ");
            OptionInfo[] options = GetOptions();

            if (HasAliases) sb.Append($"[{Ids.Skip(1).ToJoinedString(", ")}] ");

            if (options.Length != 0)
            {
                string optionsString = GetOptions().Select(p =>
                    $"{p.OptionType.Name.ToLower()}:{p.Id!.ToLower()}{(p.HasDefaultValue ? $"(default_value:{p.DefaultValue?.ToString() ?? StringHelpers.NULL_STRING}) " : " ")}")
                    .ToJoinedString();

                sb.Append(optionsString);
            }

            sb.Append($"# {Description}");

            return sb.ToString();
        }

        public override string ToString() => $"{{Id: {Id}, Desc: {Description}}}";
    }
}
